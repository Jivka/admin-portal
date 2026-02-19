import { useState, useEffect, useCallback } from 'react';
import type { FormEvent } from 'react';
import { useNavigate, useLocation, useBlocker, useBeforeUnload } from 'react-router-dom';
import {
  Box,
  Typography,
  Paper,
  TextField,
  Button,
  Chip,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Link,
} from '@mui/material';
import { CheckCircle, Cancel } from '@mui/icons-material';
import { useAppSelector, useAppDispatch } from '../../../store/hooks';
import { setUser } from '../../../store/authSlice';
import { usersApi } from '../../../api';
import { ErrorAlert, TenantRoleList } from '../../../components/common';
import type { AxiosError } from 'axios';
import type { UserOutput } from '../../../types';

const ProfilePage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useAppSelector((state) => state.auth);
  const dispatch = useAppDispatch();

  // Profile data from API
  const [profileData, setProfileData] = useState<UserOutput | null>(null);

  // Form state
  const [firstName, setFirstName] = useState(user?.firstName || '');
  const [lastName, setLastName] = useState(user?.lastName || '');
  const [email, setEmail] = useState(user?.email || '');
  const [phone, setPhone] = useState(user?.phone || '');

  // UI state
  const [isLoading, setIsLoading] = useState(false);
  const [isLoadingProfile, setIsLoadingProfile] = useState(true);
  const [error, setError] = useState<AxiosError | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [showEmailConfirmDialog, setShowEmailConfirmDialog] = useState(false);
  const [isDirty, setIsDirty] = useState(false);

  // Handle success message from location state (e.g., from change password)
  useEffect(() => {
    const stateSuccess = (location.state as { success?: string })?.success;
    if (stateSuccess) {
      setSuccessMessage(stateSuccess);
      // Clear location state to prevent showing message on refresh
      window.history.replaceState({}, document.title);
    }
  }, [location.state]);

  // Fetch fresh profile data on mount
  useEffect(() => {
    const fetchProfile = async () => {
      if (!user?.userId) return;

      setIsLoadingProfile(true);
      try {
        const data = await usersApi.getProfile(user.userId);
        setProfileData(data);
        // Initialize form with fresh data
        setFirstName(data.firstName || '');
        setLastName(data.lastName || '');
        setEmail(data.email || '');
        setPhone(data.phone || '');
      } catch (err) {
        console.error('Failed to fetch profile:', err);
        // Fallback to user data from auth state
        if (user) {
          setFirstName(user.firstName || '');
          setLastName(user.lastName || '');
          setEmail(user.email || '');
          setPhone(user.phone || '');
        }
      } finally {
        setIsLoadingProfile(false);
      }
    };

    fetchProfile();
  }, [user?.userId]);

  // Track form changes
  useEffect(() => {
    const currentData = profileData || user;
    if (!currentData) return;

    const hasChanges =
      firstName.trim() !== (currentData.firstName || '') ||
      lastName.trim() !== (currentData.lastName || '') ||
      email.trim() !== (currentData.email || '') ||
      phone.trim() !== (currentData.phone || '');

    setIsDirty(hasChanges);
  }, [firstName, lastName, email, phone, profileData, user]);

  // Auto-dismiss success message after 5 seconds
  useEffect(() => {
    if (successMessage) {
      const timer = setTimeout(() => setSuccessMessage(null), 5000);
      return () => clearTimeout(timer);
    }
  }, [successMessage]);

  // Block navigation when there are unsaved changes
  const blocker = useBlocker(
    ({ currentLocation, nextLocation }) =>
      isDirty && currentLocation.pathname !== nextLocation.pathname
  );

  // Warn on browser close/refresh
  useBeforeUnload(
    useCallback(
      (event) => {
        if (isDirty) {
          event.preventDefault();
        }
      },
      [isDirty]
    )
  );

  const handleFieldChange = (
    setter: (value: string) => void,
    value: string
  ) => {
    setter(value);
    setError(null);
  };

  const handleCancel = () => {
    const currentData = profileData || user;
    if (currentData) {
      setFirstName(currentData.firstName || '');
      setLastName(currentData.lastName || '');
      setEmail(currentData.email || '');
      setPhone(currentData.phone || '');
      setIsDirty(false);
      setError(null);
      setSuccessMessage(null);
    }
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!user || isLoading) return;

    // Check if email changed
    const currentData = profileData || user;
    const emailChanged = email.trim() !== (currentData.email || '');
    if (emailChanged) {
      setShowEmailConfirmDialog(true);
      return;
    }

    // Proceed with update
    await performUpdate();
  };

  const performUpdate = async () => {
    if (!user) return;

    setIsLoading(true);
    setError(null);
    setSuccessMessage(null);

    try {
      const currentData = profileData || user;
      const emailChanged = email.trim() !== (currentData.email || '');

      const updatedUser = await usersApi.updateProfile({
        userId: user.userId!,
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        email: email.trim(),
        phone: phone.trim() || null,
        roleId: user.tenantRoles?.[0]?.roleId || 0,
      });

      // Update local profile data
      setProfileData(updatedUser);

      // Update Redux state - construct clean user object with updated values
      dispatch(setUser({
        userId: updatedUser.userId,
        firstName: updatedUser.firstName,
        lastName: updatedUser.lastName,
        email: updatedUser.email,
        phone: updatedUser.phone,
        tenantRoles: updatedUser.tenantRoles,
        active: updatedUser.active ?? undefined,
        isVerified: updatedUser.isVerified ?? undefined,
        createdOn: updatedUser.createdOn,
        fullName: updatedUser.fullName,
      }));

      // Reset dirty flag
      setIsDirty(false);

      // Show success message
      setSuccessMessage(
        emailChanged
          ? 'Profile updated! A verification email has been sent to your new email address.'
          : 'Profile updated successfully!'
      );
    } catch (err) {
      setError(err as AxiosError);
    } finally {
      setIsLoading(false);
      setShowEmailConfirmDialog(false);
    }
  };

  const handleEmailConfirmCancel = () => {
    setShowEmailConfirmDialog(false);
  };

  const handleEmailConfirmContinue = async () => {
    await performUpdate();
  };

  const handleChangePassword = () => {
    navigate('/change-password');
  };

  if (!user || isLoadingProfile) {
    return (
      <Box>
        <Typography variant="h4" component="h1" gutterBottom>
          My Profile
        </Typography>
        <Paper sx={{ p: 3, mt: 2 }}>
          <Typography>Loading profile...</Typography>
        </Paper>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        My Profile
      </Typography>

      {/* Success Alert */}
      {successMessage && (
        <Alert severity="success" onClose={() => setSuccessMessage(null)} sx={{ mb: 2 }}>
          {successMessage}
        </Alert>
      )}

      {/* Error Alert */}
      {error && <ErrorAlert error={error} onClose={() => setError(null)} />}

      <Paper sx={{ p: 3, mt: 2 }}>
        <Typography variant="h6" gutterBottom>
          {profileData?.fullName || `${profileData?.firstName || ''} ${profileData?.lastName || ''}`.trim() || user.fullName || 'User'}
          &nbsp;
          <i>{profileData?.email ?? user.email}</i>
        </Typography>

        {/* Read-only Information */}
        <Box sx={{ mb: 3 }}>
          {/* Display tenant-role assignments */}
          <TenantRoleList 
            tenantRoles={profileData?.tenantRoles} 
            title="Tenant & Role Assignments"
          />

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1, mt: 2 }}>
            <Typography variant="body1">
              <strong>Status:</strong>
            </Typography>
            <Chip
              label={(profileData?.active ?? user.active) ? 'Active' : 'Inactive'}
              color={(profileData?.active ?? user.active) ? 'success' : 'default'}
              size="small"
            />
          </Box>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Typography variant="body1">
              <strong>Email Verification:</strong>
            </Typography>
            <Chip
              label={(profileData?.isVerified ?? user.isVerified) ? 'Verified' : 'Not Verified'}
              color={(profileData?.isVerified ?? user.isVerified) ? 'success' : 'warning'}
              size="small"
              icon={(profileData?.isVerified ?? user.isVerified) ? <CheckCircle /> : <Cancel />}
            />
          </Box>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1, mt: 1 }}>
            <Typography variant="body1">
              <strong>Created On:</strong>
            </Typography>
            <Typography variant="body2">
              {profileData?.createdOn ? new Date(profileData.createdOn).toLocaleString() : '-'}
            </Typography>
          </Box>
        </Box>

        {/* Edit Profile Form */}
        <Typography variant="h6" gutterBottom sx={{ mt: 4 }}>
          Edit Profile
        </Typography>

        <form onSubmit={handleSubmit}>
          <TextField
            label="First Name"
            value={firstName}
            onChange={(e) => handleFieldChange(setFirstName, e.target.value)}
            fullWidth
            required
            margin="normal"
            disabled={isLoading}
          />

          <TextField
            label="Last Name"
            value={lastName}
            onChange={(e) => handleFieldChange(setLastName, e.target.value)}
            fullWidth
            required
            margin="normal"
            disabled={isLoading}
          />

          <TextField
            label="Email"
            type="email"
            value={email}
            onChange={(e) => handleFieldChange(setEmail, e.target.value)}
            fullWidth
            required
            margin="normal"
            disabled={isLoading}
            inputProps={{
              inputMode: 'email',
              autoCapitalize: 'none',
              spellCheck: false,
              autoCorrect: 'off',
            }}
          />

          <TextField
            label="Phone"
            value={phone}
            onChange={(e) => handleFieldChange(setPhone, e.target.value)}
            fullWidth
            margin="normal"
            disabled={isLoading}
            placeholder="Optional"
          />

          <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
            <Button
              type="submit"
              variant="contained"
              disabled={!isDirty || isLoading}
            >
              {isLoading ? 'Saving...' : 'Save Changes'}
            </Button>
            <Button
              type="button"
              variant="outlined"
              onClick={handleCancel}
              disabled={isLoading}
            >
              Cancel
            </Button>
          </Box>
        </form>

        {/* Change Password Link */}
        <Box sx={{ mt: 4, pt: 3, borderTop: 1, borderColor: 'divider' }}>
          <Typography variant="h6" gutterBottom>
            Account Security
          </Typography>
          <Link
            component="button"
            variant="body2"
            onClick={handleChangePassword}
            sx={{ cursor: 'pointer' }}
          >
            Change Password
          </Link>
        </Box>
      </Paper>

      {/* Unsaved Changes Dialog */}
      {blocker.state === 'blocked' && (
        <Dialog open onClose={() => blocker.reset()}>
          <DialogTitle>Unsaved Changes</DialogTitle>
          <DialogContent>
            <DialogContentText>
              You have unsaved changes. Are you sure you want to leave?
            </DialogContentText>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => blocker.reset()}>Cancel</Button>
            <Button onClick={() => blocker.proceed()} color="error">
              Leave
            </Button>
          </DialogActions>
        </Dialog>
      )}

      {/* Email Change Confirmation Dialog */}
      {showEmailConfirmDialog && (
        <Dialog open onClose={handleEmailConfirmCancel}>
          <DialogTitle>Confirm Email Change</DialogTitle>
          <DialogContent>
            <DialogContentText>
              A verification email will be sent to{' '}
              <strong>{email.trim()}</strong>. You can login with the new email
              only after you verify it.
            </DialogContentText>
          </DialogContent>
          <DialogActions>
            <Button onClick={handleEmailConfirmCancel}>Cancel</Button>
            <Button onClick={handleEmailConfirmContinue} variant="contained">
              Continue
            </Button>
          </DialogActions>
        </Dialog>
      )}
    </Box>
  );
};

export default ProfilePage;
