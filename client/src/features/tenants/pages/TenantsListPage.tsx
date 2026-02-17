import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Button,
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
import { fetchTenants, fetchMyTenants, toggleTenantStatus } from '../../../store/tenantsSlice';
import { getTenantTypeLabel, getOwnershipLabel } from '../../../utils';
import { ContactsList } from '../../../components/common';
import { CreateTenantDialog, EditTenantDialog, DeleteConfirmationDialog } from '../components';
import type { TenantOutput } from '../../../types';

const SYSTEM_ADMIN = 1;
const TENANT_ADMIN = 2;

const TenantsListPage = () => {
  const dispatch = useAppDispatch();
  const { user } = useAppSelector((state) => state.auth);
  const { tenants, isLoading, error, pagination } = useAppSelector((state) => state.tenants);
  
  // Extract all role IDs from user's tenant roles
  const userRoleIds = user?.tenantRoles?.map(tr => tr.roleId).filter((id): id is number => id !== null && id !== undefined) || [];
  const isSystemAdmin = userRoleIds.includes(SYSTEM_ADMIN);
  const isTenantAdmin = userRoleIds.includes(TENANT_ADMIN);

  const [expandedRow, setExpandedRow] = useState<number | null>(null);
  const [searchName, setSearchName] = useState('');
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
  const [selectedTenant, setSelectedTenant] = useState<TenantOutput | null>(null);

  // Load tenants based on role
  useEffect(() => {
    if (isSystemAdmin) {
      dispatch(fetchTenants({ page: page + 1, size: rowsPerPage, name: searchName || undefined }));
    } else if (isTenantAdmin) {
      dispatch(fetchMyTenants());
    }
  }, [dispatch, isSystemAdmin, isTenantAdmin, page, rowsPerPage, searchName]);

  const handleExpandClick = (tenantId?: number) => {
    setExpandedRow(expandedRow === tenantId ? null : tenantId || null);
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

  const handleStatusToggle = async (tenant: TenantOutput) => {
    if (!isSystemAdmin || !tenant.tenantId) return;
    
    try {
      await dispatch(toggleTenantStatus({ tenantId: tenant.tenantId, active: !tenant.active })).unwrap();
      setSnackbar({
        open: true,
        message: `Tenant ${tenant.active ? 'deactivated' : 'activated'} successfully`,
        severity: 'success',
      });
    } catch (err) {
      setSnackbar({
        open: true,
        message: `Failed to toggle tenant status: ${err}`,
        severity: 'error',
      });
    }
  };

  const handleOpenCreateDialog = () => {
    setCreateDialogOpen(true);
  };

  const handleOpenEditDialog = (tenant: TenantOutput) => {
    setSelectedTenant(tenant);
    setEditDialogOpen(true);
  };

  const handleOpenDeleteDialog = (tenant: TenantOutput) => {
    setSelectedTenant(tenant);
    setDeleteDialogOpen(true);
  };

  const handleDialogSuccess = (message: string) => {
    setSnackbar({
      open: true,
      message,
      severity: 'success',
    });
    // Refresh the list
    if (isSystemAdmin) {
      dispatch(fetchTenants({ page: page + 1, size: rowsPerPage, name: searchName || undefined }));
    } else if (isTenantAdmin) {
      dispatch(fetchMyTenants());
    }
  };

  const handleCloseSnackbar = () => {
    setSnackbar({ ...snackbar, open: false });
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1">
          Tenants Management
        </Typography>
        {isSystemAdmin && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreateDialog}>
            Add Tenant
          </Button>
        )}
      </Box>

      {error && (
        <Box sx={{ mb: 2 }}>
          <Alert severity="error">{error}</Alert>
        </Box>
      )}

      <Paper sx={{ p: 2, mb: 2 }}>
        <TextField
          label="Search by Tenant Name"
          variant="outlined"
          size="small"
          fullWidth
          value={searchName}
          onChange={handleSearchChange}
          disabled={!isSystemAdmin}
        />
      </Paper>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell width={50} />
              <TableCell>Tenant Name</TableCell>
              <TableCell>BIC</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>Ownership</TableCell>
              <TableCell>Domain</TableCell>
              <TableCell>Active</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {isLoading ? (
              <TableRow>
                <TableCell colSpan={8} align="center" sx={{ py: 4 }}>
                  <CircularProgress />
                </TableCell>
              </TableRow>
            ) : tenants.length === 0 ? (
              <TableRow>
                <TableCell colSpan={8} align="center" sx={{ py: 4 }}>
                  <Typography variant="body2" color="text.secondary">
                    No tenants found
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              tenants.map((tenant) => (
                <React.Fragment key={tenant.tenantId}>
                  <TableRow hover>
                    <TableCell>
                      <IconButton
                        size="small"
                        onClick={() => handleExpandClick(tenant.tenantId)}
                        aria-label="expand row"
                      >
                        {expandedRow === tenant.tenantId ? <KeyboardArrowUpIcon /> : <KeyboardArrowDownIcon />}
                      </IconButton>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" fontWeight="medium">
                        {tenant.tenantName}
                      </Typography>
                    </TableCell>
                    <TableCell>{tenant.tenantBIC}</TableCell>
                    <TableCell>
                      <Chip label={getTenantTypeLabel(tenant.tenantType)} size="small" />
                    </TableCell>
                    <TableCell>
                      <Chip label={getOwnershipLabel(tenant.ownership)} size="small" variant="outlined" />
                    </TableCell>
                    <TableCell>{tenant.domain || '-'}</TableCell>
                    <TableCell>
                      <Switch
                        checked={tenant.active || false}
                        onChange={() => handleStatusToggle(tenant)}
                        disabled={!isSystemAdmin}
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="right">
                      <IconButton size="small" aria-label="edit" onClick={() => handleOpenEditDialog(tenant)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                      {isSystemAdmin && (
                        <IconButton size="small" aria-label="delete" onClick={() => handleOpenDeleteDialog(tenant)}>
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      )}
                    </TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell style={{ paddingBottom: 0, paddingTop: 0 }} colSpan={8}>
                      <Collapse in={expandedRow === tenant.tenantId} timeout="auto" unmountOnExit>
                        <Box sx={{ margin: 2 }}>
                          <Typography variant="subtitle2" gutterBottom component="div" fontWeight="bold">
                            Contacts
                          </Typography>
                          <ContactsList contacts={tenant.contacts} />
                        </Box>
                      </Collapse>
                    </TableCell>
                  </TableRow>
                </React.Fragment>
              ))
            )}
          </TableBody>
        </Table>
        {isSystemAdmin && (
          <TablePagination
            rowsPerPageOptions={[5, 10, 25, 50]}
            component="div"
            count={pagination.count}
            rowsPerPage={rowsPerPage}
            page={page}
            onPageChange={handleChangePage}
            onRowsPerPageChange={handleChangeRowsPerPage}
          />
        )}
      </TableContainer>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={handleCloseSnackbar}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert onClose={handleCloseSnackbar} severity={snackbar.severity} sx={{ width: '100%' }}>
          {snackbar.message}
        </Alert>
      </Snackbar>

      <CreateTenantDialog
        open={createDialogOpen}
        onClose={() => setCreateDialogOpen(false)}
        onSuccess={handleDialogSuccess}
      />

      <EditTenantDialog
        open={editDialogOpen}
        tenant={selectedTenant}
        onClose={() => setEditDialogOpen(false)}
        onSuccess={handleDialogSuccess}
        isSystemAdmin={isSystemAdmin}
      />

      <DeleteConfirmationDialog
        open={deleteDialogOpen}
        tenant={selectedTenant}
        onClose={() => setDeleteDialogOpen(false)}
        onSuccess={handleDialogSuccess}
      />
    </Box>
  );
};

export default TenantsListPage;
