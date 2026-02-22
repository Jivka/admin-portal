import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type { SigninRequest, SigninResponse } from '../types';
import { authApi } from '../api';

// Auth state interface
interface AuthState {
  user: SigninResponse | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isInitializing: boolean;
  error: string | null;
}

// Initial state
const initialState: AuthState = {
  user: null,
  isAuthenticated: false,
  isLoading: false,
  isInitializing: true,
  error: null,
};

// Async thunks
export const signIn = createAsyncThunk(
  'auth/signIn',
  async (credentials: SigninRequest, { rejectWithValue }) => {
    try {
      const response = await authApi.signIn(credentials);
      return response;
    } catch (error: unknown) {
      const err = error as { 
        response?: { 
          data?: { 
            'error-message'?: string;
            'error-code'?: string;
            error?: { message?: string }; 
            message?: string 
          } 
        }; 
        message?: string 
      };
      // Try hyphenated format first (error-message), then ApiResult format, then fallback
      const errorMessage = err.response?.data?.['error-message']
        || err.response?.data?.error?.message 
        || err.response?.data?.message 
        || err.message 
        || 'Sign in failed';
      return rejectWithValue(errorMessage);
    }
  }
);

export const signOut = createAsyncThunk(
  'auth/signOut',
  async (_, { rejectWithValue }) => {
    try {
      await authApi.logout();
    } catch (error: unknown) {
      const err = error as { 
        response?: { 
          data?: { 
            'error-message'?: string;
            error?: { message?: string }; 
            message?: string 
          } 
        }; 
        message?: string 
      };
      const errorMessage = err.response?.data?.['error-message']
        || err.response?.data?.error?.message 
        || err.response?.data?.message 
        || err.message 
        || 'Sign out failed';
      return rejectWithValue(errorMessage);
    }
  }
);

export const refreshToken = createAsyncThunk(
  'auth/refreshToken',
  async (_, { rejectWithValue }) => {
    try {
      const response = await authApi.refreshToken();
      return response;
    } catch (error: unknown) {
      const err = error as { 
        response?: { 
          data?: { 
            'error-message'?: string;
            error?: { message?: string }; 
            message?: string 
          } 
        }; 
        message?: string 
      };
      const errorMessage = err.response?.data?.['error-message']
        || err.response?.data?.error?.message 
        || err.response?.data?.message 
        || err.message 
        || 'Token refresh failed';
      return rejectWithValue(errorMessage);
    }
  }
);

/**
 * Called once on app startup to restore auth state from an existing SessionId cookie.
 * Silently succeeds or fails – never throws.
 */
export const initAuth = createAsyncThunk(
  'auth/initAuth',
  async (_, { rejectWithValue }) => {
    try {
      const response = await authApi.refreshToken();
      return response;
    } catch {
      return rejectWithValue(null);
    }
  }
);

// Auth slice
const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
    setUser: (state, action: PayloadAction<SigninResponse>) => {
      state.user = action.payload;
      state.isAuthenticated = true;
    },
    clearAuth: (state) => {
      state.user = null;
      state.isAuthenticated = false;
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    // Sign In
    builder
      .addCase(signIn.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(signIn.fulfilled, (state, action) => {
        state.isLoading = false;
        state.user = action.payload;
        state.isAuthenticated = true;
        state.error = null;
      })
      .addCase(signIn.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
        state.isAuthenticated = false;
      });

    // Sign Out
    builder
      .addCase(signOut.pending, (state) => {
        state.isLoading = true;
      })
      .addCase(signOut.fulfilled, (state) => {
        state.isLoading = false;
        state.user = null;
        state.isAuthenticated = false;
        state.error = null;
      })
      .addCase(signOut.rejected, (state, action) => {
        state.isLoading = false;
        // Still clear auth on logout failure for security
        state.user = null;
        state.isAuthenticated = false;
        state.error = action.payload as string;
      });

    // Refresh Token
    builder
      .addCase(refreshToken.pending, (state) => {
        state.isLoading = true;
      })
      .addCase(refreshToken.fulfilled, (state, action) => {
        state.isLoading = false;
        state.user = action.payload;
        state.isAuthenticated = true;
        state.error = null;
      })
      .addCase(refreshToken.rejected, (state, action) => {
        state.isLoading = false;
        state.user = null;
        state.isAuthenticated = false;
        state.error = action.payload as string;
      });

    // Init Auth (session restore on app load)
    builder
      .addCase(initAuth.pending, (state) => {
        state.isInitializing = true;
      })
      .addCase(initAuth.fulfilled, (state, action) => {
        state.isInitializing = false;
        state.user = action.payload;
        state.isAuthenticated = true;
        state.error = null;
      })
      .addCase(initAuth.rejected, (state) => {
        state.isInitializing = false;
        // No valid session – user stays unauthenticated
      });
  },
});

export const { clearError, setUser, clearAuth } = authSlice.actions;
export default authSlice.reducer;
