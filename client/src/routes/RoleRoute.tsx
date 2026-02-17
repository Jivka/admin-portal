import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAppSelector } from '../store/hooks';

interface RoleRouteProps {
  children: ReactNode;
  allowedRoleIds: number[];
  redirectTo?: string;
}

/**
 * RoleRoute - Protects routes based on user role
 * Checks if user has any role in the allowedRoleIds array
 * 
 * Role IDs (fetched from /api/roles at runtime):
 * - 1: System Admin
 * - 2: Tenant Admin
 * - 3: Power User
 * - 4: End User
 * 
 * Checks user's tenantRoles array for role assignments
 */
export const RoleRoute = ({ 
  children, 
  allowedRoleIds,
  redirectTo = '/dashboard'
}: RoleRouteProps) => {
  const { user, isAuthenticated } = useAppSelector((state) => state.auth);

  // Must be authenticated first
  if (!isAuthenticated || !user) {
    return <Navigate to="/login" replace />;
  }

  // Extract all role IDs from user's tenant roles
  const userRoleIds = user.tenantRoles?.map(tr => tr.roleId).filter((id): id is number => id !== null && id !== undefined) || [];

  // Check if user has any of the allowed roles
  const hasRequiredRole = userRoleIds.some(roleId => allowedRoleIds.includes(roleId));

  if (hasRequiredRole) {
    return <>{children}</>;
  }

  // If no role match, redirect to default
  return <Navigate to={redirectTo} replace />;
};

export default RoleRoute;
