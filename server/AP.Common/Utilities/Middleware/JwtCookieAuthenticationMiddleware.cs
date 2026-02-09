using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using AP.Common.Services.Contracts;
using static AP.Common.Constants.Constants;

namespace AP.Common.Utilities.Middleware;

/// <summary>
/// DEPRECATED: This middleware is obsolete. Use SessionAuthenticationMiddleware instead.
/// SessionAuthenticationMiddleware reads the SessionId cookie and retrieves JWT tokens from the server-side session.
/// </summary>
[Obsolete("Use SessionAuthenticationMiddleware for session-based authentication")]
public class JwtCookieAuthenticationMiddleware(ICurrentToken currentToken) : IMiddleware
{
    private readonly ICurrentToken currentToken = currentToken;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // This middleware is deprecated - SessionAuthenticationMiddleware should be used instead
        // Keeping for backward compatibility but functionality moved to SessionAuthenticationMiddleware
        await next.Invoke(context);
    }
}

public static class JwtCookieAuthenticationMiddlewareExtensions
{
    /// <summary>
    /// DEPRECATED: Use UseSessionAuthentication instead
    /// </summary>
    [Obsolete("Use UseSessionAuthentication for session-based authentication")]
    public static IApplicationBuilder UseJwtCookieAuthentication(
        this IApplicationBuilder app)
        => app
            .UseMiddleware<SessionAuthenticationMiddleware>()
            .UseAuthentication();
}