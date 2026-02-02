using AP.Common.Models;
using AP.Identity.Internal.Models.Tenants;

namespace AP.Identity.Internal.Services.Contracts;

public interface ITenantsService
{
    Task<ApiResult<List<TenantOutput>>> GetAllTenants();
    Task<ApiResult<List<TenantOutput>>> GetTenants(int currentUserId);
    Task<ApiResult<TenantsResponse>> GetTenants(int? page, int? size, string? name, string? sort);
    Task<ApiResult<TenantOutput>> GetTenant(int tenantId);
    Task<ApiResult<TenantOutput>> CreateTenant(TenantRequest model, int currentUserId);
    Task<ApiResult<TenantOutput>> EditTenant(int tenantId, TenantRequest model, int currentUserId);
    Task<ApiResult<TenantContactsResponse>> EditTenantContacts(TenantContactsRequest model);
    Task<ApiResult<TenantOutput>> ActivateOrDeactivateTenant(int tenantId, bool active, int currentUserId);
    Task<ApiResult<bool>> DeleteTenant(int tenantId, int currentUserId);
}