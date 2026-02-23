import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Box,
  CircularProgress,
  Alert,
} from '@mui/material';
import type { AxiosError } from 'axios';
import { useAppDispatch } from '../../../store/hooks';
import { updateUser } from '../../../store/usersSlice';
import { ErrorAlert, TenantRoleList } from '../../../components/common';
import type { EditUserRequest, UserOutput } from '../../../types';

interface EditUserDialogProps {
  open: boolean;
  onClose: () => void;
  onSuccess: (message: string) => void;
  user: UserOutput | null;
  isSystemAdmin: boolean;
}

export const EditUserDialog: React.FC<EditUserDialogProps> = ({ open, onClose, onSuccess, user, isSystemAdmin }) => {
  const dispatch = useAppDispatch();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<AxiosError | null>(null);

  const [formData, setFormData] = useState<EditUserRequest>({
    userId: 0,
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    roleId: 0,
  });

  useEffect(() => {
    if (user) {
      setFormData({
        userId: user.userId || 0,
        firstName: user.firstName || '',
        lastName: user.lastName || '',
        email: user.email || '',
        phone: user.phone || '',
        roleId: user.tenantRoles?.[0]?.roleId || 0,
      });
      setError(null);
    }
  }, [user]);

  const handleChange = (field: keyof EditUserRequest) => (event: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, [field]: event.target.value });
    setError(null);
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setIsLoading(true);
    setError(null);

    try {
      const tenantId = user?.tenantRoles?.[0]?.tenantId ?? undefined;
      await dispatch(updateUser({
        data: {
          userId: formData.userId,
          firstName: formData.firstName.trim(),
          lastName: formData.lastName.trim(),
          email: formData.email.trim(),
          phone: formData.phone?.trim() || undefined,
          roleId: formData.roleId,
        },
        isSystemAdmin,
        tenantId,
      })).unwrap();

      onSuccess('User updated successfully');
      handleClose();
    } catch (err) {
      setError(err as AxiosError);
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    if (!isLoading) {
      setError(null);
      onClose();
    }
  };

  const isFormValid = formData.firstName && formData.lastName && formData.email;

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth aria-labelledby="edit-user-dialog-title">
      <form onSubmit={handleSubmit}>
        <DialogTitle id="edit-user-dialog-title">Edit User</DialogTitle>
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
              autoComplete="given-name"
              inputProps={{ maxLength: 128 }}
            />

            <TextField
              label="Last Name"
              required
              fullWidth
              value={formData.lastName}
              onChange={handleChange('lastName')}
              disabled={isLoading}
              autoComplete="family-name"
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
              autoComplete="email"
              inputProps={{ maxLength: 256 }}
            />

            <TextField
              label="Phone"
              fullWidth
              type="tel"
              value={formData.phone}
              onChange={handleChange('phone')}
              disabled={isLoading}
              autoComplete="tel"
              inputProps={{ maxLength: 32 }}
            />

            {/* Display all tenant-role pairs */}
            <TenantRoleList tenantRoles={user?.tenantRoles} />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose} disabled={isLoading}>
            Cancel
          </Button>
          <Button type="submit" variant="contained" disabled={isLoading || !isFormValid}>
            {isLoading ? <CircularProgress size={24} aria-label="Updating user" /> : 'Update User'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
};

export default EditUserDialog;
