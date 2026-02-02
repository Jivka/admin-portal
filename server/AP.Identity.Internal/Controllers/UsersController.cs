using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using AP.Common.Models;
using AP.Common.Services.Contracts;
using AP.Common.Utilities.Attributes;
using AP.Identity.Internal.Models;
using AP.Identity.Internal.Services.Contracts;

namespace AP.Identity.Internal.Controllers;

[ApiController]
[Route("api")]
[Tags("[System Admin] Users")]
[AuthorizeAdministrator(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UsersController(
    ISystemService systemService,
    IUsersService usersService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("roles")]
    public async Task<List<RoleOutput>> GetAllRoles()
        => await systemService.GetAllRoles();

    [HttpGet("users")]
    public async Task<ActionResult<UsersResponse>> GetUsers(int? page, int? size, string? name,/* byte? role,*/ string? sort)
        => await WithSystemAdminAccess<UsersResponse>(async ()
            => await usersService.GetUsers(page, size, name,/* role,*/ sort));

    [HttpGet("users/tenantId={tenantId}")]
    public async Task<ActionResult<List<UserOutput>>> GetUsersByTenant(int tenantId)
        => await WithSystemAdminAccess<List<UserOutput>>(async ()
            => await usersService.GetUsersByTenant(tenantId));

    [HttpGet("users/{userId}")]
    public async Task<ActionResult<UserOutput>> GetUser(int userId)
    => await WithSystemAdminAccess<UserOutput>(async ()
        => await usersService.GetUser(userId));

    [HttpPost("users")]
    public async Task<ActionResult<UserOutput>> CreateUser(CreateUserRequest model)
        => await WithSystemAdminAccess<UserOutput>(async ()
            => await usersService.CreateUser(model, currentUser.UserId, Origin()));

    [HttpPut("users")]
    public async Task<ActionResult<UserOutput>> EditUser(EditUserRequest model)
        => await WithSystemAdminAccess<UserOutput>(async ()
            => await usersService.EditUser(model, currentUser.UserId, Origin()));

    [HttpPatch("users/{userId}")]
    public async Task<ActionResult<UserOutput>> ActivateDeactivateUser(int userId, [BindRequired] bool active)
    => await WithSystemAdminAccess<UserOutput>(async ()
        => await usersService.ActivateOrDeactivateUser(userId, active));

    [HttpPost("revoke-refresh-token")]
    public async Task<ActionResult<string>> RevokeToken(RevokeTokenRequest model)
        => await WithSystemAdminAccess<string>(async ()
            => await usersService.RevokeToken(model, IpAddress(), currentUser.UserId));

    [HttpDelete("users/{userId}")]
    public async Task<ActionResult<bool>> DeleteUser(int userId)
    => await WithSystemAdminAccess<bool>(async ()
        => await usersService.DeleteUser(userId));

    private async Task<ApiResult<TResult>> WithSystemAdminAccess<TResult>(Func<Task<ApiResult<TResult>>> action)
    {
        var hasAccess = await systemService.IsCurrentUserSystemAdmin(currentUser);
        if (hasAccess.Succeeded)
        {
            return await action();
        }

        return ApiResult<TResult>.Failure(hasAccess.Error ?? default!);
    }

    private string? IpAddress()
    {
        var forwardedHeader = HttpContext.Request.Headers[ForwardedHeadersDefaults.XForwardedForHeaderName];
        if (!StringValues.IsNullOrEmpty(forwardedHeader))
        {
            return forwardedHeader.FirstOrDefault();
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private string? Origin()
    {
        var origin = HttpContext.Request.Headers.Origin;

        return !string.IsNullOrEmpty(origin)
            ? origin
            : default!;
    }
}