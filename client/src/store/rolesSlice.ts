import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { RoleOutput } from '../types';
import { rolesApi } from '../api';

// Roles state interface
interface RolesState {
  roles: RoleOutput[];
  isLoading: boolean;
  error: string | null;
  isLoaded: boolean;
}

// Initial state
const initialState: RolesState = {
  roles: [],
  isLoading: false,
  error: null,
  isLoaded: false,
};

// Async thunks
export const fetchAllRoles = createAsyncThunk(
  'roles/fetchAllRoles',
  async (_, { rejectWithValue }) => {
    try {
      const response = await rolesApi.getAllRoles();
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to fetch roles');
    }
  }
);

export const fetchTenantRoles = createAsyncThunk(
  'roles/fetchTenantRoles',
  async (_, { rejectWithValue }) => {
    try {
      const response = await rolesApi.getTenantRoles();
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to fetch tenant roles');
    }
  }
);

// Roles slice
const rolesSlice = createSlice({
  name: 'roles',
  initialState,
  reducers: {
    clearRoles: (state) => {
      state.roles = [];
      state.isLoaded = false;
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    // Fetch All Roles (System Admin)
    builder
      .addCase(fetchAllRoles.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchAllRoles.fulfilled, (state, action) => {
        state.isLoading = false;
        state.roles = action.payload;
        state.isLoaded = true;
        state.error = null;
      })
      .addCase(fetchAllRoles.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Fetch Tenant Roles (Tenant Admin)
    builder
      .addCase(fetchTenantRoles.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchTenantRoles.fulfilled, (state, action) => {
        state.isLoading = false;
        state.roles = action.payload;
        state.isLoaded = true;
        state.error = null;
      })
      .addCase(fetchTenantRoles.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export const { clearRoles } = rolesSlice.actions;
export default rolesSlice.reducer;
