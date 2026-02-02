using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AP.Common.Utilities.Extensions;
using static AP.Common.Constants.Constants;
using static AP.Common.Constants.ErrorMessagesConstants;

namespace AP.Common.Utilities.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeTenantAdministratorAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly JsonSerializerOptions options = new() { WriteIndented = true };

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var contextUser = context.HttpContext.User;
        if (contextUser == null || !contextUser.IsTenantAdministrator())
        {
            var apiError = UserNotTenantAdminError.WithMessageArgs(TenantAdministratorRoleDisplayName);

            context.Result = new JsonResult(apiError, options)
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                ContentType = "application/json",
            };
        }
    }
}