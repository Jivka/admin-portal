import { useState, useRef, useEffect } from 'react';
import type { FormEvent } from 'react';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import {
  Box,
  Container,
  Typography,
  TextField,
  Button,
  Paper,
  Link,
  CircularProgress,
  Stack,
} from '@mui/material';
import type { AxiosError } from 'axios';
import { authApi } from '../../../api';
import { ErrorAlert } from '../../../components/common';

const ForgotPasswordPage = () => {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<AxiosError | null>(null);

  const emailInputRef = useRef<HTMLInputElement>(null);

  // Auto-focus email field on mount
  useEffect(() => {
    setTimeout(() => emailInputRef.current?.focus(), 100);
  }, []);

  // Clear error when user edits email
  const handleEmailChange = (value: string) => {
    setEmail(value);
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
      await authApi.forgotPassword({ email: trimmedEmail });

      // On success, navigate to reset password page with email
      navigate('/reset-password', {
        state: { email: trimmedEmail },
      });
    } catch (err) {
      setError(err as AxiosError);
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
              Forgot Password
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Enter your email to receive password reset instructions
            </Typography>
          </Box>

          <ErrorAlert error={error} onClose={() => setError(null)} />

          <form autoComplete="on" onSubmit={handleSubmit}>
            <Stack spacing={2}>
              <TextField
                inputRef={emailInputRef}
                label="Email"
                type="email"
                autoComplete="email"
                value={email}
                onChange={(e) => handleEmailChange(e.target.value)}
                disabled={isLoading}
                required
                fullWidth
                inputProps={{
                  inputMode: 'email',
                  maxLength: 128,
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
                    Sending…
                  </>
                ) : (
                  'Send Reset Instructions'
                )}
              </Button>

              <Box sx={{ display: 'flex', justifyContent: 'flex-start', mt: 2 }}>
                <Link component={RouterLink} to="/login" variant="body2">
                  Back to login
                </Link>
              </Box>
            </Stack>
          </form>
        </Paper>
      </Box>
    </Container>
  );
};

export default ForgotPasswordPage;
