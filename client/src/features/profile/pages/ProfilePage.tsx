import { Box, Typography, Paper } from '@mui/material';
import { useAppSelector } from '../../../store/hooks';

const ProfilePage = () => {
  const { user } = useAppSelector((state) => state.auth);

  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        My Profile
      </Typography>

      <Paper sx={{ p: 3, mt: 2 }}>
        <Typography variant="h6" gutterBottom>
          User Information
        </Typography>
        {user && (
          <Box>
            <Typography variant="body1">
              <strong>Name:</strong> {user.firstName} {user.lastName}
            </Typography>
            <Typography variant="body1">
              <strong>Email:</strong> {user.email}
            </Typography>
            <Typography variant="body1">
              <strong>Phone:</strong> {user.phone || 'Not set'}
            </Typography>
            <Typography variant="body1">
              <strong>Role:</strong> {user.roleName}
            </Typography>
            <Typography variant="body1">
              <strong>Status:</strong> {user.active ? 'Active' : 'Inactive'}
            </Typography>
            <Typography variant="body1">
              <strong>Email Verified:</strong> {user.isVerified ? 'Yes' : 'No'}
            </Typography>
          </Box>
        )}
        <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
          Profile editing form - to be implemented
        </Typography>
      </Paper>
    </Box>
  );
};

export default ProfilePage;
