using AP.Common.Models;
using AP.Common.Services.Contracts;
using AP.Identity.Internal.Models;

namespace AP.Identity.Internal.Services.Contracts;

public interface ITenantUsersService
{
    Task<ApiResult<UsersResponse>> GetTenantUsers(int userId, int? tenantId, int? page, int? size, string? name, string? sort);
    Task<ApiResult<List<UserOutput>>> GetUsersByTenant(int tenantId);
    Task<ApiResult<UserOutput>> GetTenantUser(int tenantId, int userId);
    Task<ApiResult<UserOutput>> CreateTenantUser(int tenantId, CreateUserRequest model, int currentUserId, string? origin);
    Task<ApiResult<UserOutput>> EditTenantUser(int tenantId, EditUserRequest model, int currentUserId, string? origin);
    Task<ApiResult<UserOutput>> ActivateOrDeactivateTenantUser(int tenantId, int userId, bool active, int currentUserId);
    Task<ApiResult<bool>> DeleteTenantUser(int tenantId, int userId, int currentUserId);
}