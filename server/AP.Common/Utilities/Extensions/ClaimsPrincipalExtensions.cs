using MassTransit.Caching.Internals;
using AP.Common.Models;
using System.Security.Claims;
using System.Text.Json;
using static AP.Common.Constants.Constants;

namespace AP.Common.Utilities.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool IsAdministrator(this ClaimsPrincipal user)
        => user.IsInRole(SystemAdministratorRoleName);

    public static bool IsTestUserX(this ClaimsPrincipal user)
        => user.IsInRole(TestRoleName);

    public static bool IsSystemAdministrator(this ClaimsPrincipal user)
    {
        // check if one and only one claim in the user claims is of type Role
        // and one and only one of the value is of AdministratorRoleName
        var roleClaim = user.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role && c.Value != null);

        if (roleClaim is null)
            return false;

        var tenatRoles = JsonSerializer.Deserialize<List<TenantRole>>(roleClaim.Value);
        var result = tenatRoles?.SingleOrDefault(c => c.RoleName == SystemAdministratorRoleName);

        return result is not null;
    }

    public static bool IsTenantAdministrator(this ClaimsPrincipal user)
    {
        // check if one and only one claim in the user claims is of type Role
        // and there's at least one user's tenant with TenantAdministratorRoleName
        var roleClaim = user.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role && c.Value != null);

        if (roleClaim is null)
            return false;

        var tenatRoles = JsonSerializer.Deserialize<List<TenantRole>>(roleClaim.Value);
        var result = tenatRoles?.FirstOrDefault(c => c.RoleName == TenantAdministratorRoleName);

        return result is not null;
    }

    public static bool IsTestUser(this ClaimsPrincipal user)
    {
        return user.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == TestRoleName);
    }
}