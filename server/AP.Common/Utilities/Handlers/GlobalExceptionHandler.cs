using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using static AP.Common.Constants.ErrorMessagesConstants;

namespace AP.Common.Utilities.Handlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly JsonSerializerOptions options = new() { WriteIndented = true/*, PropertyNamingPolicy = JsonNamingPolicy.CamelCase*/ };

    public async ValueTask<bool> TryHandleAsync(
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken)
    {
        var errorMessage = exception.Message;

        var response = httpContext.Response;
        response.ContentType = "application/json";

        switch (exception)
        {
            case ValidationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            default:
                // unhandled error
                logger.LogError(exception, "Exception catched by global error handler!");

                errorMessage = JsonSerializer.Serialize(UnexpectedError, options);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        await response.WriteAsync(errorMessage, cancellationToken);

        return true;
    }
}