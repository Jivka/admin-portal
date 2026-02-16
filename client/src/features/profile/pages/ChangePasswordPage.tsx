import { useState, useEffect } from 'react';
import type { FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Container,
  Typography,
  TextField,
  Button,
  Paper,
  IconButton,
  InputAdornment,
  CircularProgress,
  Stack,
} from '@mui/material';
import { Visibility, VisibilityOff, ArrowBack } from '@mui/icons-material';
import type { AxiosError } from 'axios';
import { useAppSelector } from '../../../store/hooks';
import { usersApi } from '../../../api';
import { ErrorAlert, PasswordStrength } from '../../../components/common';

const ChangePasswordPage = () => {
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);

  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showCurrentPassword, setShowCurrentPassword] = useState(false);
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<AxiosError | null>(null);
  const [passwordMatchError, setPasswordMatchError] = useState<string | null>(null);

  // Clear error when user edits any field
  const handleCurrentPasswordChange = (value: string) => {
    setCurrentPassword(value);
    setError(null);
  };

  const handleNewPasswordChange = (value: string) => {
    setNewPassword(value);
    setError(null);
    setPasswordMatchError(null);
  };

  const handleConfirmPasswordChange = (value: string) => {
    setConfirmPassword(value);
    setError(null);
    setPasswordMatchError(null);
  };

  // Check password match in real-time
  useEffect(() => {
    if (confirmPassword && newPassword !== confirmPassword) {
      setPasswordMatchError('Passwords do not match');
    } else {
      setPasswordMatchError(null);
    }
  }, [newPassword, confirmPassword]);

  const handleCancel = () => {
    navigate('/profile');
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    // Prevent double submits
    if (isLoading) return;

    // Validate passwords match
    if (newPassword !== confirmPassword) {
      setPasswordMatchError('Passwords do not match');
      return;
    }

    // Ensure user data is available
    if (!user?.userId || !user?.email) {
      setError({
        message: 'User information not available',
      } as AxiosError);
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      await usersApi.changePassword({
        userId: user.userId,
        email: user.email,
        currentPassword,
        newPassword,
      });

      // On success, navigate to profile with success message
      navigate('/profile', {
        state: { success: 'Password changed successfully.' },
      });
    } catch (err) {
      setError(err as AxiosError);
      // Clear all password fields on error (security best practice)
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Container maxWidth="sm">
      <Box sx={{ py: 4 }}>
        <Paper sx={{ p: 4 }}>
          {/* Header with back button */}
          <Box sx={{ mb: 3 }}>
            <Button
              startIcon={<ArrowBack />}
              onClick={handleCancel}
              disabled={isLoading}
              sx={{ mb: 2 }}
            >
              Back to Profile
            </Button>
            <Typography variant="h4" component="h1" gutterBottom fontWeight={600}>
              Change Password
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Enter your current password and choose a new password
            </Typography>
          </Box>

          <ErrorAlert error={error} onClose={() => setError(null)} />

          <form autoComplete="on" onSubmit={handleSubmit}>
            <Stack spacing={3}>
              {/* Current Password */}
              <TextField
                label="Current Password"
                type={showCurrentPassword ? 'text' : 'password'}
                autoComplete="current-password"
                value={currentPassword}
                onChange={(e) => handleCurrentPasswordChange(e.target.value)}
                disabled={isLoading}
                required
                fullWidth
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        aria-label="toggle current password visibility"
                        onClick={() => setShowCurrentPassword(!showCurrentPassword)}
                        edge="end"
                        disabled={isLoading}
                      >
                        {showCurrentPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />

              {/* New Password */}
              <Box>
                <TextField
                  label="New Password"
                  type={showNewPassword ? 'text' : 'password'}
                  autoComplete="new-password"
                  value={newPassword}
                  onChange={(e) => handleNewPasswordChange(e.target.value)}
                  disabled={isLoading}
                  required
                  fullWidth
                  InputProps={{
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton
                          aria-label="toggle new password visibility"
                          onClick={() => setShowNewPassword(!showNewPassword)}
                          edge="end"
                          disabled={isLoading}
                        >
                          {showNewPassword ? <VisibilityOff /> : <Visibility />}
                        </IconButton>
                      </InputAdornment>
                    ),
                  }}
                />
                <PasswordStrength password={newPassword} />
              </Box>

              {/* Confirm New Password */}
              <TextField
                label="Confirm New Password"
                type={showConfirmPassword ? 'text' : 'password'}
                autoComplete="new-password"
                value={confirmPassword}
                onChange={(e) => handleConfirmPasswordChange(e.target.value)}
                disabled={isLoading}
                required
                fullWidth
                error={!!passwordMatchError}
                helperText={passwordMatchError}
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        aria-label="toggle confirm password visibility"
                        onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                        edge="end"
                        disabled={isLoading}
                      >
                        {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />

              {/* Action Buttons */}
              <Stack direction="row" spacing={2} sx={{ mt: 2 }}>
                <Button
                  type="submit"
                  variant="contained"
                  disabled={isLoading || !!passwordMatchError}
                  fullWidth
                  sx={{ py: 1.5 }}
                >
                  {isLoading ? (
                    <>
                      <CircularProgress size={20} sx={{ mr: 1 }} />
                      Changing Password...
                    </>
                  ) : (
                    'Change Password'
                  )}
                </Button>
                <Button
                  variant="outlined"
                  onClick={handleCancel}
                  disabled={isLoading}
                  fullWidth
                  sx={{ py: 1.5 }}
                >
                  Cancel
                </Button>
              </Stack>
            </Stack>
          </form>
        </Paper>
      </Box>
    </Container>
  );
};

export default ChangePasswordPage;
