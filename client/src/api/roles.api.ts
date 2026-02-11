import apiClient from './client';
import type { RoleOutput } from '../types';

export const rolesApi = {
  /**
   * Get all roles (System Admin only)
   */
  getAllRoles: async (): Promise<RoleOutput[]> => {
    const response = await apiClient.get<RoleOutput[]>('/api/roles');
    return response.data;
  },

  /**
   * Get tenant roles (Tenant Admin only)
   */
  getTenantRoles: async (): Promise<RoleOutput[]> => {
    const response = await apiClient.get<RoleOutput[]>('/api/tenants/roles');
    return response.data;
  },
};

export default rolesApi;
