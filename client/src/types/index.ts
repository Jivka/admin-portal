// Re-export auto-generated API types with cleaner names
import type { components } from './api';

// Schema types - cleaner aliases
export type TenantRole = components['schemas']['AP.Common.Models.TenantRole'];
export type ChangePasswordRequest = components['schemas']['AP.Identity.Internal.Models.ChangePasswordRequest'];
export type CreateUserRequest = components['schemas']['AP.Identity.Internal.Models.CreateUserRequest'];
export type EditUserRequest = components['schemas']['AP.Identity.Internal.Models.EditUserRequest'];
export type ForgotPasswordRequest = components['schemas']['AP.Identity.Internal.Models.ForgotPasswordRequest'];
export type ResetPasswordRequest = components['schemas']['AP.Identity.Internal.Models.ResetPasswordRequest'];
export type RevokeTokenRequest = components['schemas']['AP.Identity.Internal.Models.RevokeTokenRequest'];
export type RoleOutput = components['schemas']['AP.Identity.Internal.Models.RoleOutput'];
export type SigninRequest = components['schemas']['AP.Identity.Internal.Models.SigninRequest'];
export type SigninResponse = components['schemas']['AP.Identity.Internal.Models.SigninResponse'];
export type SignupRequest = components['schemas']['AP.Identity.Internal.Models.SignupRequest'];
export type ContactInput = components['schemas']['AP.Identity.Internal.Models.Tenants.ContactInput'];
export type ContactOutput = components['schemas']['AP.Identity.Internal.Models.Tenants.ContactOutput'];
export type TenantContactsRequest = components['schemas']['AP.Identity.Internal.Models.Tenants.TenantContactsRequest'];
export type TenantContactsResponse = components['schemas']['AP.Identity.Internal.Models.Tenants.TenantContactsResponse'];
export type TenantOutput = components['schemas']['AP.Identity.Internal.Models.Tenants.TenantOutput'];
export type TenantRequest = components['schemas']['AP.Identity.Internal.Models.Tenants.TenantRequest'];
export type TenantRequestOptionalFields = components['schemas']['AP.Identity.Internal.Models.Tenants.TenantRequestOptionalFields'];
export type TenantsResponse = components['schemas']['AP.Identity.Internal.Models.Tenants.TenantsResponse'];
export type UserOutput = components['schemas']['AP.Identity.Internal.Models.UserOutput'];
export type UsersResponse = components['schemas']['AP.Identity.Internal.Models.UsersResponse'];
export type VerifyEmailRequest = components['schemas']['AP.Identity.Internal.Models.VerifyEmailRequest'];

// Re-export paths and components for advanced use cases
export type { paths, components } from './api';
