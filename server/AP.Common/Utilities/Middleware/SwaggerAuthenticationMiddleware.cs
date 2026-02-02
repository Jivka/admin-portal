using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace AP.Common.Utilities.Middleware
{
    public class SwaggerAuthenticationMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var path = context.Request.Path;
            if (path.StartsWithSegments("/swagger/v1/swagger.json") || path.StartsWithSegments("/swagger/v1/swagger.bak"))
            {
                string? authHeader = context.Request.Headers.Authorization;
                if (authHeader != null && authHeader.StartsWith("Basic "))
                {
                    // Get the credentials from request header
                    var header = AuthenticationHeaderValue.Parse(authHeader);
                    var inBytes = Convert.FromBase64String(header.Parameter ?? string.Empty);
                    var credentials = Encoding.UTF8.GetString(inBytes).Split(':');
                    var username = credentials[0];
                    var password = credentials[1];
                    // validate credentials
                    if (username.Equals("swagger") && password.Equals("swagger"))
                    {
                        context.Response.Headers.Remove("WWW-Authenticate");
                        await next.Invoke(context).ConfigureAwait(false);
                        return;
                    }
                    else
                    {
                        context.Response.Headers.WWWAuthenticate = "Basic";
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        var result = JsonSerializer.Serialize(new { message = "Unauthorized" });
                        await context.Response.WriteAsync(result);
                    }
                }
                else if (authHeader == null)
                {
                    context.Response.Headers.WWWAuthenticate = "Basic";
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    var result = JsonSerializer.Serialize(new { message = "Unauthorized" });
                    await context.Response.WriteAsync(result);
                    return;
                }
                await next.Invoke(context).ConfigureAwait(false);
            }
            else
            {
                await next.Invoke(context).ConfigureAwait(false);
            }
        }
    }
}