using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using AP.Common.Services.Contracts;
using static AP.Common.Constants.Constants;

namespace AP.Common.Utilities.Middleware;

public class JwtHeaderAuthenticationMiddleware(ICurrentToken currentToken) : IMiddleware
{
    private readonly ICurrentToken currentToken = currentToken;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var token = context.Request.Headers[AuthorizationHeaderName].ToString();

        if (!string.IsNullOrWhiteSpace(token))
        {
            var tokenParts = token.Split();
            currentToken.Set(tokenParts[^1]);
        }

        await next.Invoke(context);
    }
}

public static class JwtHeaderAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtHeaderAuthentication(
        this IApplicationBuilder app)
        => app
            .UseMiddleware<JwtHeaderAuthenticationMiddleware>()
            .UseAuthentication()
            .UseAuthorization();
}