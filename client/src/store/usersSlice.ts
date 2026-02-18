import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { UserOutput, CreateUserRequest, EditUserRequest } from '../types';
import { usersApi } from '../api';

// Users state interface
interface UsersState {
  users: UserOutput[];
  selectedUser: UserOutput | null;
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
const initialState: UsersState = {
  users: [],
  selectedUser: null,
  isLoading: false,
  error: null,
  isLoaded: false,
  pagination: {
    count: 0,
    page: 1,
    size: 10,
  },
};

// Async thunks for System Admin
export const fetchUsers = createAsyncThunk(
  'users/fetchUsers',
  async (params: { tenantId?: number; page?: number; size?: number; name?: string; sort?: string } | undefined, { rejectWithValue }) => {
    try {
      const response = await usersApi.getUsers(params);
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to fetch users');
    }
  }
);

// Async thunks for Tenant Admin
export const fetchTenantUsers = createAsyncThunk(
  'users/fetchTenantUsers',
  async (params: { tenantId?: number; page?: number; size?: number; name?: string; sort?: string } | undefined, { rejectWithValue }) => {
    try {
      const response = await usersApi.getTenantUsers(params);
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to fetch tenant users');
    }
  }
);

export const fetchUserById = createAsyncThunk(
  'users/fetchUserById',
  async ({ userId, isSystemAdmin }: { userId: number; isSystemAdmin: boolean }, { rejectWithValue }) => {
    try {
      const response = isSystemAdmin 
        ? await usersApi.getUserById(userId)
        : await usersApi.getTenantUserById(userId);
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to fetch user');
    }
  }
);

export const createUser = createAsyncThunk(
  'users/createUser',
  async ({ data, isSystemAdmin }: { data: CreateUserRequest; isSystemAdmin: boolean }, { rejectWithValue }) => {
    try {
      const response = isSystemAdmin 
        ? await usersApi.createUser(data)
        : await usersApi.createTenantUser(data);
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to create user');
    }
  }
);

export const updateUser = createAsyncThunk(
  'users/updateUser',
  async ({ data, isSystemAdmin, tenantId }: { data: EditUserRequest; isSystemAdmin: boolean; tenantId?: number }, { rejectWithValue }) => {
    try {
      if (isSystemAdmin) {
        const response = await usersApi.updateUser(data);
        return response;
      } else {
        if (!tenantId) {
          return rejectWithValue('Tenant ID is required for Tenant Admin operations');
        }
        const response = await usersApi.updateTenantUser(data, tenantId);
        return response;
      }
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to update user');
    }
  }
);

export const deleteUser = createAsyncThunk(
  'users/deleteUser',
  async ({ userId, isSystemAdmin, tenantId }: { userId: number; isSystemAdmin: boolean; tenantId?: number }, { rejectWithValue }) => {
    try {
      if (isSystemAdmin) {
        await usersApi.deleteUser(userId);
      } else {
        if (!tenantId) {
          return rejectWithValue('Tenant ID is required for Tenant Admin operations');
        }
        await usersApi.deleteTenantUser(userId, tenantId);
      }
      return userId;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to delete user');
    }
  }
);

export const toggleUserStatus = createAsyncThunk(
  'users/toggleUserStatus',
  async ({ userId, active, isSystemAdmin, tenantId }: { userId: number; active: boolean; isSystemAdmin: boolean; tenantId?: number }, { rejectWithValue }) => {
    try {
      if (isSystemAdmin) {
        const response = await usersApi.toggleUserStatus(userId, active);
        return response;
      } else {
        if (!tenantId) {
          return rejectWithValue('Tenant ID is required for Tenant Admin operations');
        }
        const response = await usersApi.toggleTenantUserStatus(userId, active, tenantId);
        return response;
      }
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } }; message?: string };
      return rejectWithValue(err.response?.data?.message || err.message || 'Failed to toggle user status');
    }
  }
);

// Users slice
const usersSlice = createSlice({
  name: 'users',
  initialState,
  reducers: {
    clearUsers: (state) => {
      state.users = [];
      state.selectedUser = null;
      state.isLoaded = false;
      state.error = null;
    },
    clearSelectedUser: (state) => {
      state.selectedUser = null;
    },
  },
  extraReducers: (builder) => {
    // Fetch Users (System Admin - paginated)
    builder
      .addCase(fetchUsers.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchUsers.fulfilled, (state, action) => {
        state.isLoading = false;
        state.users = action.payload.users || [];
        state.pagination = {
          count: action.payload.count || 0,
          page: action.payload.page || 1,
          size: action.payload.size || 10,
        };
        state.isLoaded = true;
        state.error = null;
      })
      .addCase(fetchUsers.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Fetch Tenant Users (Tenant Admin - paginated)
    builder
      .addCase(fetchTenantUsers.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchTenantUsers.fulfilled, (state, action) => {
        state.isLoading = false;
        state.users = action.payload.users || [];
        state.pagination = {
          count: action.payload.count || 0,
          page: action.payload.page || 1,
          size: action.payload.size || 10,
        };
        state.isLoaded = true;
        state.error = null;
      })
      .addCase(fetchTenantUsers.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Fetch User By ID
    builder
      .addCase(fetchUserById.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchUserById.fulfilled, (state, action) => {
        state.isLoading = false;
        state.selectedUser = action.payload;
        state.error = null;
      })
      .addCase(fetchUserById.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Create User
    builder
      .addCase(createUser.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(createUser.fulfilled, (state, action) => {
        state.isLoading = false;
        if (action.payload) {
          state.users = [action.payload, ...state.users];
          state.pagination.count += 1;
        }
        state.error = null;
      })
      .addCase(createUser.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Update User
    builder
      .addCase(updateUser.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(updateUser.fulfilled, (state, action) => {
        state.isLoading = false;
        if (action.payload) {
          const index = state.users.findIndex(u => u.userId === action.payload.userId);
          if (index !== -1) {
            state.users[index] = action.payload;
          }
        }
        state.error = null;
      })
      .addCase(updateUser.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Delete User
    builder
      .addCase(deleteUser.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(deleteUser.fulfilled, (state, action) => {
        state.isLoading = false;
        state.users = state.users.filter(u => u.userId !== action.payload);
        state.pagination.count -= 1;
        state.error = null;
      })
      .addCase(deleteUser.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Toggle User Status
    builder
      .addCase(toggleUserStatus.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(toggleUserStatus.fulfilled, (state, action) => {
        state.isLoading = false;
        if (action.payload) {
          const index = state.users.findIndex(u => u.userId === action.payload.userId);
          if (index !== -1) {
            state.users[index] = action.payload;
          }
        }
        state.error = null;
      })
      .addCase(toggleUserStatus.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export const { clearUsers, clearSelectedUser } = usersSlice.actions;
export default usersSlice.reducer;
