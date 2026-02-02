using AP.Common.Data.Identity.Entities;
using AP.Common.Models;
using AP.Identity.Internal.Models;

namespace AP.Identity.Internal.Services.Contracts;

public interface IIdentityService
{
    Task<ApiResult<UserOutput>> SignUpUser(SignupRequest model, string? origin);
    Task<ApiResult<string>> ResendVerificationCode(string email, string? origin);
    Task<ApiResult<UserOutput>> VerifyEmail(VerifyEmailRequest model);
    Task<ApiResult<SigninResponse>> SignIn(SigninRequest model, string? ipAddress);
    Task<ApiResult<SigninResponse>> RefreshToken(RefreshTokenRequest model, string? ipAddress);
    Task<ApiResult<string>> ChangePassword(ChangePasswordRequest model);
    Task<ApiResult<string>> ForgotPassword(ForgotPasswordRequest model, string? origin);
    Task<ApiResult<string>> ResetPassword(ResetPasswordRequest model);
    Task SendPasswordResetEmail(User user, string? origin);

    string GenerateResetToken();
}