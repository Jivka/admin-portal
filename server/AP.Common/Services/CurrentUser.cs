using System.Security;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using AP.Common.Services.Contracts;
using AP.Common.Utilities.Extensions;

namespace AP.Common.Services;

public class CurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal? user = null;

    public CurrentUser(IHttpContextAccessor httpContextAccessor, ClaimsPrincipal identity)
    {
        RequestCreatedOn = DateTime.UtcNow;
        TraceId = httpContextAccessor.GetTraceId();

        user = identity;
        UserId = Convert.ToInt32(identity.FindFirstValue(ClaimTypes.NameIdentifier));
        Email = identity.FindFirstValue(ClaimTypes.Email) ?? throw new SecurityException("Email claim is missing in user's token");
        IpAddress = GetIpAddress(httpContextAccessor.HttpContext);
    }

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        RequestCreatedOn = DateTime.UtcNow;
        TraceId = httpContextAccessor.GetTraceId();

        var contextUser = httpContextAccessor.HttpContext?.User
            ?? throw new SecurityException("This request does not have an authenticated user.");

        user = contextUser;
        UserId = Convert.ToInt32(contextUser.FindFirstValue(ClaimTypes.NameIdentifier));
        Email = contextUser.FindFirstValue(ClaimTypes.Email) ?? throw new SecurityException("Email claim is missing in user's token");
        TenantRoles = contextUser.FindFirstValue(ClaimTypes.Role);
        IpAddress = GetIpAddress(httpContextAccessor.HttpContext);
    }

    public int UserId { get; }
    public string? UserSub { get; }
    public string? Username { get; }
    public string? Nickname { get; }
    public string? GivenName { get; }
    public string? FamilyName { get; }
    public required string Email { get; init; } // Changed to 'init' to make it settable during object initialization
    public bool? EmailVerified { get; }
    public string? Picture { get; }

    public string? TenantRoles { get; init; }

    public bool IsSystemAdministrator => user != null && user.IsSystemAdministrator();

    public bool IsTestUser => user != null && user.IsTestUser();

    public string? IpAddress { get; }
    public string? TraceId { get; init; }
    public DateTime RequestCreatedOn { get; }

    private static string? GetIpAddress(HttpContext? httpContext)
    {
        if (httpContext?.Request.Headers.ContainsKey("X-Forwarded-For") == true)
        {
            return httpContext?.Request.Headers["X-Forwarded-For"];
        }
        else
        {
            return httpContext?.Connection.RemoteIpAddress?.ToString();
        }
    }
}