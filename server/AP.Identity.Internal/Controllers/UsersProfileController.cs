using AP.Common.Models;
using AP.Common.Services.Contracts;
using AP.Common.Utilities.Attributes;
using AP.Identity.Internal.Models;
using AP.Identity.Internal.Services.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Principal;

namespace AP.Identity.Internal.Controllers;

[ApiController]
[Route("api")]
[Tags("Users' Profile")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UsersProfileController(
    ISystemService systemService,
    IUsersService usersService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("users/profile/{userId}")]
    public async Task<ActionResult<UserOutput>> GetUser(int userId)
        => await WithAuthorizedAccess<UserOutput>(async ()
            => await usersService.GetUser(userId), userId);

    [HttpPut("users/profile")]
    public async Task<ActionResult<UserOutput>> EditUser(EditUserRequest model)
        => await WithAuthorizedAccess<UserOutput>(async ()
            => await usersService.EditUser(model, currentUser.UserId, Origin()), model.UserId);

    [HttpPut("users/profile/change-password")]
    public async Task<ActionResult<string>> ChangePassword(ChangePasswordRequest model)
        => await WithAuthorizedAccess<string>(async ()
            => await usersService.ChangePassword(model), model.UserId);

    [HttpPatch("users/profile/{userId}/status")]
    public async Task<ActionResult<UserOutput>> ActivateDeactivateUser(int userId, [BindRequired] bool active)
    => await WithAuthorizedAccess<UserOutput>(async ()
        => await usersService.ActivateOrDeactivateUser(userId, active), userId);

    [HttpDelete("users/profile/{userId}")]
    public async Task<ActionResult<bool>> DeleteUser(int userId)
    => await WithAuthorizedAccess<bool>(async ()
        => await usersService.DeleteUser(userId), userId);

    private async Task<ApiResult<TResult>> WithAuthorizedAccess<TResult>(Func<Task<ApiResult<TResult>>> action, int userId)
    {
        var hasAccess = await systemService.IsCurrentUserAuthorizedUser(currentUser, userId);
        if (hasAccess.Succeeded)
        {
            return await action();
        }

        return ApiResult<TResult>.Failure(hasAccess.Error ?? default!);
    }

    private string? Origin()
    {
        var origin = HttpContext.Request.Headers.Origin;

        return !string.IsNullOrEmpty(origin)
            ? origin
            : default!;
    }
}