import { useState, useRef, useEffect } from 'react';
import type { FormEvent } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
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
import { Visibility, VisibilityOff } from '@mui/icons-material';
import type { AxiosError } from 'axios';
import { authApi } from '../../../api';
import { ErrorAlert, PasswordStrength } from '../../../components/common';

const VerifyEmailPage = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  // Extract URL parameters
  const emailParam = searchParams.get('email') || '';
  const codeParam = searchParams.get('code') || '';

  const [email] = useState(emailParam);
  const [verificationToken] = useState(codeParam);
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<AxiosError | null>(null);
  const [passwordMatchError, setPasswordMatchError] = useState<string | null>(null);

  const passwordInputRef = useRef<HTMLInputElement>(null);

  // Auto-focus password field on mount
  useEffect(() => {
    setTimeout(() => passwordInputRef.current?.focus(), 100);
  }, []);

  // Clear error when user edits any field
  const handlePasswordChange = (value: string) => {
    setPassword(value);
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
    if (confirmPassword && password !== confirmPassword) {
      setPasswordMatchError('Passwords do not match');
    } else {
      setPasswordMatchError(null);
    }
  }, [password, confirmPassword]);

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    // Prevent double submits
    if (isLoading) return;

    // Validate passwords match
    if (password !== confirmPassword) {
      setPasswordMatchError('Passwords do not match');
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      await authApi.verifyEmail({
        verificationToken,
        email,
        password,
        confirmPassword,
      });

      // On success, navigate to login with success message
      navigate('/login', {
        state: { success: 'Email verified successfully. Please log in.' },
      });
    } catch (err) {
      setError(err as AxiosError);
      // Clear password fields on error
      setPassword('');
      setConfirmPassword('');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Container maxWidth="sm">
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          alignItems: 'center',
        }}
      >
        <Paper sx={{ p: 4, width: '100%' }}>
          <Box sx={{ mb: 3, textAlign: 'center' }}>
            <Typography variant="h4" component="h1" gutterBottom fontWeight={600}>
              Verify Email
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Set your password to complete registration
            </Typography>
          </Box>

          <ErrorAlert error={error} onClose={() => setError(null)} />

          <form autoComplete="on" onSubmit={handleSubmit}>
            <Stack spacing={2}>
              <TextField
                label="Email"
                type="email"
                value={email}
                disabled
                fullWidth
                inputProps={{
                  readOnly: true,
                }}
              />

              <TextField
                inputRef={passwordInputRef}
                label="Password"
                type={showPassword ? 'text' : 'password'}
                autoComplete="new-password"
                value={password}
                onChange={(e) => handlePasswordChange(e.target.value)}
                disabled={isLoading}
                required
                fullWidth
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        aria-label="toggle password visibility"
                        onClick={() => setShowPassword(!showPassword)}
                        disabled={isLoading}
                        edge="end"
                      >
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />

              <PasswordStrength password={password} />

              <TextField
                label="Confirm Password"
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
                        disabled={isLoading}
                        edge="end"
                      >
                        {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />

              <Button
                type="submit"
                variant="contained"
                size="large"
                fullWidth
                disabled={isLoading || !!passwordMatchError}
                sx={{ mt: 1 }}
              >
                {isLoading ? (
                  <>
                    <CircularProgress size={16} sx={{ mr: 1 }} color="inherit" />
                    Verifying…
                  </>
                ) : (
                  'Verify Email & Set Password'
                )}
              </Button>
            </Stack>
          </form>
        </Paper>
      </Box>
    </Container>
  );
};

export default VerifyEmailPage;
