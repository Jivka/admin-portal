import { useState, useEffect, useRef, FormEvent } from 'react';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import {
  Box,
  Container,
  Typography,
  TextField,
  Button,
  Paper,
  FormControlLabel,
  Checkbox,
  Link,
  IconButton,
  InputAdornment,
  CircularProgress,
  Alert,
  Stack,
} from '@mui/material';
import { Visibility, VisibilityOff } from '@mui/icons-material';
import { useAppDispatch, useAppSelector } from '../../../store/hooks';
import { signIn } from '../../../store/authSlice';
import { STORAGE_KEYS } from '../../../utils/constants';

const LoginPage = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { isLoading, error } = useAppSelector((state) => state.auth);

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [uiError, setUiError] = useState<string | null>(null);

  const emailInputRef = useRef<HTMLInputElement>(null);
  const passwordInputRef = useRef<HTMLInputElement>(null);

  // On mount, load remembered email and focus appropriately
  useEffect(() => {
    const rememberedEmail = localStorage.getItem(STORAGE_KEYS.REMEMBERED_EMAIL);
    if (rememberedEmail) {
      setEmail(rememberedEmail);
      setRememberMe(true);
      // Focus password when email is already filled
      setTimeout(() => passwordInputRef.current?.focus(), 100);
    } else {
      // Focus email when no remembered email
      setTimeout(() => emailInputRef.current?.focus(), 100);
    }
  }, []);

  // Mirror Redux error to local uiError
  useEffect(() => {
    setUiError(error);
  }, [error]);

  // Clear uiError when user edits email or password
  const handleEmailChange = (value: string) => {
    setEmail(value);
    setUiError(null);
  };

  const handlePasswordChange = (value: string) => {
    setPassword(value);
    setUiError(null);
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

    try {
      await dispatch(signIn({ email: trimmedEmail, password })).unwrap();

      // On success, update/remove remembered email based on checkbox
      if (rememberMe) {
        localStorage.setItem(STORAGE_KEYS.REMEMBERED_EMAIL, trimmedEmail);
      } else {
        localStorage.removeItem(STORAGE_KEYS.REMEMBERED_EMAIL);
      }

      // Navigate to dashboard with replace to prevent back to login
      navigate('/dashboard', { replace: true });
    } catch (err) {
      // On failure, clear only the password field
      setPassword('');
      // uiError is already set by the useEffect mirroring Redux error
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
              Login
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Sign in to your account
            </Typography>
          </Box>

          {uiError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {uiError}
            </Alert>
          )}

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
                  autoCapitalize: 'none',
                  spellCheck: false,
                  autoCorrect: 'off',
                }}
              />

              <TextField
                inputRef={passwordInputRef}
                label="Password"
                type={showPassword ? 'text' : 'password'}
                autoComplete="current-password"
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

              <FormControlLabel
                control={
                  <Checkbox
                    checked={rememberMe}
                    onChange={(e) => setRememberMe(e.target.checked)}
                    disabled={isLoading}
                  />
                }
                label="Remember me"
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
                    Logging in…
                  </>
                ) : (
                  'Sign in'
                )}
              </Button>

              <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 2 }}>
                <Link component={RouterLink} to="/forgot-password" variant="body2">
                  Forgot password?
                </Link>
                <Link component={RouterLink} to="/signup" variant="body2">
                  Create account
                </Link>
              </Box>
            </Stack>
          </form>
        </Paper>
      </Box>
    </Container>
  );
};

export default LoginPage;
