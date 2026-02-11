import { Box, Container, Typography } from '@mui/material';

const SignupPage = () => {
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
          Sign Up
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Sign up page - to be implemented
        </Typography>
      </Box>
    </Container>
  );
};

export default SignupPage;
