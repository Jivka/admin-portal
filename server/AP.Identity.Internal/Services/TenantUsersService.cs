using System.Security.Cryptography;
using System.Threading.Channels;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AP.Common.Data;
using AP.Common.Data.Identity.Entities;
using AP.Common.Data.Identity.Enums;
using AP.Common.Data.Options;
using AP.Common.Models;
using AP.Common.Utilities.Helpers;
using AP.Identity.Internal.Models;
using AP.Identity.Internal.Services.Contracts;
using static AP.Identity.Internal.Constants.ApiErrorMessages;

namespace AP.Identity.Internal.Services;

public class TenantUsersService(
    DataContext dbContext,
    IOptions<IdentitySettings> identitySettings,
    Channel<SendEmailRequest> sendEmailChannel,
    IMapper mapper) : ITenantUsersService
{
    private readonly IdentitySettings identitySettings = identitySettings.Value;

    public async Task<ApiResult<UsersResponse>> GetTenantUsers(int currentUserId, int? tenantId, int? page, int? size, string? name, string? sort)
    {
        var searchFilter = name != null ? name.Replace(" ", "") : string.Empty;

        IQueryable<User> baseQuery;
        if (tenantId.HasValue)
        {
            baseQuery = dbContext.Users.Where(user => user.UserTenants!.Any(ut => ut.TenantId == tenantId.Value));
        }
        else
        {
            var currentUserTenantAdminTenantIds = await dbContext.UserTenants
                .Where(ut => ut.UserId == currentUserId && ut.RoleId == (byte)Roles.TenantAdmin)
                .Select(ut => ut.TenantId)
                .ToListAsync();
            baseQuery = dbContext.Users.Where(user => user.UserTenants!.Any(ut => currentUserTenantAdminTenantIds.Contains(ut.TenantId)));
        }

        var count = await baseQuery
            .CountAsync(user => (name == null || (user.FirstName + user.LastName).Contains(searchFilter)));
        Pager.Calculate(count, page, size, out int skipRows, out int takeRows);

        var users = baseQuery
            .Include(user => user.UserTenants)!.ThenInclude(t => t.Role)
            .Where(user => (name == null || (user.FirstName + user.LastName).Contains(searchFilter)))
            .OrderByDescending(x => x.UserId)
            .Skip(skipRows)
            .Take(takeRows);

        var usersResponse = new UsersResponse()
        {
            Count = count,
            Page = page,
            Size = size,
            Name = name,
            Sort = sort,
            Users = await users.Select(user => MapToDomainModel(user)).ToListAsync()
        };

        return ApiResult<UsersResponse>.SuccessWith(usersResponse);
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

    public async Task<ApiResult<UserOutput>> GetTenantUser(int tenantId, int userId)
    {
        var user = await dbContext.Users
                .Include(user => user.UserTenants)!.ThenInclude(t => t.Role)
                .Where(user => user.UserId == userId && user.UserTenants!.Any(ut => ut.TenantId == tenantId))
                .Select(user => MapToDomainModel(user))
                .FirstOrDefaultAsync();

        if (user is null)
        {
            return ApiResult<UserOutput>.Failure(UserNotFound, userId);
        }

        return ApiResult<UserOutput>.SuccessWith(user);
    }

    public async Task<ApiResult<UserOutput>> CreateTenantUser(int tenantId, CreateUserRequest model, int currentUserId, string? origin)
    {
        // validate
        var errors = new List<ApiError>();
        if (tenantId != model.TenantId)
        {
            errors.Add(InvalidTenant.WithMessageArgs(model.TenantId));
        }
        if (!await dbContext.Roles.AnyAsync(r => r.RoleId == model.RoleId))
        {
            errors.Add(InvalidRole.WithMessageArgs(model.RoleId));
        }
        // don't allow role System Admin for tenant users
        if (model.RoleId == (byte)Roles.SystemAdmin)
        {
            errors.Add(InvalidRoleForTenant.WithMessageArgs(model.RoleId, model.TenantId));
        }
        if (await dbContext.Users.AnyAsync(x => x.Email == model.Email))
        {
            errors.Add(EmailAlreadyUsed.WithMessageArgs(model.Email));
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

    public async Task<ApiResult<UserOutput>> EditTenantUser(int tenantId, EditUserRequest model, int currentUserId, string? origin)
    {
        var errors = new List<ApiError>();

        var user = await dbContext.Users
            .Include(user => user.UserTenants)!.ThenInclude(t => t.Role)
            .FirstOrDefaultAsync(user => user.UserId == model.UserId && user.UserTenants!.Any(ut => ut.TenantId == tenantId));
        if (user is null)
        {
            errors.Add(InvalidUser.WithMessageArgs(model.UserId));
        }
        else if (await dbContext.Users.AnyAsync(x => x.UserId != model.UserId && x.Email == model.Email))
        {
            errors.Add(EmailAlreadyUsed.WithMessageArgs(model.Email));
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

    public async Task<ApiResult<UserOutput>> ActivateOrDeactivateTenantUser(int tenantId, int userId, bool active, int currentUserId)
    {
        var user = await dbContext.Users
            .Include(user => user.UserTenants)!.ThenInclude(t => t.Role)
            .FirstOrDefaultAsync(user => user.UserId == userId && user.UserTenants!.Any(ut => ut.TenantId == tenantId));
        if (user == null)
        {
            return ApiResult<UserOutput>.Failure(UserNotFound, userId);
        }

        user.Active = active;
        await dbContext.SaveChangesAsync();

        return ApiResult<UserOutput>.SuccessWith(mapper.Map<UserOutput>(user));
    }

    public async Task<ApiResult<bool>> DeleteTenantUser(int tenantId, int userId, int currentUserId)
    {
        var user = await dbContext.Users
            .Include(user => user.UserTenants)
            .FirstOrDefaultAsync(u => u.UserId == userId && u.UserTenants!.Any(ut => ut.TenantId == tenantId));
        if (user is null)
        {
            return ApiResult<bool>.Failure(UserNotFound.WithMessageArgs(userId));
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
                    RoleName = ut.RoleName,
                    RoleDisplayName = ut.RoleDisplayName
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