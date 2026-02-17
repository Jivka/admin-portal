import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  DialogContentText,
  Button,
  Box,
  CircularProgress,
  Typography,
  Alert,
} from '@mui/material';
import WarningIcon from '@mui/icons-material/Warning';
import { useAppDispatch } from '../../../store/hooks';
import { deleteTenant } from '../../../store/tenantsSlice';
import { ErrorAlert } from '../../../components/common';
import type { TenantOutput } from '../../../types';

interface DeleteConfirmationDialogProps {
  open: boolean;
  tenant: TenantOutput | null;
  onClose: () => void;
  onSuccess: (message: string) => void;
}

export const DeleteConfirmationDialog: React.FC<DeleteConfirmationDialogProps> = ({
  open,
  tenant,
  onClose,
  onSuccess,
}) => {
  const dispatch = useAppDispatch();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<any>(null);

  const handleDelete = async () => {
    if (!tenant?.tenantId) return;

    setIsLoading(true);
    setError(null);

    try {
      await dispatch(deleteTenant(tenant.tenantId)).unwrap();
      onSuccess(`Tenant "${tenant.tenantName}" deleted successfully`);
      handleClose();
    } catch (err) {
      setError(err as string);
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

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="xs" fullWidth>
      <DialogTitle>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <WarningIcon color="warning" />
          <Typography variant="h6">Confirm Delete</Typography>
        </Box>
      </DialogTitle>
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

        <DialogContentText>
          Are you sure you want to delete the tenant <strong>"{tenant?.tenantName}"</strong>?
        </DialogContentText>
        <DialogContentText sx={{ mt: 1, color: 'text.secondary', fontSize: '0.875rem' }}>
          Note: Deletion will be prevented if this tenant has associated users.
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={isLoading}>
          Cancel
        </Button>
        <Button onClick={handleDelete} color="error" variant="contained" disabled={isLoading}>
          {isLoading ? <CircularProgress size={24} /> : 'Delete'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
