import { Box, Typography, Paper } from '@mui/material';

const TenantProfilePage = () => {
  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        Tenant Profile
      </Typography>

      <Paper sx={{ p: 3 }}>
        <Typography variant="body1" color="text.secondary">
          Tenant profile management (edit details, contacts, status) - to be implemented
        </Typography>
      </Paper>
    </Box>
  );
};

export default TenantProfilePage;
