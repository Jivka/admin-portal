import apiClient from './client';
import type {
  UserOutput,
  UsersResponse,
  CreateUserRequest,
  EditUserRequest,
  ChangePasswordRequest,
} from '../types';

// Pagination params interface
interface PaginationParams {
  tenantId?: number;
  page?: number;
  size?: number;
  name?: string;
  sort?: string;
}

export const usersApi = {
  // ============================================
  // System Admin - User Management
  // ============================================

  /**
   * Get paginated list of all users (System Admin only)
   * Optionally filter by tenantId
   */
  getUsers: async (params?: PaginationParams): Promise<UsersResponse> => {
    const response = await apiClient.get<UsersResponse>('/api/users', { params });
    return response.data;
  },

  /**
   * Get users by tenant (System Admin only)
   */
  getUsersByTenant: async (tenantId: number): Promise<UserOutput[]> => {
    const response = await apiClient.get<UserOutput[]>(`/api/users/tenantId=${tenantId}`);
    return response.data;
  },

  /**
   * Get user by ID (System Admin only)
   */
  getUserById: async (userId: number): Promise<UserOutput> => {
    const response = await apiClient.get<UserOutput>(`/api/users/${userId}`);
    return response.data;
  },

  /**
   * Create new user (System Admin only)
   */
  createUser: async (data: CreateUserRequest): Promise<UserOutput> => {
    const response = await apiClient.post<UserOutput>('/api/users', data);
    return response.data;
  },

  /**
   * Edit user (System Admin only)
   */
  updateUser: async (data: EditUserRequest): Promise<UserOutput> => {
    const response = await apiClient.put<UserOutput>('/api/users', data);
    return response.data;
  },

  /**
   * Toggle user active status (System Admin only)
   */
  toggleUserStatus: async (userId: number, active: boolean): Promise<UserOutput> => {
    const response = await apiClient.patch<UserOutput>(`/api/users/${userId}`, null, {
      params: { active },
    });
    return response.data;
  },

  /**
   * Delete user (System Admin only)
   */
  deleteUser: async (userId: number): Promise<boolean> => {
    const response = await apiClient.delete<boolean>(`/api/users/${userId}`);
    return response.data;
  },

  // ============================================
  // Tenant Admin - Tenant User Management
  // ============================================

  /**
   * Get paginated list of tenant users (Tenant Admin only)
   * Optionally filter by tenantId (if not provided, returns all users from admin's tenants)
   */
  getTenantUsers: async (params?: PaginationParams): Promise<UsersResponse> => {
    const response = await apiClient.get<UsersResponse>('/api/tenants/users', { params });
    return response.data;
  },

  /**
   * Get tenant user by ID (Tenant Admin only)
   */
  getTenantUserById: async (userId: number, tenantId?: number): Promise<UserOutput> => {
    const url = tenantId ? `/api/tenants/users/${tenantId}/${userId}` : `/api/tenants/users/${userId}`;
    const response = await apiClient.get<UserOutput>(url);
    return response.data;
  },

  /**
   * Create tenant user (Tenant Admin only)
   */
  createTenantUser: async (data: CreateUserRequest): Promise<UserOutput> => {
    const response = await apiClient.post<UserOutput>(`/api/tenants/users/${data.tenantId}`, data);
    return response.data;
  },

  /**
   * Edit tenant user (Tenant Admin only)
   */
  updateTenantUser: async (data: EditUserRequest, tenantId: number): Promise<UserOutput> => {
    const response = await apiClient.put<UserOutput>(`/api/tenants/users/${tenantId}`, data);
    return response.data;
  },

  /**
   * Toggle tenant user status (Tenant Admin only)
   */
  toggleTenantUserStatus: async (userId: number, active: boolean, tenantId: number): Promise<UserOutput> => {
    const response = await apiClient.patch<UserOutput>(`/api/tenants/users/${tenantId}/${userId}`, null, {
      params: { active },
    });
    return response.data;
  },

  /**
   * Delete tenant user (Tenant Admin only)
   */
  deleteTenantUser: async (userId: number, tenantId: number): Promise<boolean> => {
    const response = await apiClient.delete<boolean>(`/api/tenants/users/${tenantId}/${userId}`);
    return response.data;
  },

  // ============================================
  // User Profile - Self Service
  // ============================================

  /**
   * Get own profile
   */
  getProfile: async (userId: number): Promise<UserOutput> => {
    const response = await apiClient.get<UserOutput>(`/api/users/profile/${userId}`);
    return response.data;
  },

  /**
   * Update own profile
   */
  updateProfile: async (data: EditUserRequest): Promise<UserOutput> => {
    const response = await apiClient.put<UserOutput>('/api/users/profile', data);
    return response.data;
  },

  /**
   * Change own password
   */
  changePassword: async (data: ChangePasswordRequest): Promise<string> => {
    const response = await apiClient.put<string>('/api/users/profile/change-password', data);
    return response.data;
  },

  /**
   * Toggle own status
   */
  toggleOwnStatus: async (userId: number, active: boolean): Promise<UserOutput> => {
    const response = await apiClient.patch<UserOutput>(`/api/users/profile/${userId}/status`, null, {
      params: { active },
    });
    return response.data;
  },

  /**
   * Delete own account
   */
  deleteOwnAccount: async (userId: number): Promise<boolean> => {
    const response = await apiClient.delete<boolean>(`/api/users/profile/${userId}`);
    return response.data;
  },
};

export default usersApi;
