import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  MenuItem,
  Box,
  CircularProgress,
  Alert,
} from '@mui/material';
import { useAppDispatch } from '../../../store/hooks';
import { createTenant } from '../../../store/tenantsSlice';
import { tenantTypeOptions, ownershipOptions } from '../../../utils';
import { ErrorAlert } from '../../../components/common';

interface CreateTenantDialogProps {
  open: boolean;
  onClose: () => void;
  onSuccess: (message: string) => void;
}

export const CreateTenantDialog: React.FC<CreateTenantDialogProps> = ({ open, onClose, onSuccess }) => {
  const dispatch = useAppDispatch();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<any>(null);

  const [formData, setFormData] = useState({
    tenantName: '',
    tenantBIC: '',
    tenantType: 1,
    ownership: 1,
    domain: '',
    summary: '',
    logoUrl: '',
  });

  const handleChange = (field: string) => (event: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, [field]: event.target.value });
    setError(null);
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setIsLoading(true);
    setError(null);

    try {
      await dispatch(createTenant({
        tenantName: formData.tenantName.trim(),
        tenantBIC: formData.tenantBIC.trim(),
        tenantType: formData.tenantType,
        ownership: formData.ownership,
        domain: formData.domain.trim() || undefined,
        summary: formData.summary.trim() || undefined,
        logoUrl: formData.logoUrl.trim() || undefined,
      })).unwrap();

      onSuccess('Tenant created successfully');
      handleClose();
    } catch (err) {
      setError(err as string);
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    if (!isLoading) {
      setFormData({
        tenantName: '',
        tenantBIC: '',
        tenantType: 1,
        ownership: 1,
        domain: '',
        summary: '',
        logoUrl: '',
      });
      setError(null);
      onClose();
    }
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit}>
        <DialogTitle>Add New Tenant</DialogTitle>
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
              label="Tenant Name"
              required
              fullWidth
              value={formData.tenantName}
              onChange={handleChange('tenantName')}
              disabled={isLoading}
              inputProps={{ maxLength: 128 }}
            />

            <TextField
              label="Tenant BIC"
              required
              fullWidth
              value={formData.tenantBIC}
              onChange={handleChange('tenantBIC')}
              disabled={isLoading}
              inputProps={{ maxLength: 128 }}
            />

            <TextField
              select
              label="Tenant Type"
              required
              fullWidth
              value={formData.tenantType}
              onChange={handleChange('tenantType')}
              disabled={isLoading}
            >
              {tenantTypeOptions.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </TextField>

            <TextField
              select
              label="Ownership"
              required
              fullWidth
              value={formData.ownership}
              onChange={handleChange('ownership')}
              disabled={isLoading}
            >
              {ownershipOptions.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </TextField>

            <TextField
              label="Domain"
              fullWidth
              value={formData.domain}
              onChange={handleChange('domain')}
              disabled={isLoading}
              inputProps={{ maxLength: 128 }}
              helperText="Optional"
            />

            <TextField
              label="Summary"
              fullWidth
              multiline
              rows={3}
              value={formData.summary}
              onChange={handleChange('summary')}
              disabled={isLoading}
              inputProps={{ maxLength: 256 }}
              helperText="Optional"
            />

            <TextField
              label="Logo URL"
              fullWidth
              value={formData.logoUrl}
              onChange={handleChange('logoUrl')}
              disabled={isLoading}
              inputProps={{ maxLength: 256 }}
              helperText="Optional"
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose} disabled={isLoading}>
            Cancel
          </Button>
          <Button type="submit" variant="contained" disabled={isLoading}>
            {isLoading ? <CircularProgress size={24} /> : 'Create Tenant'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
};
