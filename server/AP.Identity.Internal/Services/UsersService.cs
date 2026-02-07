using System.Security.Cryptography;
using System.Threading.Channels;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AP.Common.Data;
using AP.Common.Data.Extensions;
using AP.Common.Data.Identity.Entities;
using AP.Common.Data.Identity.Enums;
using AP.Common.Data.Options;
using AP.Common.Models;
using AP.Common.Utilities.Helpers;
using AP.Identity.Internal.Models;
using AP.Identity.Internal.Services.Contracts;
using static AP.Identity.Internal.Constants.ApiErrorMessages;
using static AP.Identity.Internal.Constants.NoticeMessages;

namespace AP.Identity.Internal.Services;

public class UsersService(
    DataContext dbContext,
    IRefreshTokenService refreshTokenService,
    IOptions<IdentitySettings> identitySettings,
    Channel<SendEmailRequest> sendEmailChannel,
    IMapper mapper) : IUsersService
{
    private readonly IdentitySettings identitySettings = identitySettings.Value;

    public async Task<ApiResult<UsersResponse>> GetAllUsers()
    {
        var users = dbContext.Users
            .Include(user => user.UserTenants)!.ThenInclude(t => t.Role);

        var response = new UsersResponse()
        {
            Count = await users.CountAsync(),
            Users = await users.Select(user => MapToDomainModel(user)).ToListAsync()
        };

        return ApiResult<UsersResponse>.SuccessWith(response);
    }

    public async Task<ApiResult<UsersResponse>> GetUsers(int? page, int? size, string? name,/* byte? roleId,*/ string? sort)
    {
        var searchFilter = name != null ? name.Replace(" ", "") : string.Empty;

        var count = await dbContext.Users
            .CountAsync(user => (name == null || (user.FirstName + user.LastName).Contains(searchFilter)));
        Pager.Calculate(count, page, size,/* out int? pageNum, out int? pageSize,*/ out int skipRows, out int takeRows);

        var users = dbContext.Users
            .Include(user => user.UserTenants)!.ThenInclude(t => t.Role)
            .Where(user => (name == null || (user.FirstName + user.LastName).Contains(searchFilter)))
            .OrderByDescending(x => x.UserId)
            .Skip(skipRows)
            .Take(takeRows);

        var response = new UsersResponse()
        {
            Count = count,
            Page = page,
            Size = size,
            Name = name,
            Sort = sort,
            Users = await users.Select(user => MapToDomainModel(user)).ToListAsync()
        };

        return ApiResult<UsersResponse>.SuccessWith(response);
    }

    public async Task<ApiResult<List<UserOutput>>> GetUsersByTenant(int tenantId)
    {
        if (!await dbContext.Tenants.AnyAsync(t => t.TenantId == tenantId))
        {
            return ApiResult<List<UserOutput>>.Failure(TenantNotFound, tenantId);
        }

        var users = await dbContext.Users
            .Include(user => user.UserTenants)!.ThenInclude(t => t.Role)
            .Where(user => user.UserTenants!.Any(ut => ut.TenantId == tenantId))
            .Select(user => MapToDomainModel(user))
            .ToListAsync();

        return ApiResult<List<UserOutput>>.SuccessWith(users);
    }

    public async Task<ApiResult<UserOutput>> GetUser(int userId)
    {
        var user = await dbContext.Users
            .Include(user => user.UserTenants)!.ThenInclude(t => t.Role)
            .FirstOrDefaultAsync(user => user.UserId == userId);
        if (user == null)
        {
            return ApiResult<UserOutput>.Failure(UserNotFound, userId);
        }

        return ApiResult<UserOutput>.SuccessWith(MapToDomainModel(user));
    }

    public async Task<ApiResult<UserOutput>> CreateUser(CreateUserRequest model, int currentUserId, string? origin)
    {
        var errors = new List<ApiError>();

        // validate
        if (!await dbContext.Roles.AnyAsync(r => r.RoleId == model.RoleId))
        {
            errors.Add(InvalidRole.WithMessageArgs(model.RoleId));
        }
        if (await dbContext.Users.AnyAsync(x => x.Email == model.Email))
        {
            errors.Add(EmailAlreadyUsed.WithMessageArgs(model.Email));
        }
        if (!await dbContext.Users.AnyAsync(u => u.UserId == currentUserId))
        {
            errors.Add(CreateUserByInvalidUser.WithMessageArgs(currentUserId));
        }
        // don't allow role System Admin if model tenant id is not SystemTenantId
        if (model.RoleId == (byte)Roles.SystemAdmin && model.TenantId != identitySettings.SystemTenantId)
        {
            errors.Add(InvalidRoleForTenant.WithMessageArgs(model.RoleId, model.TenantId));
        }

        // return with errors
        if (errors.Count != 0)
        {
            errors.Insert(0, CreateUserError);
            return ApiResult<UserOutput>.Failure(errors);
        }

        // map model to new User object
        var user = MapToEntityModel(model, currentUserId);

        // save user
        dbContext.Users.Add(user);
        int result = await dbContext.SaveChangesAsync();

        // send email
        await SendVerificationEmail(user, origin);

        return result == 2
                ? ApiResult<UserOutput>.SuccessWith(MapToDomainModel(user), StatusCodes.Status201Created)
                : ApiResult<UserOutput>.Failure(CreateUserError);
    }

    public async Task<ApiResult<UserOutput>> EditUser(EditUserRequest model, int currentUserId, string? origin)
    {
        var errors = new List<ApiError>();

        var user = await dbContext.Users
            .Include(user => user.UserTenants)!.ThenInclude(t => t.Role)
            .FirstOrDefaultAsync(user => user.UserId == model.UserId);
        if (user is null)
        {
            errors.Add(InvalidUser.WithMessageArgs(model.UserId));
        }
        else if (await dbContext.Users.AnyAsync(x => x.UserId != model.UserId && x.Email == model.Email))
        {
            errors.Add(EmailAlreadyUsed.WithMessageArgs(model.Email));
        }
        if (!await dbContext.Users.AnyAsync(u => u.UserId == currentUserId))
        {
            errors.Add(EditUserByInvalidUser.WithMessageArgs(currentUserId));
        }

        // return with errors
        if (user == null || errors.Count != 0)
        {
            errors.Insert(0, EditUserError.WithMessageArgs(model.UserId));

            return ApiResult<UserOutput>.Failure(errors);
        }

        bool sendVerifyEmail = false;
        if (model.Email != user.Email)
        {
            user.VerifiedOn = null;
            user.VerificationToken = GenerateVerificationToken();
            sendVerifyEmail = true;
        }

        // map model to updated User object
        user.UserId = model.UserId;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.Phone = model.Phone;
        user.UpdatedOn = DateTime.UtcNow;
        user.UpdatedBy = currentUserId;

        // update user
        await dbContext.SaveChangesAsync();

        // send email
        if (sendVerifyEmail)
        {
            await SendVerificationEmail(user, origin);
        }

        return ApiResult<UserOutput>.SuccessWith(MapToDomainModel(user));
    }

    public async Task<ApiResult<string>> ChangePassword(ChangePasswordRequest model)
    {
        if (model.NewPassword == model.CurrentPassword)
        {
            return ApiResult<string>.Failure(NewOldPasswordAreEqual);
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);
        if (user is null)
        {
            return ApiResult<string>.Failure(InvalidUser.WithMessageArgs(model.UserId));
        }
        if (user.Email != model.Email)
        {
            return ApiResult<string>.Failure(InvalidEmail.WithMessageArgs(model.Email));
        }
        if ( !user.IsVerified || !BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
        {
            return ApiResult<string>.Failure(InvalidCredentials);
        }

        user!.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        user.PasswordChanged = DateTime.UtcNow;

        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync();

        return ApiResult<string>.SuccessWith(PasswordChangedSuccessfully);
    }

    public async Task<ApiResult<UserOutput>> ActivateOrDeactivateUser(int userId, bool active)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
        if (user == null)
        {
            return ApiResult<UserOutput>.Failure(UserNotFound, userId);
        }

        user.Active = active;
        await dbContext.SaveChangesAsync();

        return ApiResult<UserOutput>.SuccessWith(mapper.Map<UserOutput>(user));
    }

    public async Task<ApiResult<string>> RevokeToken(RevokeTokenRequest model, string? ipAddress, int? currentUserId)
    {
        var currentUser = await dbContext.Users.FindAsync(currentUserId);

        if (model.RefreshToken is null)
        {
            return ApiResult<string>.Failure(NotProvidedRefreshToken);
        }
        if (currentUser is null || !currentUser.OwnsRefreshToken(model.RefreshToken) && !currentUser.IsSystemAdministrator)
        {
            return ApiResult<string>.Failure(UnauthorizedToRevokeRefreshToken);
        }

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.RefreshTokens != null &&
                                 u.RefreshTokens.Any(rt => rt.Token == model.RefreshToken &&
                                                           rt.User != null &&
                                                           rt.User.Email == model.Email));
        if (user is null)
        {
            return ApiResult<string>.Failure(NotFoundRefreshToken.WithMessageArgs(model.Email));
        }

        var userRefreshToken = user.RefreshTokens?.Single(x => x.Token == model.RefreshToken);
        if (userRefreshToken is null || !userRefreshToken.IsActive)
        {
            return ApiResult<string>.Failure(NotActiveRefreshToken.WithMessageArgs(model.Email));
        }

        // revoke refresh token and save
        refreshTokenService.RevokeRefreshToken(userRefreshToken, ipAddress, RevokedWithNoReplacement, null);

        dbContext.Update(user);
        await dbContext.SaveChangesAsync();

        return ApiResult<string>.SuccessWith(RefreshTokenRevoked);
    }

    public async Task<ApiResult<bool>> DeleteUser(int userId)
    {
        var user = await dbContext.Users
            .Include(u => u.UserTenants)!.ThenInclude(ut => ut.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null)
        {
            return ApiResult<bool>.Failure(UserNotFound.WithMessageArgs(userId));
        }

        // cannot delete the only one System Admin
        var otherSystemAdministratorExist = await dbContext.UserTenants.AnyAsync(u => u.UserId != userId && u.RoleId == (byte)Roles.SystemAdmin);
        if (user.IsSystemAdministrator && !otherSystemAdministratorExist)
        {
            return ApiResult<bool>.Failure(CannotDeleteUser.WithMessageArgs(user.Email, userId, Roles.SystemAdmin.GetDisplayName()));
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();

        return ApiResult<bool>.SuccessWith(true, StatusCodes.Status204NoContent);
    }

    #region private methods

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

    private User MapToEntityModel(CreateUserRequest model, int currentUserId)
    {
        return new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Phone = model.Phone,
            Active = true,
            Enabled = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = currentUserId,
            VerificationToken = GenerateVerificationToken(),
            // hash the initial password
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(identitySettings.InitialPassword),
            UserTenants =
            [
                new UserTenant
                {
                    TenantId = model.TenantId,
                    RoleId = model.RoleId,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = currentUserId
                }
            ]
        };
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

    #endregion
}