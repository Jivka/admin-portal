/**
 * Format date string to locale format
 */
export const formatDate = (dateString: string | undefined | null): string => {
  if (!dateString) return '-';
  const date = new Date(dateString);
  return date.toLocaleDateString();
};

/**
 * Format date string to locale format with time
 */
export const formatDateTime = (dateString: string | undefined | null): string => {
  if (!dateString) return '-';
  const date = new Date(dateString);
  return date.toLocaleString();
};

/**
 * Get user initials for avatar
 */
export const getInitials = (firstName?: string | null, lastName?: string | null): string => {
  const first = firstName?.charAt(0)?.toUpperCase() || '';
  const last = lastName?.charAt(0)?.toUpperCase() || '';
  return `${first}${last}` || '?';
};

/**
 * Truncate text with ellipsis
 */
export const truncateText = (text: string | undefined | null, maxLength: number): string => {
  if (!text) return '';
  if (text.length <= maxLength) return text;
  return `${text.substring(0, maxLength)}...`;
};
