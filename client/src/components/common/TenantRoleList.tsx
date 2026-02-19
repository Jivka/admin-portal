import React from 'react';
import {
  Box,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Alert,
} from '@mui/material';
import { useAppSelector } from '../../store/hooks';
import type { TenantRole } from '../../types';

interface TenantRoleListProps {
  tenantRoles: TenantRole[] | null | undefined;
  title?: string;
  emptyMessage?: string;
}

export const TenantRoleList: React.FC<TenantRoleListProps> = ({
  tenantRoles,
  title = 'Tenant & Role Assignments',
  emptyMessage = 'No tenant-role assignments found',
}) => {
  const { tenants } = useAppSelector((state) => state.tenants);

  // Get tenant name from tenantId
  const getTenantName = (tenantId: number | null | undefined): string => {
    if (!tenantId) return 'N/A';
    const tenant = tenants.find((t) => t.tenantId === tenantId);
    return tenant?.tenantName || `Tenant ${tenantId}`;
  };

  return (
    <Box>
      <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 600 }}>
        {title}
      </Typography>
      {tenantRoles && tenantRoles.length > 0 ? (
        <TableContainer component={Paper} variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>
                  <strong>Tenant</strong>
                </TableCell>
                <TableCell>
                  <strong>Role</strong>
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {tenantRoles.map((tr, index) => (
                <TableRow key={index}>
                  <TableCell>{getTenantName(tr.tenantId)}</TableCell>
                  <TableCell>{tr.roleDisplayName || tr.roleName || 'N/A'}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      ) : (
        <Alert severity="info">{emptyMessage}</Alert>
      )}
    </Box>
  );
};
