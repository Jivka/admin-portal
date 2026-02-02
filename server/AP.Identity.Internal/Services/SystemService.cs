using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AP.Common.Data;
using AP.Common.Data.Identity.Enums;
using AP.Common.Data.Options;
using AP.Common.Models;
using AP.Common.Services.Contracts;
using AP.Identity.Internal.Models;
using AP.Identity.Internal.Services.Contracts;
using static AP.Common.Constants.Constants;
using static AP.Identity.Internal.Constants.ApiErrorMessages;

namespace AP.Identity.Internal.Services;

public class SystemService : ISystemService
{
    private readonly DataContext dbContext;
    private readonly IdentitySettings identitySettings;
    private readonly IMapper mapper;

    public SystemService(DataContext dbContext, IOptions<IdentitySettings> identitySettings, IMapper mapper)
    {
        this.dbContext = dbContext;
        this.identitySettings = identitySettings.Value;
        this.mapper = mapper;

        // Initialize SystemTenantId only if not set (e.g., 0 or default)
        if (this.identitySettings.SystemTenantId == 0)
        {
            var systemTenant = dbContext.UserTenants
                .AsNoTracking()
                .FirstOrDefault(ut => ut.RoleId == (byte)Roles.SystemAdmin);

            this.identitySettings.SystemTenantId = systemTenant?.TenantId ?? 0;
        }
    }

    public async Task<List<RoleOutput>> GetAllRoles()
    {
        return await dbContext.Roles.Select(role => mapper.Map<RoleOutput>(role)).ToListAsync();
    }

    public async Task<List<RoleOutput>?> GetTenantRoles()
    {
        // select the 3 roles TenantAdmin, PowerUser, and EndUser from Roles
        return await dbContext.Roles
            .Where(role => role.RoleId == (byte)Roles.TenantAdmin ||
                           role.RoleId == (byte)Roles.PowerUser ||
                           role.RoleId == (byte)Roles.EndUser)
            .Select(role => new RoleOutput
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                RoleDescription = role.RoleDisplayName
            })
            .ToListAsync();
    }

    public async Task<ApiResult> IsCurrentUserSystemAdmin(ICurrentUser currentUser)
    {
        // check if currentUser.TenantRoles includes System tenantId (=1) with role SystemAdmin
        if (currentUser.TenantRoles is null || currentUser.TenantRoles.Length == 0)
        {
            return ApiResult.Failure(SystemAccessDenied, 1);
        }

        var tenantRoles = JsonSerializer.Deserialize<List<TenantRole>>(currentUser.TenantRoles);
        if (tenantRoles?.Where(tr => tr.TenantId == identitySettings.SystemTenantId).Any(tr => tr.RoleName == SystemAdministratorRoleName) is false)
        {
            return ApiResult.Failure(SystemAccessDenied, 2);
        }

        // check in db
        if (dbContext.UserTenants is null || !await dbContext.UserTenants
           .AnyAsync(ut => ut.UserId == currentUser.UserId && ut.TenantId == identitySettings.SystemTenantId && ut.RoleId == (byte)Roles.SystemAdmin))
        {
            return ApiResult.Failure(SystemAccessDenied, 3);
        }

        return ApiResult.Success;
    }

    public async Task<ApiResult> IsCurrentUserTenantAdmin(ICurrentUser currentUser, int tenantId)
    {
        // check if tenantId exists in db
        if (!await dbContext.Tenants.AnyAsync(t => t.TenantId == tenantId))
        {
            return ApiResult.Failure(TenantNotFound, tenantId);
        }

        // return with error if tenantId is SystemTenantId
        if (tenantId == identitySettings.SystemTenantId)
        {
            return ApiResult.Failure(TenantAccessDenied, tenantId, 0);
        }

        // check if currentUser.TenantRoles include tenantId with role TenantAdmin
        if (currentUser.TenantRoles is null || currentUser.TenantRoles.Length == 0)
        {
            return ApiResult.Failure(TenantAccessDenied, tenantId, 1);
        }

        var tenantRoles = JsonSerializer.Deserialize<List<TenantRole>>(currentUser.TenantRoles);
        if (tenantRoles?.Where(tr => tr.TenantId == tenantId).Any(tr => tr.RoleName == TenantAdministratorRoleName) is false)
        {
            return ApiResult.Failure(TenantAccessDenied, tenantId, 2);
        }

        // check in db
        var dbCheck = await dbContext.UserTenants
            .Where(ut => ut.UserId == currentUser.UserId && ut.TenantId == tenantId && ut.RoleId == (byte)Roles.TenantAdmin)
            .FirstOrDefaultAsync();

        if (dbCheck is null)
        {
            return ApiResult.Failure(TenantAccessDenied, tenantId, 3);
        }

        return ApiResult.Success;
    }
}
