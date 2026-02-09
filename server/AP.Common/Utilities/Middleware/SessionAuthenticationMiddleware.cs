using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AP.Common.Services.Contracts;
using static AP.Common.Constants.Constants;

namespace AP.Common.Utilities.Middleware;

public class SessionAuthenticationMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Try to get session ID from cookie
        var sessionIdCookie = context.Request.Cookies[SessionCookieName];
        
        if (!string.IsNullOrEmpty(sessionIdCookie) && long.TryParse(sessionIdCookie, out var sessionId))
        {
            // Get services from DI container
            var sessionService = context.RequestServices.GetService<ISessionService>();
            var currentToken = context.RequestServices.GetService<ICurrentToken>();
            var logger = context.RequestServices.GetService<ILogger<SessionAuthenticationMiddleware>>();
            
            if (sessionService != null)
            {
                try
                {
                    // Retrieve session from database
                    var session = await sessionService.GetSessionById(sessionId);
                    
                    if (session != null && !string.IsNullOrEmpty(session.AccessToken))
                    {
                        // Set the token in current token service
                        currentToken?.Set(session.AccessToken);
                        
                        // Check if Authorization header already exists
                        if (context.Request.Headers.ContainsKey(AuthorizationHeaderName))
                        {
                            // Remove existing header first
                            context.Request.Headers.Remove(AuthorizationHeaderName);
                        }
                        
                        // Add the JWT token to the Authorization header for authentication
                        context.Request.Headers.Append(AuthorizationHeaderName, $"{AuthorizationHeaderValuePrefix} {session.AccessToken}");
                        
                        // Store session in HttpContext items for later use
                        context.Items["UserSession"] = session;
                        
                        logger?.LogDebug("Session {SessionId} authenticated for user {UserId}", sessionId, session.UserId);
                    }
                    else
                    {
                        logger?.LogWarning("Session {SessionId} not found or has no access token", sessionId);
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error retrieving session {SessionId}", sessionId);
                }
            }
        }

        await next.Invoke(context);
    }
}

public static class SessionAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseSessionAuthentication(this IApplicationBuilder app)
        => app.UseMiddleware<SessionAuthenticationMiddleware>();
}

