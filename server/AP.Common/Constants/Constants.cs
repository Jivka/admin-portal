using AP.Common.Data.Extensions;
using AP.Common.Data.Identity.Enums;

namespace AP.Common.Constants;

public static class Constants
{
    public const string AuthenticationCookieName = "Authentication";
    public const string AuthorizationHeaderName = "Authorization";
    public const string AuthorizationHeaderValuePrefix = "Bearer";
    public const string BearerScheme = "Bearer";

    public const string EmailMissingErrorMessage = $"Email address is missing";
    public static readonly string EmailDomainErrorMessage = "Email domain is limited to {0} only";

    public static readonly string PasswordStrengthErrorMessage = "Password is not strong enough. " +
        "It requires a minimun of {0} and maximum of {1} characters with at least one lower case, one upper case, one digit, and one special character from {2}.";

    public static readonly string SystemAdministratorRoleDisplayName = Roles.SystemAdmin.GetDisplayName();
    public static readonly string SystemAdministratorRoleName = Roles.SystemAdmin.ToString();
    public static readonly string TenantAdministratorRoleDisplayName = Roles.TenantAdmin.GetDisplayName();
    public static readonly string TenantAdministratorRoleName = Roles.TenantAdmin.ToString();
    public static readonly string TestRoleName = Roles.TestUser.ToString();
}