using AP.Common.Models;
using AP.Common.Services.Contracts;
using AP.Common.Utilities.Attributes;
using AP.Identity.Internal.Models;
using AP.Identity.Internal.Models.Tenants;
using AP.Identity.Internal.Services.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AP.Identity.Internal.Controllers;

[ApiController]
[Route("api")]
[Tags("[Tenant Admin] Tenants' Profile")]
[AuthorizeTenantAdministrator(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TenantsProfileController(
    ISystemService systemService,
    ITenantsService tenantsService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("tenants/profile")]
    public async Task<ActionResult<List<TenantOutput>>> GetTenants()
        => await tenantsService.GetTenants(currentUser.UserId);

    [HttpGet("tenants/profile/{tenantId}")]
    public async Task<ActionResult<TenantOutput>> GetTenantById(int tenantId)
        => await WithTenantAdminAccess<TenantOutput>(async ()
            => await tenantsService.GetTenant(tenantId), tenantId);

    [HttpPut("tenants/profile/{tenantId}")]
    public async Task<ActionResult<TenantOutput>> EditTenant(int tenantId, TenantRequest model)
        => await WithTenantAdminAccess<TenantOutput>(async ()
            => await tenantsService.EditTenant(tenantId, model, currentUser.UserId), tenantId);

    [HttpPatch("tenants/profile/{tenantId}/contacts")]
    public async Task<ActionResult<TenantContactsResponse>> EditTenantContacts(int tenantId, TenantContactsRequest model)
        => await WithTenantAdminAccess<TenantContactsResponse>(async ()
            => await tenantsService.EditTenantContacts(tenantId, model), tenantId);

    [HttpPatch("tenants/profile/{tenantId}/status")]
    public async Task<ActionResult<TenantOutput>> ActivateDeactivateUser(int tenantId, [BindRequired] bool active)
        => await WithTenantAdminAccess<TenantOutput>(async ()
            => await tenantsService.ActivateOrDeactivateTenant(tenantId, active, currentUser.UserId), tenantId);

    [HttpDelete("tenants/profile/{tenantId}")]
    public async Task<ActionResult<bool>> DeleteTenant(int tenantId)
        => await WithTenantAdminAccess<bool>(async ()
            => await tenantsService.DeleteTenant(tenantId, currentUser.UserId), tenantId);

    private async Task<ApiResult<TResult>> WithTenantAdminAccess<TResult>(Func<Task<ApiResult<TResult>>> action, int tenantId)
    {
        var hasAccess = await systemService.IsCurrentUserTenantAdmin(currentUser, tenantId);
        if (hasAccess.Succeeded)
        {
            return await action();
        }

        return ApiResult<TResult>.Failure(hasAccess.Error ?? default!);
    }
}