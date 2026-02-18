using AP.Common.Models;
using AP.Identity.Internal.Models;

namespace AP.Identity.Internal.Services.Contracts;

public interface IUsersService
{
    Task<ApiResult<UsersResponse>> GetAllUsers();
    Task<ApiResult<UsersResponse>> GetUsers(int? tenantId, int? page, int? size, string? name,/* byte? roleId,*/ string? sort);
    Task<ApiResult<List<UserOutput>>> GetUsersByTenant(int tenantId);
    Task<ApiResult<UserOutput>> GetUser(int userId);
    Task<ApiResult<UserOutput>> CreateUser(CreateUserRequest model, int currentUserId, string? origin);
    Task<ApiResult<UserOutput>> EditUser(EditUserRequest model, int currentUserId, string? origin);
    Task<ApiResult<string>> ChangePassword(ChangePasswordRequest model);
    Task<ApiResult<UserOutput>> ActivateOrDeactivateUser(int userId, bool active);
    Task<ApiResult<string>> RevokeToken(RevokeTokenRequest model, string? ipAddress, int? currentUserId);
    Task<ApiResult<bool>> DeleteUser(int userId);
}