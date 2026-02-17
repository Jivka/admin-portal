import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { TenantOutput } from '../types';
import { tenantsApi } from '../api';

// Tenants state interface
interface TenantsState {
  tenants: TenantOutput[];
  selectedTenant: TenantOutput | null;
  isLoading: boolean;
  error: string | null;
  isLoaded: boolean;
  pagination: {
    count: number;
    page: number;
    size: number;
  };
}

// Initial state
const initialState: TenantsState = {
  tenants: [],
  selectedTenant: null,
  isLoading: false,
  error: null,
  isLoaded: false,
  pagination: {
    count: 0,
    page: 1,
    size: 10,
  },
};

// Async thunks
export const fetchTenants = createAsyncThunk(
  'tenants/fetchTenants',
  async (params: { page?: number; size?: number; name?: string; sort?: string } | undefined, { rejectWithValue }) => {
    try {
      const response = await tenantsApi.getTenants(params);
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to fetch tenants');
    }
  }
);

export const fetchMyTenants = createAsyncThunk(
  'tenants/fetchMyTenants',
  async (_, { rejectWithValue }) => {
    try {
      const response = await tenantsApi.getMyTenants();
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to fetch your tenants');
    }
  }
);

export const fetchTenantById = createAsyncThunk(
  'tenants/fetchTenantById',
  async (tenantId: number, { rejectWithValue }) => {
    try {
      const response = await tenantsApi.getTenantById(tenantId);
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to fetch tenant');
    }
  }
);

export const createTenant = createAsyncThunk(
  'tenants/createTenant',
  async (data: { tenantName: string; tenantBIC: string; tenantType: number; ownership: number; domain?: string; summary?: string; logoUrl?: string }, { rejectWithValue }) => {
    try {
      const response = await tenantsApi.createTenant(data);
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to create tenant');
    }
  }
);

export const updateTenant = createAsyncThunk(
  'tenants/updateTenant',
  async ({ tenantId, data, isSystemAdmin }: { tenantId: number; data: { tenantName: string; tenantBIC: string; tenantType: number; ownership: number; domain?: string; summary?: string; logoUrl?: string }; isSystemAdmin: boolean }, { rejectWithValue }) => {
    try {
      // Use appropriate API endpoint based on role
      const response = isSystemAdmin 
        ? await tenantsApi.updateTenant(tenantId, data)
        : await tenantsApi.updateTenantProfile(tenantId, data);
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to update tenant');
    }
  }
);

export const deleteTenant = createAsyncThunk(
  'tenants/deleteTenant',
  async (tenantId: number, { rejectWithValue }) => {
    try {
      await tenantsApi.deleteTenant(tenantId);
      return tenantId;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to delete tenant');
    }
  }
);

export const toggleTenantStatus = createAsyncThunk(
  'tenants/toggleTenantStatus',
  async ({ tenantId, active }: { tenantId: number; active: boolean }, { rejectWithValue }) => {
    try {
      const response = await tenantsApi.toggleTenantStatus(tenantId, active);
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to toggle tenant status');
    }
  }
);

// Tenants slice
const tenantsSlice = createSlice({
  name: 'tenants',
  initialState,
  reducers: {
    clearTenants: (state) => {
      state.tenants = [];
      state.selectedTenant = null;
      state.isLoaded = false;
      state.error = null;
    },
    clearSelectedTenant: (state) => {
      state.selectedTenant = null;
    },
  },
  extraReducers: (builder) => {
    // Fetch Tenants (paginated for System Admin)
    builder
      .addCase(fetchTenants.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchTenants.fulfilled, (state, action) => {
        state.isLoading = false;
        state.tenants = action.payload.tenants || [];
        state.pagination = {
          count: action.payload.count || 0,
          page: action.payload.page || 1,
          size: action.payload.size || 10,
        };
        state.isLoaded = true;
        state.error = null;
      })
      .addCase(fetchTenants.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Fetch My Tenants (for Tenant Admin)
    builder
      .addCase(fetchMyTenants.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchMyTenants.fulfilled, (state, action) => {
        state.isLoading = false;
        state.tenants = action.payload;
        state.pagination = {
          count: action.payload.length,
          page: 1,
          size: action.payload.length,
        };
        state.isLoaded = true;
        state.error = null;
      })
      .addCase(fetchMyTenants.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Fetch Tenant By ID
    builder
      .addCase(fetchTenantById.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchTenantById.fulfilled, (state, action) => {
        state.isLoading = false;
        state.selectedTenant = action.payload;
        state.error = null;
      })
      .addCase(fetchTenantById.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Create Tenant
    builder
      .addCase(createTenant.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(createTenant.fulfilled, (state, action) => {
        state.isLoading = false;
        state.tenants.push(action.payload);
        state.pagination.count += 1;
        state.error = null;
      })
      .addCase(createTenant.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Update Tenant
    builder
      .addCase(updateTenant.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(updateTenant.fulfilled, (state, action) => {
        state.isLoading = false;
        const index = state.tenants.findIndex(t => t.tenantId === action.payload.tenantId);
        if (index !== -1) {
          state.tenants[index] = action.payload;
        }
        if (state.selectedTenant?.tenantId === action.payload.tenantId) {
          state.selectedTenant = action.payload;
        }
        state.error = null;
      })
      .addCase(updateTenant.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Delete Tenant
    builder
      .addCase(deleteTenant.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(deleteTenant.fulfilled, (state, action) => {
        state.isLoading = false;
        state.tenants = state.tenants.filter(t => t.tenantId !== action.payload);
        state.pagination.count -= 1;
        if (state.selectedTenant?.tenantId === action.payload) {
          state.selectedTenant = null;
        }
        state.error = null;
      })
      .addCase(deleteTenant.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Toggle Tenant Status
    builder
      .addCase(toggleTenantStatus.pending, (state) => {
        state.error = null;
      })
      .addCase(toggleTenantStatus.fulfilled, (state, action) => {
        const index = state.tenants.findIndex(t => t.tenantId === action.payload.tenantId);
        if (index !== -1) {
          state.tenants[index] = action.payload;
        }
        if (state.selectedTenant?.tenantId === action.payload.tenantId) {
          state.selectedTenant = action.payload;
        }
        state.error = null;
      })
      .addCase(toggleTenantStatus.rejected, (state, action) => {
        state.error = action.payload as string;
      });
  },
});

export const { clearTenants, clearSelectedTenant } = tenantsSlice.actions;
export default tenantsSlice.reducer;
