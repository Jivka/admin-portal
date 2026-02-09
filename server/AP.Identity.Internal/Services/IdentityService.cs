using System.Security.Cryptography;
using System.Threading.Channels;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AP.Common.Data;
using AP.Common.Data.Identity.Entities;
using AP.Common.Data.Options;
using AP.Common.Models;
using AP.Common.Services.Contracts;
using AP.Identity.Internal.Models;
using AP.Identity.Internal.Services.Contracts;
using static AP.Identity.Internal.Constants.ApiErrorMessages;
using static AP.Identity.Internal.Constants.EventMessages;
using static AP.Identity.Internal.Constants.NoticeMessages;
using static AP.Common.Constants.Constants;

namespace AP.Identity.Internal.Services;

public class IdentityService(
    DataContext dbContext,
    IJwtService jwtTokenGenerator,
    IRefreshTokenService refreshTokenService,
    ISessionService sessionService,
    IOptions<IdentitySettings> identitySettings,
    Channel<UserEventRequest> userEventChannel,
    Channel<SendEmailRequest> sendEmailChannel,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor) : IIdentityService
{
    private readonly IdentitySettings identitySettings = identitySettings.Value;

    public async Task<ApiResult<UserOutput>> SignUpUser(SignupRequest model, string? origin)
    {
        var existingUser = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == model.Email);
        if (existingUser is not null)
        {
            return ApiResult<UserOutput>.Failure(EmailAlreadyUsed, model.Email);
        }

        // map model to new User object
        var user = MapToEntityModel(model);

        // save user
        dbContext.Users.Add(user);
        int result = await dbContext.SaveChangesAsync();

        // send email
        await SendVerificationEmail(user, origin);

        return result == 1
                ? ApiResult<UserOutput>.SuccessWith(MapToDomainModel(user), StatusCodes.Status201Created)
                : ApiResult<UserOutput>.Failure(CreateUserError);
    }

    public async Task<ApiResult<string>> ResendVerificationCode(string email, string? origin)
    {
        // map model to new User object
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user is null)
        {
            return ApiResult<string>.Failure(InvalidEmail, email);
        }

        user.VerificationToken = GenerateVerificationToken();

        // save user
        int result = await dbContext.SaveChangesAsync();

        // send email
        await SendVerificationEmail(user, origin);

        return result == 1
                ? ApiResult<string>.SuccessWith(email)
                : ApiResult<string>.Failure(ResendVerificationCodeError, email);
    }

    public async Task<ApiResult<UserOutput>> VerifyEmail(VerifyEmailRequest model)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == model.Email);
        if (user is null)
        {
            return ApiResult<UserOutput>.Failure(InvalidEmail, model.Email);
        }

        if (user.IsVerified)
        {
            return ApiResult<UserOutput>.Failure(UserAlreadyVerified, model.Email);
        }

        if (user.VerificationToken != model.VerificationToken)
        {
            return ApiResult<UserOutput>.Failure(InvalidVerificationToken, model.Email);
        }

        user.EmailVerified = true;
        user.VerifiedOn = DateTime.UtcNow;
        user.VerificationToken = null;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

        // save user as verified with password
        dbContext.Users.Update(user);
        int result = await dbContext.SaveChangesAsync();

        return result == 1
                    ? ApiResult<UserOutput>.SuccessWith(MapToDomainModel(user))
                    : ApiResult<UserOutput>.Failure(VerifyUserError, model.Email);
    }

    public async Task<ApiResult<SigninResponse>> SignIn(SigninRequest model, string? ipAddress)
    {
        var user = await dbContext.Users
            .Include(user => user.UserTenants)!.ThenInclude(t => t.Role)
            .FirstOrDefaultAsync(user => user.Email == model.Email);

        // validate
        if (user is null)
        {
            await dbContext.SaveChangesAsync();
            await SendUserEvent(null, model.Email, LoginInvalidEmail, false, ipAddress);
            return ApiResult<SigninResponse>.Failure(InvalidCredentials);
        }
        else if (!user.IsVerified)
        {
            await dbContext.SaveChangesAsync();
            await SendUserEvent(user.UserId, user.Email, LoginUnverifiedEmail, false, ipAddress);
            return ApiResult<SigninResponse>.Failure(NotVerifiedEmail, model.Email);
        }
        else if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            await dbContext.SaveChangesAsync();
            await SendUserEvent(user.UserId, user.Email, LoginInvalidPassword, false, ipAddress);
            return ApiResult<SigninResponse>.Failure(InvalidCredentials);
        }

        var tenantRoles = user.UserTenants!
            .Select(ut => new TenantRole()
            {
                TenantId = ut.TenantId ?? default,
                RoleId = ut.RoleId,
                RoleName = ut.RoleName
            })
            .ToList();

        // authentication successful so generate jwt and refresh tokens
        var jwtToken = jwtTokenGenerator.GenerateJwtToken(user, tenantRoles);
        var refreshToken = refreshTokenService.GenerateRefreshToken(ipAddress);

        user.RefreshTokens?.Add(refreshToken);

        // remove old refresh tokens from the user
        refreshTokenService.RemoveOldRefreshTokens(user);

        // save login to db
        dbContext.Update(user);
        await dbContext.SaveChangesAsync();

        // send login event
        await SendUserEvent(user.UserId, user.Email, LoggedIn, true, ipAddress);

        var response = mapper.Map<SigninResponse>(MapToDomainModel(user));

        // Create server-side session with tokens
        var session = await sessionService.CreateSession(user.UserId, jwtToken, refreshToken.Token ?? string.Empty, ipAddress);

        // Set session ID cookie (not the tokens themselves)
        SetSessionCookie(session.SessionId);

        return ApiResult<SigninResponse>.SuccessWith(response);
    }

    public async Task<ApiResult<SigninResponse>> RefreshToken(RefreshTokenRequest model, string? ipAddress)
    {
        // validations
        var user = await dbContext.Users
            .Include(user => user.UserTenants)!.ThenInclude(t => t.Role)
            .FirstOrDefaultAsync(u => u.Email == model.Email &&
                                 u.RefreshTokens != null &&
                                 u.RefreshTokens.Any(t => t.Token == model.RefreshToken && t.CreatedByIp == ipAddress));
        if (user is null)
        {
            return ApiResult<SigninResponse>.Failure(NotFoundRefreshToken.WithMessageArgs(model.Email));
        }

        var refreshToken = user.RefreshTokens?
            .Where(rt => rt != null && rt.CreatedByIp == ipAddress &&
                         rt.User != null && rt.User.Email == model.Email)
            .Single(x => x.Token == model.RefreshToken);
        if (refreshToken is null)
        {
            return ApiResult<SigninResponse>.Failure(InvalidRefreshToken.WithMessageArgs(model.Email));
        }

        if (refreshToken.IsRevoked)
        {
            // revoke all descendant tokens in case this token has been compromised
            refreshTokenService.RevokeDescendantRefreshTokens(refreshToken, user, ipAddress, string.Format(AttemptedReuse, model.RefreshToken));
            dbContext.Update(user);
            await dbContext.SaveChangesAsync();
        }

        // revoked or expired
        if (!refreshToken.IsActive)
        {
            return ApiResult<SigninResponse>.Failure(NotActiveRefreshToken.WithMessageArgs(model.Email));
        }

        // replace old refresh token with a new one (rotate token)
        var newRefreshToken = refreshTokenService.RotateRefreshToken(refreshToken, ipAddress);
        user.RefreshTokens?.Add(newRefreshToken);

        // remove all old refresh tokens from user
        refreshTokenService.RemoveOldRefreshTokens(user);

        // save changes to db
        dbContext.Update(user);
        await dbContext.SaveChangesAsync();

        var tenantRoles = user.UserTenants!
            .Select(ut => new TenantRole()
            {
                TenantId = ut.TenantId ?? default,
                RoleId = ut.RoleId,
                RoleName = ut.RoleName
            })
            .ToList();

        // generate new jwt
        var jwtToken = jwtTokenGenerator.GenerateJwtToken(user, tenantRoles);

        var response = mapper.Map<SigninResponse>(MapToDomainModel(user));

        // Update server-side session with new tokens
        var session = await sessionService.GetSessionByUserAndIp(user.UserId, ipAddress);
        if (session != null)
        {
            await sessionService.UpdateSession(session, jwtToken, newRefreshToken.Token ?? string.Empty);
        }
        else
        {
            // Create new session if it doesn't exist
            session = await sessionService.CreateSession(user.UserId, jwtToken, newRefreshToken.Token ?? string.Empty, ipAddress);
            SetSessionCookie(session.SessionId);
        }

        return ApiResult<SigninResponse>.SuccessWith(response);
    }

    public async Task<ApiResult<string>> ForgotPassword(ForgotPasswordRequest model, string? origin)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => model.Email != null && x.Email == model.Email);
        if (user is null)
        {
            return ApiResult<string>.Failure(InvalidEmail, model.Email);
        }

        // create reset token that expires after 1 day
        user.ResetToken = GenerateResetToken();
        user.ResetTokenExpires = DateTime.UtcNow.AddDays(identitySettings.ResetTokenTTE);

        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync();

        // send email
        await SendPasswordResetEmail(user, origin);

        return ApiResult<string>.SuccessWith(EmailWithPasswordResetInstructions);
    }

    public async Task<ApiResult<string>> ResetPassword(ResetPasswordRequest model)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => model.Email != null && x.Email == model.Email);
        if (user is null)
        {
            return ApiResult<string>.Failure(InvalidEmail, model.Email);
        }

        if (user.ResetToken is null || user.ResetToken != model.ResetToken)
        {
            return ApiResult<string>.Failure(InvalidResetToken, model.Email);
        }

        if (user.ResetTokenExpires < DateTime.UtcNow)
        {
            return ApiResult<string>.Failure(ExpiredResetToken, model.Email);
        }

        // update password and remove reset token
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
        user.PasswordReset = DateTime.UtcNow;
        user.ResetToken = null;
        user.ResetTokenExpires = null;

        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync();

        return ApiResult<string>.SuccessWith(PasswordResetSuccessfully);
    }

    public async Task SendPasswordResetEmail(User user, string? origin)
    {
        if (user.Email is null)
        {
            return;
        }

        string message;
        if (!string.IsNullOrEmpty(origin))
        {
            var resetUrl = $"{origin}/resetpassword?email={user.Email.Replace("+", "%2B")}&resetCode={user.ResetToken}";
            message = $@"<p>Please click the below link to reset your password, the link will be valid for 1 day:</p>
                            <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
        }
        else
        {
            message = $@"<p>Please use the below URL path to reset your password, the path will be valid for 1 day:</p>
                            <p><code>/resetpassword?email={user.Email.Replace("+", "%2B")}&resetCode={user.ResetToken}</code></p>";
        }

        var emailRequest = new SendEmailRequest
        {
            To = user.Email,
            Subject = "WBP - Reset Password",
            Html = $@"<h4>Reset Password</h4>
                        {message}"
        };

        // write event to the channel
        await sendEmailChannel.Writer.WriteAsync(emailRequest);
    }

    public string GenerateResetToken()
    {
        // token is a cryptographically strong random sequence of values
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        // ensure token is unique by checking against db
        var tokenIsUnique = !dbContext.Users.Any(x => x.ResetToken == token);
        if (!tokenIsUnique)
            return GenerateResetToken();

        return token;
    }

    #region private methods

    private void SetSessionCookie(long sessionId)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(identitySettings.RefreshTokenTTE)
        };

        httpContextAccessor.HttpContext?.Response.Cookies.Append(SessionCookieName, sessionId.ToString(), cookieOptions);
    }

    private string GenerateVerificationToken()
    {
        // token is a cryptographically strong random sequence of values
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        // ensure token is unique by checking against db
        var tokenIsUnique = !dbContext.Users.Any(x => x.VerificationToken == token);
        if (!tokenIsUnique)
            return GenerateVerificationToken();

        return token;
    }

    private async Task SendVerificationEmail(User user, string? origin)
    {
        if (user.Email == null)
        {
            return;
        }

        string message;
        if (!string.IsNullOrEmpty(origin))
        {
            // origin exists if request sent from browser single page app (e.g. React)
            // so send link to verify via single page app
            var verifyUrl = $"{origin}/verifyemail?email={user.Email.Replace("+", "%2B")}&code={user.VerificationToken}";
            message = $@"<p>Please click the below link to verify your email address:</p>
                            <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
        }
        else
        {
            // origin missing if request sent directly to api (e.g. from Postman)
            // so send instructions to verify directly with api
            message = $@"<p>Please use the below token to verify your email address with the <code>/verifyemail?email={user.Email.Replace("+", "%2B")}</code> api route:</p>
                            <p><code>{user.VerificationToken}</code></p>";
        }

        var emailRequest = new SendEmailRequest
        {
            To = user.Email,
            Subject = "WBP - Verify Email",
            Html = $@"<h4>Verify Email</h4>
                        {message}"
        };

        // write event to the channel
        await sendEmailChannel.Writer.WriteAsync(emailRequest);
    }

    private async Task SendUserEvent(int? userId, string email, string action, bool success, string? ipAdress)
    {
        var userEventRequest = new UserEventRequest
        {
            UserId = userId,
            UserEmail = email,
            Action = action,
            Success = success,
            ActionOn = DateTime.UtcNow,
            ActionFromIp = ipAdress
        };

        // write event to the channel
        await userEventChannel.Writer.WriteAsync(userEventRequest);
    }

    private User MapToEntityModel(SignupRequest model)
    {
        return new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Active = true,
            Enabled = true,
            CreatedOn = DateTime.UtcNow,
            VerificationToken = GenerateVerificationToken(),
            // hash the initial password
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(identitySettings.InitialPassword)
        };
    }

    private static UserOutput MapToDomainModel(User user)
    {
        return new UserOutput
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            TenantRoles = [.. user.UserTenants!
                .Select(ut => new TenantRole()
                {
                    TenantId = ut.TenantId ?? default,
                    RoleId = ut.RoleId,
                    RoleName = ut.RoleName
                })],
            Active = user.Active,
            IsVerified = user.IsVerified,
            CreatedOn = user.CreatedOn,
        };
    }

    #endregion
}