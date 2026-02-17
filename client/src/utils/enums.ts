// Tenant Type Enum
// 1 = Company, 2 = Person, 3 = Organization
export const getTenantTypeLabel = (type?: number): string => {
  switch (type) {
    case 1:
      return 'Company';
    case 2:
      return 'Person';
    case 3:
      return 'Organization';
    default:
      return 'Unknown';
  }
};

// Tenant Ownership Enum
// 1 = Owner, 2 = Installer
export const getOwnershipLabel = (ownership?: number): string => {
  switch (ownership) {
    case 1:
      return 'Owner';
    case 2:
      return 'Installer';
    default:
      return 'Unknown';
  }
};

// Tenant Type options for dropdowns
export const tenantTypeOptions = [
  { value: 1, label: 'Company' },
  { value: 2, label: 'Person' },
  { value: 3, label: 'Organization' },
];

// Ownership options for dropdowns
export const ownershipOptions = [
  { value: 1, label: 'Owner' },
  { value: 2, label: 'Installer' },
];
