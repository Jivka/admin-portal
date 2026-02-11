import { Box, Container, Typography } from '@mui/material';

const ResetPasswordPage = () => {
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
        <Typography variant="h4" component="h1" gutterBottom>
          Reset Password
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Reset password page - to be implemented
        </Typography>
      </Box>
    </Container>
  );
};

export default ResetPasswordPage;
