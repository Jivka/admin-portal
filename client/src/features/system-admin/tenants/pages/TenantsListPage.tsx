import { Box, Typography, Paper, Button } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';

const TenantsListPage = () => {
  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1">
          Tenants Management
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />}>
          Add Tenant
        </Button>
      </Box>

      <Paper sx={{ p: 3 }}>
        <Typography variant="body1" color="text.secondary">
          Tenants list with search, pagination, and CRUD operations - to be implemented
        </Typography>
      </Paper>
    </Box>
  );
};

export default TenantsListPage;
