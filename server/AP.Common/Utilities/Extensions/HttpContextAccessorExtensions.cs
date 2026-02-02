using AP.Common.Constants;
using Microsoft.AspNetCore.Http;

namespace AP.Common.Utilities.Extensions;

public static class HttpContextAccessorExtensions
{
    public static string GetTraceId(this IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
    }
}