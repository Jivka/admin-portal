using AP.Common.Models;
using AP.Common.Services.Contracts;
using AP.Identity.Internal.Models;

namespace AP.Identity.Internal.Services.Contracts;

public interface ISystemService
{
    Task<List<RoleOutput>> GetAllRoles();

    Task<List<RoleOutput>?> GetTenantRoles();

    Task<ApiResult> IsCurrentUserSystemAdmin(ICurrentUser currentUser);

    Task<ApiResult> IsCurrentUserTenantAdmin(ICurrentUser currentUser, int tenantId);

    Task<ApiResult> IsCurrentUserAuthorizedUser(ICurrentUser currentUser, int userId);
}