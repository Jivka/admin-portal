import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Button,
  Box,
  CircularProgress,
  Alert,
} from '@mui/material';
import WarningAmberIcon from '@mui/icons-material/WarningAmber';
import type { AxiosError } from 'axios';
import { useAppDispatch } from '../../../store/hooks';
import { deleteUser } from '../../../store/usersSlice';
import { ErrorAlert } from '../../../components/common';
import type { UserOutput } from '../../../types';

interface DeleteConfirmationDialogProps {
  open: boolean;
  onClose: () => void;
  onSuccess: (message: string) => void;
  user: UserOutput | null;
  isSystemAdmin: boolean;
}

export const DeleteConfirmationDialog: React.FC<DeleteConfirmationDialogProps> = ({
  open,
  onClose,
  onSuccess,
  user,
  isSystemAdmin,
}) => {
  const dispatch = useAppDispatch();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<AxiosError | null>(null);

  const handleDelete = async () => {
    if (!user?.userId) return;

    setIsLoading(true);
    setError(null);

    try {
      const tenantId = user.tenantRoles?.[0]?.tenantId ?? undefined;
      await dispatch(deleteUser({ 
        userId: user.userId, 
        isSystemAdmin,
        tenantId 
      })).unwrap();

      onSuccess(`User "${user.fullName || user.email}" deleted successfully`);
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

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <WarningAmberIcon color="warning" />
          Confirm Delete User
        </Box>
      </DialogTitle>
      <DialogContent>
        <DialogContentText>
          Are you sure you want to delete user <strong>{user?.fullName || user?.email}</strong>?
        </DialogContentText>
        <DialogContentText sx={{ mt: 1, color: 'text.secondary', fontSize: '0.875rem' }}>
          Note: Deletion will be prevented if this user has recent activity or associated records.
        </DialogContentText>

        {error && (
          <Box sx={{ mt: 2 }}>
            {typeof error === 'string' ? (
              <Alert severity="error">{error}</Alert>
            ) : (
              <ErrorAlert error={error} />
            )}
          </Box>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={isLoading}>
          Cancel
        </Button>
        <Button
          onClick={handleDelete}
          variant="contained"
          color="error"
          disabled={isLoading}
        >
          {isLoading ? <CircularProgress size={24} /> : 'Delete User'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default DeleteConfirmationDialog;
