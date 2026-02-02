using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AP.Common.Models;
using AP.Common.Services.Contracts;
using AP.Common.Utilities.Attributes;
using AP.Identity.Internal.Models;
using AP.Identity.Internal.Models.Tenants;
using AP.Identity.Internal.Services.Contracts;

namespace AP.Identity.Internal.Controllers;

[ApiController]
[Route("api")]
[Tags("[Tenant Admin] Tenant Users")]
[AuthorizeTenantAdministrator(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TenantUsersController(
    ISystemService systemService,
    ITenantsService tenantsService,
    ITenantUsersService tenantUsersService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("tenants/roles")]
    public async Task<List<RoleOutput>?> GetTenantRoles()
        => await systemService.GetTenantRoles();

    [HttpGet("tenants/own")]
    public async Task<ActionResult<List<TenantOutput>>> GetTenants()
        => await tenantsService.GetTenants(currentUser.UserId);

    [HttpGet("tenants/users/{tenantId}")]
    public async Task<ActionResult<UsersResponse>> GetTenantUsers(int tenantId, int? page, int? size, string? name, string? sort)
        => await WithTenantAdminAccess<UsersResponse>(async ()
            => await tenantUsersService.GetTenantUsers(tenantId, page, size, name, sort), tenantId);

    [HttpGet("tenants/users/{tenantId}/{userId}")]
    public async Task<ActionResult<UserOutput>> GetTenantUser(int tenantId, int userId)
        => await WithTenantAdminAccess<UserOutput>(async ()
            => await tenantUsersService.GetTenantUser(tenantId, userId), tenantId);

    [HttpPost("tenants/users/{tenantId}")]
    public async Task<ActionResult<UserOutput>> CreateTenantUser(int tenantId, CreateUserRequest model)
        => await WithTenantAdminAccess<UserOutput>(async ()
            => await tenantUsersService.CreateTenantUser(tenantId, model, currentUser.UserId, Origin()), tenantId);

    [HttpPut("tenants/users/{tenantId}")]
    public async Task<ActionResult<UserOutput>> EditTenantUser(int tenantId, EditUserRequest model)
    => await WithTenantAdminAccess<UserOutput>(async ()
        => await tenantUsersService.EditTenantUser(tenantId, model, currentUser.UserId, Origin()), tenantId);

    [HttpPatch("tenants/users/{tenantId}/{userId}")]
    public async Task<ActionResult<UserOutput>> ActivateDeactivateTenantUser(int tenantId, int userId, [BindRequired] bool active)
        => await WithTenantAdminAccess<UserOutput>(async()
            => await tenantUsersService.ActivateOrDeactivateTenantUser(tenantId, userId, active, currentUser.UserId), tenantId);

    [HttpDelete("tenants/users/{tenantId}/{userId}")]
    public async Task<ActionResult<bool>> DeleteTenantUser(int tenantId, int userId)
    => await WithTenantAdminAccess<bool>(async ()
        => await tenantUsersService.DeleteTenantUser(tenantId, userId, currentUser.UserId), tenantId);

    private async Task<ApiResult<TResult>> WithTenantAdminAccess<TResult>(Func<Task<ApiResult<TResult>>> action, int tenantId)
    {
        var hasAccess = await systemService.IsCurrentUserTenantAdmin(currentUser, tenantId);
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