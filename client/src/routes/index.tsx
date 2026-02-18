import { createBrowserRouter, Navigate } from 'react-router-dom';
import MainLayout from '../components/layout/MainLayout';
import PrivateRoute from './PrivateRoute';
import RoleRoute from './RoleRoute';

// Auth pages (public)
import LoginPage from '../features/auth/pages/LoginPage';
import SignupPage from '../features/auth/pages/SignupPage';
import VerifyEmailPage from '../features/auth/pages/VerifyEmailPage';
import ForgotPasswordPage from '../features/auth/pages/ForgotPasswordPage';
import ResetPasswordPage from '../features/auth/pages/ResetPasswordPage';

// Protected pages
import DashboardPage from '../features/dashboard/pages/DashboardPage';
import ProfilePage from '../features/profile/pages/ProfilePage';
import ChangePasswordPage from '../features/profile/pages/ChangePasswordPage';

// Shared features
import TenantsListPage from '../features/tenants/pages/TenantsListPage';
import UsersListPage from '../features/users/pages/UsersListPage';

// Role constants - these will be validated against /api/roles at runtime
const SYSTEM_ADMIN = 1;
const TENANT_ADMIN = 2;

export const router = createBrowserRouter([
  // Public routes (no authentication required)
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/signup',
    element: <SignupPage />,
  },
  {
    path: '/verify-email',
    element: <VerifyEmailPage />,
  },
  {
    path: '/forgot-password',
    element: <ForgotPasswordPage />,
  },
  {
    path: '/reset-password',
    element: <ResetPasswordPage />,
  },

  // Protected routes (authentication required)
  {
    path: '/',
    element: (
      <PrivateRoute>
        <MainLayout />
      </PrivateRoute>
    ),
    children: [
      // Default redirect to dashboard
      {
        index: true,
        element: <Navigate to="/dashboard" replace />,
      },
      // Dashboard - accessible to all authenticated users
      {
        path: 'dashboard',
        element: <DashboardPage />,
      },
      // Profile - accessible to all authenticated users
      {
        path: 'profile',
        element: <ProfilePage />,
      },
      {
        path: 'change-password',
        element: <ChangePasswordPage />,
      },

      // Shared features (accessible by multiple roles)
      {
        path: 'tenants',
        element: (
          <RoleRoute allowedRoleIds={[SYSTEM_ADMIN, TENANT_ADMIN]}>
            <TenantsListPage />
          </RoleRoute>
        ),
      },
      {
        path: 'users',
        element: (
          <RoleRoute allowedRoleIds={[SYSTEM_ADMIN, TENANT_ADMIN]}>
            <UsersListPage />
          </RoleRoute>
        ),
      },
    ],
  },

  // Catch-all redirect to login
  {
    path: '*',
    element: <Navigate to="/login" replace />,
  },
]);

export default router;
