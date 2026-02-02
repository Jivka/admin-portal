using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using AP.Common.Services.Contracts;
using static AP.Common.Constants.Constants;

namespace AP.Common.Utilities.Middleware;

public class JwtCookieAuthenticationMiddleware(ICurrentToken currentToken) : IMiddleware
{
    private readonly ICurrentToken currentToken = currentToken;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var token = context.Request.Cookies[AuthenticationCookieName];

        if (token != null)
        {
            currentToken.Set(token);

            context.Request.Headers.Append(AuthorizationHeaderName, $"{AuthorizationHeaderValuePrefix} {token}");
        }

        await next.Invoke(context);
    }
}

public static class JwtCookieAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtCookieAuthentication(
        this IApplicationBuilder app)
        => app
            .UseMiddleware<JwtCookieAuthenticationMiddleware>()
            .UseAuthentication();
}