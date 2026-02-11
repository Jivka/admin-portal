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
 * Checks if user's roleId is in the allowedRoleIds array
 * 
 * Role IDs (fetched from /api/roles at runtime):
 * - 1: System Admin
 * - 2: Tenant Admin
 * - 3: Power User
 * - 4: End User
 * 
 * Also checks tenantRoles for users with multiple tenant assignments
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

  // Check if user's roleId is in allowed roles
  const userRoleId = user.roleId;
  const hasDirectRole = userRoleId !== undefined && allowedRoleIds.includes(userRoleId);

  if (hasDirectRole) {
    return <>{children}</>;
  }

  // If no direct role match, redirect to default
  return <Navigate to={redirectTo} replace />;
};

export default RoleRoute;
