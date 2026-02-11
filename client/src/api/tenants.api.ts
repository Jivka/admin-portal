import apiClient from './client';
import type {
  TenantOutput,
  TenantsResponse,
  TenantRequest,
  TenantContactsRequest,
  TenantContactsResponse,
} from '../types';

// Pagination params interface
interface PaginationParams {
  page?: number;
  size?: number;
  name?: string;
  sort?: string;
}

export const tenantsApi = {
  // ============================================
  // System Admin - Tenant Management
  // ============================================

  /**
   * Get all tenants without pagination (System Admin only)
   */
  getAllTenants: async (): Promise<TenantOutput[]> => {
    const response = await apiClient.get<TenantOutput[]>('/api/tenants/all');
    return response.data;
  },

  /**
   * Get paginated list of tenants (System Admin only)
   */
  getTenants: async (params?: PaginationParams): Promise<TenantsResponse> => {
    const response = await apiClient.get<TenantsResponse>('/api/tenants', { params });
    return response.data;
  },

  /**
   * Get tenant by ID (System Admin only)
   */
  getTenantById: async (tenantId: number): Promise<TenantOutput> => {
    const response = await apiClient.get<TenantOutput>(`/api/tenants/${tenantId}`);
    return response.data;
  },

  /**
   * Create new tenant (System Admin only)
   */
  createTenant: async (data: TenantRequest): Promise<TenantOutput> => {
    const response = await apiClient.post<TenantOutput>('/api/tenants', data);
    return response.data;
  },

  /**
   * Update tenant (System Admin only)
   */
  updateTenant: async (tenantId: number, data: TenantRequest): Promise<TenantOutput> => {
    const response = await apiClient.put<TenantOutput>(`/api/tenants/${tenantId}`, data);
    return response.data;
  },

  /**
   * Delete tenant (System Admin only)
   */
  deleteTenant: async (tenantId: number): Promise<boolean> => {
    const response = await apiClient.delete<boolean>(`/api/tenants/${tenantId}`);
    return response.data;
  },

  /**
   * Update tenant contacts (System Admin only)
   */
  updateTenantContacts: async (tenantId: number, data: TenantContactsRequest): Promise<TenantContactsResponse> => {
    const response = await apiClient.patch<TenantContactsResponse>(`/api/tenants/${tenantId}/contacts`, data);
    return response.data;
  },

  /**
   * Toggle tenant active status (System Admin only)
   */
  toggleTenantStatus: async (tenantId: number, active: boolean): Promise<TenantOutput> => {
    const response = await apiClient.patch<TenantOutput>(`/api/tenants/${tenantId}/status`, null, {
      params: { active },
    });
    return response.data;
  },

  // ============================================
  // Tenant Admin - Tenant Profile Management
  // ============================================

  /**
   * Get current user's tenants (Tenant Admin)
   */
  getMyTenants: async (): Promise<TenantOutput[]> => {
    const response = await apiClient.get<TenantOutput[]>('/api/tenants/profile');
    return response.data;
  },

  /**
   * Get tenant profile by ID (Tenant Admin)
   */
  getTenantProfile: async (tenantId: number): Promise<TenantOutput> => {
    const response = await apiClient.get<TenantOutput>(`/api/tenants/profile/${tenantId}`);
    return response.data;
  },

  /**
   * Update tenant profile (Tenant Admin)
   */
  updateTenantProfile: async (tenantId: number, data: TenantRequest): Promise<TenantOutput> => {
    const response = await apiClient.put<TenantOutput>(`/api/tenants/profile/${tenantId}`, data);
    return response.data;
  },

  /**
   * Delete tenant profile (Tenant Admin)
   */
  deleteTenantProfile: async (tenantId: number): Promise<boolean> => {
    const response = await apiClient.delete<boolean>(`/api/tenants/profile/${tenantId}`);
    return response.data;
  },

  /**
   * Update tenant profile contacts (Tenant Admin)
   */
  updateTenantProfileContacts: async (tenantId: number, data: TenantContactsRequest): Promise<TenantContactsResponse> => {
    const response = await apiClient.patch<TenantContactsResponse>(`/api/tenants/profile/${tenantId}/contacts`, data);
    return response.data;
  },

  /**
   * Toggle tenant profile status (Tenant Admin)
   */
  toggleTenantProfileStatus: async (tenantId: number, active: boolean): Promise<TenantOutput> => {
    const response = await apiClient.patch<TenantOutput>(`/api/tenants/profile/${tenantId}/status`, null, {
      params: { active },
    });
    return response.data;
  },
};

export default tenantsApi;
