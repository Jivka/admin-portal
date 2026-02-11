import { Box, Container, Typography } from '@mui/material';

const ForgotPasswordPage = () => {
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
          Forgot Password
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Forgot password page - to be implemented
        </Typography>
      </Box>
    </Container>
  );
};

export default ForgotPasswordPage;
