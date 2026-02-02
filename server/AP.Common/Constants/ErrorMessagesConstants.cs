using Microsoft.AspNetCore.Http;
using AP.Common.Models;

namespace AP.Common.Constants;

public static class ErrorMessagesConstants
{
    public static readonly ApiError UnexpectedError = new ("unexpected_error", "The system has encountered an unexpected error", StatusCodes.Status500InternalServerError);
    public static readonly ApiError UserNotAdminError = new("auth_error", "User is not {0}", StatusCodes.Status401Unauthorized);
    public static readonly ApiError UserNotTenantAdminError = new("tenant_auth_error", "User is not {0}", StatusCodes.Status401Unauthorized);

    public static readonly ApiError SigninValidationError = new ("signin_error", "Validation error(s) on sign-in");
    public static readonly ApiError SignupValidationError = new("signup_error", "Validation error(s) on sign-up");
    public static readonly ApiError VerifyEmailValidationError = new("verify_email_error", "Validation error(s) on email verification");
    public static readonly ApiError ChangePasswordValidationError = new("change_password_error", "Validation error(s) on change password");
    public static readonly ApiError CreateUserValidationError = new("create_user_error", "Validation error(s) on create user");
    public static readonly ApiError EditUserValidationError = new("edit_user_error", "Validation error(s) on edit user");
    public static readonly ApiError ForgotPasswordValidationError = new("forgot_password_error", "Validation error(s) on forgot password");
    public static readonly ApiError RefreshTokenValidationError = new("refres_token_error", "Validation error(s) on refresh token");
    public static readonly ApiError ResetPasswordValidationError = new("reset_password_error", "Validation error(s) on reset password");
    public static readonly ApiError RevokeTokenValidationError = new("revoke_token_error", "Validation error(s) on revoke token");

    public static readonly ApiError TenantValidationError = new("tenant_validation_error", "Validation error(s) on tenant request");
    public static readonly ApiError ContactValidationError = new("contact_validation_error", "Validation error(s) on contact request");
}