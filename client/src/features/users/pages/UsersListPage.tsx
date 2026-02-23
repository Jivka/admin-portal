import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Button,
  Autocomplete,
  TextField,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  IconButton,
  Collapse,
  Switch,
  Snackbar,
  Alert,
  Chip,
  CircularProgress,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import { useAppDispatch, useAppSelector } from '../../../store/hooks';
import { fetchUsers, fetchTenantUsers, toggleUserStatus } from '../../../store/usersSlice';
import { fetchTenants, fetchMyTenants } from '../../../store/tenantsSlice';
import { CreateUserDialog, EditUserDialog, DeleteConfirmationDialog } from '../components/index';
import type { UserOutput, TenantOutput } from '../../../types';

const SYSTEM_ADMIN = 1;
const TENANT_ADMIN = 2;

const UsersListPage = () => {
  const dispatch = useAppDispatch();
  const { user } = useAppSelector((state) => state.auth);
  const { users, isLoading, error, pagination } = useAppSelector((state) => state.users);
  const { tenants } = useAppSelector((state) => state.tenants);
  
  // Extract all role IDs from user's tenant roles
  const userRoleIds = user?.tenantRoles?.map(tr => tr.roleId).filter((id): id is number => id !== null && id !== undefined) || [];
  const isSystemAdmin = userRoleIds.includes(SYSTEM_ADMIN);
  const isTenantAdmin = userRoleIds.includes(TENANT_ADMIN);

  const [expandedRow, setExpandedRow] = useState<number | null>(null);
  const [searchName, setSearchName] = useState('');
  const [selectedTenant, setSelectedTenant] = useState<TenantOutput | null>(null);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success',
  });
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<UserOutput | null>(null);

  // Load tenants for autocomplete
  useEffect(() => {
    if (isSystemAdmin) {
      dispatch(fetchTenants({ page: 1, size: 1000 })); // Load all tenants for filter
    } else if (isTenantAdmin) {
      dispatch(fetchMyTenants()); // Load only tenant admin's tenants
    }
  }, [dispatch, isSystemAdmin, isTenantAdmin]);

  // Load users based on role and filters
  useEffect(() => {
    const tenantId = selectedTenant?.tenantId;
    if (isSystemAdmin) {
      dispatch(fetchUsers({ 
        tenantId, 
        page: page + 1, 
        size: rowsPerPage, 
        name: searchName || undefined 
      }));
    } else if (isTenantAdmin) {
      dispatch(fetchTenantUsers({ 
        tenantId, 
        page: page + 1, 
        size: rowsPerPage, 
        name: searchName || undefined 
      }));
    }
  }, [dispatch, isSystemAdmin, isTenantAdmin, page, rowsPerPage, searchName, selectedTenant]);

  const handleExpandClick = (userId?: number) => {
    setExpandedRow(expandedRow === userId ? null : userId || null);
  };

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage);
    setExpandedRow(null);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
    setExpandedRow(null);
  };

  const handleSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSearchName(event.target.value);
    setPage(0);
  };

  const handleTenantFilterChange = (_event: React.SyntheticEvent, value: TenantOutput | null) => {
    setSelectedTenant(value);
    setPage(0);
  };

  const handleStatusToggle = async (user: UserOutput) => {
    if (!isSystemAdmin || !user.userId) return;
    
    try {
      const tenantId = user.tenantRoles?.[0]?.tenantId ?? undefined;
      await dispatch(toggleUserStatus({ 
        userId: user.userId, 
        active: !user.active, 
        isSystemAdmin,
        tenantId 
      })).unwrap();
      setSnackbar({
        open: true,
        message: `User ${user.active ? 'deactivated' : 'activated'} successfully`,
        severity: 'success',
      });
    } catch (err) {
      setSnackbar({
        open: true,
        message: `Failed to toggle user status: ${err}`,
        severity: 'error',
      });
    }
  };

  const handleOpenCreateDialog = () => {
    setCreateDialogOpen(true);
  };

  const handleOpenEditDialog = (user: UserOutput) => {
    setSelectedUser(user);
    setEditDialogOpen(true);
  };

  const handleOpenDeleteDialog = (user: UserOutput) => {
    setSelectedUser(user);
    setDeleteDialogOpen(true);
  };

  const handleDialogSuccess = (message: string) => {
    setSnackbar({
      open: true,
      message,
      severity: 'success',
    });
    // Refresh the list
    const tenantId = selectedTenant?.tenantId;
    if (isSystemAdmin) {
      dispatch(fetchUsers({ tenantId, page: page + 1, size: rowsPerPage, name: searchName || undefined }));
    } else if (isTenantAdmin) {
      dispatch(fetchTenantUsers({ tenantId, page: page + 1, size: rowsPerPage, name: searchName || undefined }));
    }
  };

  const handleCloseSnackbar = () => {
    setSnackbar({ ...snackbar, open: false });
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1">
          Users Management
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreateDialog}>
          Add User
        </Button>
      </Box>

      {error && (
        <Box sx={{ mb: 2 }}>
          <Alert severity="error">{error}</Alert>
        </Box>
      )}

      <Paper sx={{ p: 2, mb: 2 }}>
        <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
          <TextField
            label="Search by Name"
            variant="outlined"
            size="small"
            sx={{ flex: 1, minWidth: 200 }}
            value={searchName}
            onChange={handleSearchChange}
          />
          <Autocomplete
            options={tenants}
            getOptionLabel={(option) => option.tenantName || ''}
            value={selectedTenant}
            onChange={handleTenantFilterChange}
            renderInput={(params) => (
              <TextField {...params} label="Filter by Tenant" size="small" />
            )}
            sx={{ flex: 1, minWidth: 200 }}
            isOptionEqualToValue={(option, value) => option.tenantId === value.tenantId}
          />
        </Box>
      </Paper>

      <TableContainer component={Paper}>
        <Table aria-label="Users list">
          <TableHead>
            <TableRow>
              <TableCell width={50} scope="col" aria-label="Row details" />
              <TableCell scope="col">Name</TableCell>
              <TableCell scope="col">Email</TableCell>
              <TableCell scope="col">Phone</TableCell>
              <TableCell scope="col">Verified</TableCell>
              <TableCell scope="col">Active</TableCell>
              <TableCell scope="col">Created On</TableCell>
              <TableCell scope="col" align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {isLoading ? (
              <TableRow>
                <TableCell colSpan={8} align="center" sx={{ py: 4 }}>
                  <CircularProgress aria-label="Loading users" />
                </TableCell>
              </TableRow>
            ) : users.length === 0 ? (
              <TableRow>
                <TableCell colSpan={8} align="center" sx={{ py: 4 }}>
                  <Typography variant="body2" color="text.secondary">
                    No users found
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              users.map((user) => (
                <React.Fragment key={user.userId}>
                  <TableRow hover>
                    <TableCell>
                      <IconButton
                        size="small"
                        onClick={() => handleExpandClick(user.userId)}
                        aria-label={`${expandedRow === user.userId ? 'Collapse' : 'Expand'} details for ${user.fullName || `${user.firstName} ${user.lastName}`}`}
                        aria-expanded={expandedRow === user.userId}
                        aria-controls={`user-details-${user.userId}`}
                      >
                        {expandedRow === user.userId ? <KeyboardArrowUpIcon /> : <KeyboardArrowDownIcon />}
                      </IconButton>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" fontWeight="medium">
                        {user.fullName || `${user.firstName} ${user.lastName}`}
                      </Typography>
                    </TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>{user.phone || '-'}</TableCell>
                    <TableCell>
                      <Chip 
                        label={user.isVerified ? 'Verified' : 'Not Verified'} 
                        size="small"
                        color={user.isVerified ? 'success' : 'default'}
                      />
                    </TableCell>
                    <TableCell>
                      <Switch
                        checked={user.active || false}
                        onChange={() => handleStatusToggle(user)}
                        disabled={!isSystemAdmin}
                        size="small"
                        inputProps={{ 'aria-label': `${user.active ? 'Deactivate' : 'Activate'} ${user.fullName || user.email}` }}
                      />
                    </TableCell>
                    <TableCell>
                      {user.createdOn ? new Date(user.createdOn).toLocaleString() : '-'}
                    </TableCell>
                    <TableCell align="right">
                      <IconButton size="small" aria-label={`Edit user ${user.fullName || user.email}`} onClick={() => handleOpenEditDialog(user)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                      <IconButton size="small" aria-label={`Delete user ${user.fullName || user.email}`} onClick={() => handleOpenDeleteDialog(user)}>
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell style={{ paddingBottom: 0, paddingTop: 0 }} colSpan={8} id={`user-details-${user.userId}`}>
                      <Collapse in={expandedRow === user.userId} timeout="auto" unmountOnExit>
                        <Box sx={{ margin: 2 }}>
                          <Table size="small" aria-label={`Tenant roles for ${user.fullName || `${user.firstName} ${user.lastName}`}`}>
                            <TableHead>
                              <TableRow>
                                <TableCell scope="col">Tenant</TableCell>
                                <TableCell scope="col">Role</TableCell>
                              </TableRow>
                            </TableHead>
                            <TableBody>
                              {user.tenantRoles && user.tenantRoles.length > 0 ? (
                                user.tenantRoles.map((tr, idx) => (
                                  <TableRow key={idx}>
                                    <TableCell>{tr.tenantName}</TableCell>
                                    <TableCell>{tr.roleDisplayName || tr.roleName}</TableCell>
                                  </TableRow>
                                ))
                              ) : (
                                <TableRow>
                                  <TableCell colSpan={2} align="center">
                                    <Typography variant="body2" color="text.secondary">
                                      No tenant roles assigned
                                    </Typography>
                                  </TableCell>
                                </TableRow>
                              )}
                            </TableBody>
                          </Table>
                        </Box>
                      </Collapse>
                    </TableCell>
                  </TableRow>
                </React.Fragment>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <TablePagination
        rowsPerPageOptions={[5, 10, 25, 50]}
        component="div"
        count={pagination.count}
        rowsPerPage={rowsPerPage}
        page={page}
        onPageChange={handleChangePage}
        onRowsPerPageChange={handleChangeRowsPerPage}
      />

      <CreateUserDialog
        open={createDialogOpen}
        onClose={() => setCreateDialogOpen(false)}
        onSuccess={handleDialogSuccess}
        isSystemAdmin={isSystemAdmin}
      />

      <EditUserDialog
        open={editDialogOpen}
        onClose={() => {
          setEditDialogOpen(false);
          setSelectedUser(null);
        }}
        onSuccess={handleDialogSuccess}
        user={selectedUser}
        isSystemAdmin={isSystemAdmin}
      />

      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        onClose={() => {
          setDeleteDialogOpen(false);
          setSelectedUser(null);
        }}
        onSuccess={handleDialogSuccess}
        user={selectedUser}
        isSystemAdmin={isSystemAdmin}
      />

      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={handleCloseSnackbar}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert
          onClose={handleCloseSnackbar}
          severity={snackbar.severity}
          sx={{ width: '100%' }}
          role={snackbar.severity === 'success' ? 'status' : 'alert'}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default UsersListPage;
