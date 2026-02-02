using Microsoft.AspNetCore.Http;
using AP.Common.Models;

namespace AP.Identity.Internal.Constants;

public static class ApiErrorMessages
{
    public static readonly ApiError SystemAccessDenied = new("system_access_denied", "System access is denied with check {0}", StatusCodes.Status401Unauthorized);

    // Tenant errors
    public static readonly ApiError TenantNotFound = new("tenant_not_found", "Tenant not found for id {0}", StatusCodes.Status404NotFound);
    public static readonly ApiError TenantNameAlreadyUsed = new("tenant_name_used", "Tenant name is already used {0}", StatusCodes.Status403Forbidden);
    public static readonly ApiError TenantBICAlreadyUsed = new("tenant_bic_used", "Tenant BIC is already used {0}", StatusCodes.Status403Forbidden);
    public static readonly ApiError CreateTenantError = new("create_tenant_error", "Error creating a tenant", StatusCodes.Status400BadRequest);
    public static readonly ApiError EditTenantError = new("edit_tenant_error", "Error editing tenant with id {0}", StatusCodes.Status400BadRequest);
    public static readonly ApiError InvalidTenantType = new("invalid_tenant_type", "Invalid tenant type {0}", StatusCodes.Status400BadRequest);
    public static readonly ApiError InvalidTenantOwnership = new("invalid_tenant_ownership", "Invalid tenant ownership {0}", StatusCodes.Status400BadRequest);
    public static readonly ApiError CannotDeleteTenant = new("cannot_delete_tenant", "Cannot delete tenant {0} id {1}", StatusCodes.Status403Forbidden);
    public static readonly ApiError TenantAccessDenied = new("tenant_access_denied", "Access to tenant id {0} is denied with check {1}", StatusCodes.Status401Unauthorized);
    public static readonly ApiError InvalidTenant = new("invalid_tenant", "Invalid tenant id {0}", StatusCodes.Status404NotFound);

    // Contact errors
    public static readonly ApiError ContactNotFound = new("contact_not_found", "Contact not found for id {0}", StatusCodes.Status404NotFound);
    public static readonly ApiError ContactNameDuplicated = new("contact_name_duplicated", "Contact person's name is duplicated in the list {0}", StatusCodes.Status403Forbidden);
    public static readonly ApiError ContactEmailDuplicated = new("cotact_name_duplicated", "Contact Email is duplicated in the list {0}", StatusCodes.Status403Forbidden);
    public static readonly ApiError PrimaryContactExists = new("primary_contact_exists", "Primary contact already exists", StatusCodes.Status403Forbidden);
    public static readonly ApiError TenantContactsError = new("tenant_update_error", "Error updating Tenant's contacts");

    public static readonly ApiError UserNotFoundForSub = new("user_not_found", "No user found with sub '{0}'", StatusCodes.Status404NotFound);

    public static readonly ApiError InvalidCredentials = new("invalid_credentials", "Invalid credentials", StatusCodes.Status401Unauthorized);
    public static readonly ApiError NewOldPasswordAreEqual = new("same_new_old_passwords", "New and old passwords are the same", StatusCodes.Status401Unauthorized);
    public static readonly ApiError NotVerifiedEmail = new("not_verified_email", "Not verified email {0}", StatusCodes.Status401Unauthorized);
    public static readonly ApiError InvalidResetToken = new("invalid_reset_token", "Invalid reset token for user email {0}", StatusCodes.Status401Unauthorized);
    public static readonly ApiError ExpiredResetToken = new("expired_reset_token", "Expired reset token for user email {0}", StatusCodes.Status401Unauthorized);

    public static readonly ApiError EmailAlreadyUsed = new("email_used", "Email is already used {0}", StatusCodes.Status403Forbidden);

    public static readonly ApiError CreateUserError = new("create_user_error", "Error creating a user", StatusCodes.Status400BadRequest);
    public static readonly ApiError EditUserError = new("edit_user_error", "Error editing user id {0}", StatusCodes.Status400BadRequest);
    public static readonly ApiError UserNotFound = new("user_not_found", "User not found for id {0}", StatusCodes.Status404NotFound);
    public static readonly ApiError CannotChangeAdminUserRole = new("cannot_change_role", "The only Admin user {0} cannot be changed to {1}", StatusCodes.Status403Forbidden);
    public static readonly ApiError InvalidRoleForTenant = new("invalid_role_for_tenant", "Invalid role {0} for tenant id {1}", StatusCodes.Status403Forbidden);
    public static readonly ApiError CreateUserByInvalidUser = new("cannot_create_user", "Error creating user by invalid user id {0}", StatusCodes.Status403Forbidden);
    public static readonly ApiError EditUserByInvalidUser = new("cannot_edit_user", "Error editing user by invalid user id {0}", StatusCodes.Status403Forbidden);
    public static readonly ApiError CannotDeleteUser = new("cannot_delete_user", "Cannot delete user {0} id {1} referenced by existing records for the following entities:", StatusCodes.Status403Forbidden);

    public static readonly ApiError InvalidRole = new("invalid_role", "Invalid role id {0}", StatusCodes.Status400BadRequest);
    public static readonly ApiError InvalidUser = new("invalid_user", "Invalid user id {0}", StatusCodes.Status400BadRequest);
    public static readonly ApiError InvalidEmail = new("invalid_email", "Invalid email {0}", StatusCodes.Status400BadRequest);

    public static readonly ApiError UserAlreadyVerified = new("already_verified_user", "Already verified user email {0}", StatusCodes.Status403Forbidden);
    public static readonly ApiError VerifyUserError = new("verify_user_error", "Error verifing user email {0}", StatusCodes.Status400BadRequest);
    public static readonly ApiError ResendVerificationCodeError = new("resend_verification_error", "Error resending verification code for email {0}", StatusCodes.Status400BadRequest);
    public static readonly ApiError InvalidVerificationToken = new("invalid_verification_code", "Invalid verification token for email {0}", StatusCodes.Status400BadRequest);

    public static readonly ApiError NotFoundRefreshToken = new("refresh_token_not_found", "Not found refresh token for user email {0}", StatusCodes.Status404NotFound);
    public static readonly ApiError InvalidRefreshToken = new("invalid_refresh_token", "Invalid refresh token for user email {0}", StatusCodes.Status400BadRequest);
    public static readonly ApiError NotActiveRefreshToken = new("not_active_refresh_token", "Not active refresh token for user email {0}", StatusCodes.Status403Forbidden);
    public static readonly ApiError NotProvidedRefreshToken = new("not_provided_refresh_token", "Refresh token is not provided", StatusCodes.Status400BadRequest);
    public static readonly ApiError UnauthorizedToRevokeRefreshToken = new("unauthorized_to_revoke_refresh_token", "Unauthorized to revoke refresh token", StatusCodes.Status403Forbidden);
}