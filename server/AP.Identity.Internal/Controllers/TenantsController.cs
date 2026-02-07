using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AP.Common.Models;
using AP.Common.Services.Contracts;
using AP.Common.Utilities.Attributes;
using AP.Identity.Internal.Models.Tenants;
using AP.Identity.Internal.Services.Contracts;

namespace AP.Identity.Internal.Controllers;

[ApiController]
[Route("api")]
[Tags("[System Admin] Tenants")]
[AuthorizeAdministrator(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TenantsController(
    ISystemService systemService,
    ITenantsService tenantsService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("tenants/all")]
    public async Task<ActionResult<List<TenantOutput>>> GetAllTenants()
        => await WithSystemAdminAccess(tenantsService.GetAllTenants);

    [HttpGet("tenants")]
    public async Task<ActionResult<TenantsResponse>> GetTenants(int? page, int? size, string? name, string? sort)
        => await WithSystemAdminAccess<TenantsResponse>(async ()
            => await tenantsService.GetTenants(page, size, name, sort));

    [HttpGet("tenants/{tenantId}")]
    public async Task<ActionResult<TenantOutput>> GetTenant(int tenantId)
        => await WithSystemAdminAccess<TenantOutput>(async ()
            => await tenantsService.GetTenant(tenantId));

    [HttpPost("tenants")]
    public async Task<ActionResult<TenantOutput>> CreateTenant(TenantRequest model)
        => await WithSystemAdminAccess<TenantOutput>(async ()
            => await tenantsService.CreateTenant(model, currentUser.UserId));

    [HttpPut("tenants/{tenantId}")]
    public async Task<ActionResult<TenantOutput>> EditTenant(int tenantId, TenantRequest model)
        => await WithSystemAdminAccess<TenantOutput>(async ()
            => await tenantsService.EditTenant(tenantId, model, currentUser.UserId));

    [HttpPatch("tenants/{tenantId}/contacts")]
    public async Task<ActionResult<TenantContactsResponse>> EditTenantContacts(int tenantId, TenantContactsRequest model)
        => await WithSystemAdminAccess<TenantContactsResponse>(async ()
            => await tenantsService.EditTenantContacts(tenantId, model));

    [HttpPatch("tenants/{tenantId}/status")]
    public async Task<ActionResult<TenantOutput>> ActivateDeactivateUser(int tenantId, [BindRequired] bool active)
        => await WithSystemAdminAccess<TenantOutput>(async ()
            => await tenantsService.ActivateOrDeactivateTenant(tenantId, active, currentUser.UserId));

    [HttpDelete("tenants/{tenantId}")]
    public async Task<ActionResult<bool>> DeleteTenant(int tenantId)
        => await WithSystemAdminAccess<bool>(async ()
            => await tenantsService.DeleteTenant(tenantId, currentUser.UserId));

    private async Task<ApiResult<TResult>> WithSystemAdminAccess<TResult>(Func<Task<ApiResult<TResult>>> action)
    {
        var hasAccess = await systemService.IsCurrentUserSystemAdmin(currentUser);
        if (hasAccess.Succeeded)
        {
            return await action();
        }

        return ApiResult<TResult>.Failure(hasAccess.Error ?? default!);
    }
}