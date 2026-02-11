import { Box, Container, Typography } from '@mui/material';

const VerifyEmailPage = () => {
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
          Verify Email
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Email verification page - to be implemented
        </Typography>
      </Box>
    </Container>
  );
};

export default VerifyEmailPage;
