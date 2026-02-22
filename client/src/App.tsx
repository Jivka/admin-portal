import { useEffect } from 'react';
import { RouterProvider } from 'react-router-dom';
import { ThemeProvider, CssBaseline, createTheme } from '@mui/material';
import { router } from './routes';
import { useAppDispatch } from './store/hooks';
import { initAuth } from './store/authSlice';

// Create default MUI theme (can be customized later)
const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
    background: {
      default: '#f5f5f5',
    },
  },
});

function App() {
  const dispatch = useAppDispatch();

  // Restore auth state from existing SessionId cookie on every app load / page reload
  useEffect(() => {
    dispatch(initAuth());
  }, [dispatch]);

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <RouterProvider router={router} />
    </ThemeProvider>
  );
}

export default App;
