import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Autocomplete,
  Box,
  CircularProgress,
  Alert,
} from '@mui/material';
import type { AxiosError } from 'axios';
import { useAppDispatch, useAppSelector } from '../../../store/hooks';
import { createUser } from '../../../store/usersSlice';
import { fetchAllRoles, fetchTenantRoles } from '../../../store/rolesSlice';
import { ErrorAlert } from '../../../components/common';
import type { CreateUserRequest, RoleOutput, TenantOutput } from '../../../types';

interface CreateUserDialogProps {
  open: boolean;
  onClose: () => void;
  onSuccess: (message: string) => void;
  isSystemAdmin: boolean;
}

export const CreateUserDialog: React.FC<CreateUserDialogProps> = ({ open, onClose, onSuccess, isSystemAdmin }) => {
  const dispatch = useAppDispatch();
  const { roles } = useAppSelector((state) => state.roles);
  const { tenants } = useAppSelector((state) => state.tenants);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<AxiosError | null>(null);

  const [formData, setFormData] = useState<CreateUserRequest>({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    roleId: 0,
    tenantId: 0,
  });

  const [selectedTenant, setSelectedTenant] = useState<TenantOutput | null>(null);
  const [selectedRole, setSelectedRole] = useState<RoleOutput | null>(null);

  // Filter roles based on user role (exclude System Admin for Tenant Admin)
  const availableRoles = isSystemAdmin 
    ? roles 
    : roles.filter(role => role.roleName !== 'SystemAdministrator');

  useEffect(() => {
    if (open) {
      if (isSystemAdmin) {
        dispatch(fetchAllRoles());
      } else {
        dispatch(fetchTenantRoles());
      }
    }
  }, [open, dispatch, isSystemAdmin]);

  const handleChange = (field: keyof CreateUserRequest) => (event: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, [field]: event.target.value });
    setError(null);
  };

  const handleTenantChange = (_event: React.SyntheticEvent, value: TenantOutput | null) => {
    setSelectedTenant(value);
    setFormData({ ...formData, tenantId: value?.tenantId || 0 });
    setError(null);
  };

  const handleRoleChange = (_event: React.SyntheticEvent, value: RoleOutput | null) => {
    setSelectedRole(value);
    setFormData({ ...formData, roleId: value?.roleId || 0 });
    setError(null);
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setIsLoading(true);
    setError(null);

    try {
      await dispatch(createUser({
        data: {
          firstName: formData.firstName.trim(),
          lastName: formData.lastName.trim(),
          email: formData.email.trim(),
          phone: formData.phone?.trim() || undefined,
          roleId: formData.roleId,
          tenantId: formData.tenantId,
        },
        isSystemAdmin,
      })).unwrap();

      onSuccess('User created successfully');
      handleClose();
    } catch (err) {
      setError(err as AxiosError);
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    if (!isLoading) {
      setFormData({
        firstName: '',
        lastName: '',
        email: '',
        phone: '',
        roleId: 0,
        tenantId: 0,
      });
      setSelectedTenant(null);
      setSelectedRole(null);
      setError(null);
      onClose();
    }
  };

  const isFormValid = formData.firstName && formData.lastName && formData.email && formData.roleId && formData.tenantId;

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit}>
        <DialogTitle>Add New User</DialogTitle>
        <DialogContent>
          {error && (
            <Box sx={{ mb: 2 }}>
              {typeof error === 'string' ? (
                <Alert severity="error">{error}</Alert>
              ) : (
                <ErrorAlert error={error} />
              )}
            </Box>
          )}

          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField
              label="First Name"
              required
              fullWidth
              value={formData.firstName}
              onChange={handleChange('firstName')}
              disabled={isLoading}
              inputProps={{ maxLength: 128 }}
            />

            <TextField
              label="Last Name"
              required
              fullWidth
              value={formData.lastName}
              onChange={handleChange('lastName')}
              disabled={isLoading}
              inputProps={{ maxLength: 128 }}
            />

            <TextField
              label="Email"
              required
              fullWidth
              type="email"
              value={formData.email}
              onChange={handleChange('email')}
              disabled={isLoading}
              inputProps={{ maxLength: 256 }}
            />

            <TextField
              label="Phone"
              fullWidth
              type="tel"
              value={formData.phone}
              onChange={handleChange('phone')}
              disabled={isLoading}
              inputProps={{ maxLength: 32 }}
            />

            <Autocomplete
              options={tenants}
              getOptionLabel={(option) => option.tenantName || ''}
              value={selectedTenant}
              onChange={handleTenantChange}
              renderInput={(params) => (
                <TextField {...params} label="Tenant" required />
              )}
              disabled={isLoading}
              isOptionEqualToValue={(option, value) => option.tenantId === value.tenantId}
            />

            <Autocomplete
              options={availableRoles}
              getOptionLabel={(option) => option.roleDisplayName || option.roleName || ''}
              value={selectedRole}
              onChange={handleRoleChange}
              renderInput={(params) => (
                <TextField {...params} label="Role" required />
              )}
              disabled={isLoading}
              isOptionEqualToValue={(option, value) => option.roleId === value.roleId}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose} disabled={isLoading}>
            Cancel
          </Button>
          <Button type="submit" variant="contained" disabled={isLoading || !isFormValid}>
            {isLoading ? <CircularProgress size={24} /> : 'Create User'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
};

export default CreateUserDialog;
