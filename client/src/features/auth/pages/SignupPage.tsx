import { useState, useRef, useEffect } from 'react';
import type { FormEvent } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
  Box,
  Container,
  Typography,
  TextField,
  Button,
  Paper,
  Link,
  CircularProgress,
  Alert,
  Stack,
} from '@mui/material';
import type { AxiosError } from 'axios';
import { authApi } from '../../../api';
import { ErrorAlert } from '../../../components/common';

const SignupPage = () => {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<AxiosError | null>(null);
  const [successEmail, setSuccessEmail] = useState<string | null>(null);
  const [isResending, setIsResending] = useState(false);

  const firstNameInputRef = useRef<HTMLInputElement>(null);

  // Auto-focus first name field on mount
  useEffect(() => {
    setTimeout(() => firstNameInputRef.current?.focus(), 100);
  }, []);

  // Clear error when user edits any field
  const handleFieldChange = (setter: (value: string) => void, value: string) => {
    setter(value);
    setError(null);
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    // Prevent double submits
    if (isLoading) return;

    // Trim email and update state if it changed
    const trimmedEmail = email.trim();
    if (trimmedEmail !== email) {
      setEmail(trimmedEmail);
    }

    setIsLoading(true);
    setError(null);

    try {
      await authApi.signUp({
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        email: trimmedEmail,
      });

      // On success, show success message
      setSuccessEmail(trimmedEmail);
      
      // Clear form
      setFirstName('');
      setLastName('');
      setEmail('');
    } catch (err) {
      setError(err as AxiosError);
    } finally {
      setIsLoading(false);
    }
  };

  const handleResendVerification = async () => {
    if (!successEmail || isResending) return;

    setIsResending(true);
    setError(null);

    try {
      await authApi.resendVerificationCode(successEmail);
      // Could show a temporary toast here, but Alert update is sufficient
    } catch (err) {
      setError(err as AxiosError);
    } finally {
      setIsResending(false);
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
              Create Account
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Sign up for a new account
            </Typography>
          </Box>

          {successEmail && (
            <Alert 
              severity="success" 
              sx={{ mb: 2 }}
              action={
                <Button 
                  color="inherit" 
                  size="small"
                  onClick={handleResendVerification}
                  disabled={isResending}
                >
                  {isResending ? 'Sending...' : 'Resend'}
                </Button>
              }
            >
              <Typography variant="body2">
                Verification email sent to <strong>{successEmail}</strong>
              </Typography>
              <Typography variant="caption" display="block" sx={{ mt: 0.5 }}>
                Please check your inbox and follow the instructions to verify your email and set your password.
              </Typography>
            </Alert>
          )}

          <ErrorAlert error={error} onClose={() => setError(null)} />

          <form autoComplete="on" onSubmit={handleSubmit}>
            <Stack spacing={2}>
              <TextField
                inputRef={firstNameInputRef}
                label="First Name"
                type="text"
                autoComplete="given-name"
                value={firstName}
                onChange={(e) => handleFieldChange(setFirstName, e.target.value)}
                disabled={isLoading}
                required
                fullWidth
                inputProps={{
                  maxLength: 256,
                  autoCapitalize: 'words',
                  spellCheck: false,
                }}
              />

              <TextField
                label="Last Name"
                type="text"
                autoComplete="family-name"
                value={lastName}
                onChange={(e) => handleFieldChange(setLastName, e.target.value)}
                disabled={isLoading}
                required
                fullWidth
                inputProps={{
                  maxLength: 256,
                  autoCapitalize: 'words',
                  spellCheck: false,
                }}
              />

              <TextField
                label="Email"
                type="email"
                autoComplete="email"
                value={email}
                onChange={(e) => handleFieldChange(setEmail, e.target.value)}
                disabled={isLoading}
                required
                fullWidth
                inputProps={{
                  inputMode: 'email',
                  maxLength: 256,
                  autoCapitalize: 'none',
                  spellCheck: false,
                  autoCorrect: 'off',
                }}
              />

              <Button
                type="submit"
                variant="contained"
                size="large"
                fullWidth
                disabled={isLoading}
                sx={{ mt: 1 }}
              >
                {isLoading ? (
                  <>
                    <CircularProgress size={16} sx={{ mr: 1 }} color="inherit" />
                    Creating account…
                  </>
                ) : (
                  'Sign Up'
                )}
              </Button>

              <Box sx={{ display: 'flex', justifyContent: 'center', mt: 2 }}>
                <Typography variant="body2" color="text.secondary">
                  Already have an account?{' '}
                  <Link component={RouterLink} to="/login">
                    Sign in
                  </Link>
                </Typography>
              </Box>
            </Stack>
          </form>
        </Paper>
      </Box>
    </Container>
  );
};

export default SignupPage;
