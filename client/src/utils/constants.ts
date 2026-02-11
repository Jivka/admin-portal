// Application constants

/**
 * API Base URL from environment
 */
export const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:5001';

/**
 * Pagination defaults
 */
export const DEFAULT_PAGE_SIZE = 10;
export const PAGE_SIZE_OPTIONS = [5, 10, 25, 50];

/**
 * Local storage keys (if needed for non-sensitive data)
 * Note: Authentication uses HttpOnly cookies, not local storage
 */
export const STORAGE_KEYS = {
  THEME_MODE: 'admin-portal-theme',
  SIDEBAR_COLLAPSED: 'admin-portal-sidebar-collapsed',
  REMEMBERED_EMAIL: 'admin-portal-remembered-email',
} as const;
