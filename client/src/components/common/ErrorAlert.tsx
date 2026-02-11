import { Alert, AlertTitle, List, ListItem, ListItemText } from '@mui/material';
import type { AxiosError } from 'axios';

interface ErrorAlertProps {
  error: AxiosError | null;
  onClose?: () => void;
}

interface ApiErrorResponse {
  // Hyphenated format (error-message, error-code)
  'error-message'?: string;
  'error-code'?: string;
  // Camel case format (error.message)
  error?: {
    code: string;
    message: string;
  };
  // Array format
  errors?: Array<{
    code: string;
    message: string;
  }>;
}

/**
 * ErrorAlert component for displaying backend API errors
 * Handles both single error and error array formats from ApiResult
 */
export const ErrorAlert = ({ error, onClose }: ErrorAlertProps) => {
  if (!error) return null;

  // Extract error data from axios error response
  const errorData = error.response?.data as ApiErrorResponse | undefined;

  // Handle hyphenated format: { "error-message": "...", "error-code": "..." }
  if (errorData?.['error-message']) {
    return (
      <Alert severity="error" onClose={onClose} sx={{ mb: 2 }}>
        {errorData['error-message']}
      </Alert>
    );
  }

  // Handle single error format: { error: { code, message } }
  if (errorData?.error?.message) {
    return (
      <Alert severity="error" onClose={onClose} sx={{ mb: 2 }}>
        {errorData.error.message}
      </Alert>
    );
  }

  // Handle error array format: { errors: [{ code, message }, ...] }
  if (errorData?.errors && errorData.errors.length > 0) {
    // If only one error, display it directly
    if (errorData.errors.length === 1) {
      return (
        <Alert severity="error" onClose={onClose} sx={{ mb: 2 }}>
          {errorData.errors[0].message}
        </Alert>
      );
    }

    // Multiple errors - display as list
    return (
      <Alert severity="error" onClose={onClose} sx={{ mb: 2 }}>
        <AlertTitle>The following errors occurred:</AlertTitle>
        <List dense sx={{ pt: 0 }}>
          {errorData.errors.map((err, index) => (
            <ListItem key={index} sx={{ px: 0, py: 0.5 }}>
              <ListItemText primary={err.message} />
            </ListItem>
          ))}
        </List>
      </Alert>
    );
  }

  // Fallback for generic axios errors or network issues
  if (error.message) {
    return (
      <Alert severity="error" onClose={onClose} sx={{ mb: 2 }}>
        {error.message === 'Network Error' 
          ? 'Unable to connect to the server. Please check your connection and try again.' 
          : error.message}
      </Alert>
    );
  }

  // Ultimate fallback
  return (
    <Alert severity="error" onClose={onClose} sx={{ mb: 2 }}>
      An unexpected error occurred. Please try again.
    </Alert>
  );
};

export default ErrorAlert;
