using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AP.Common.Constants;

namespace AP.Common.Models;

public class ApiResult
{
    internal static readonly JsonSerializerOptions Options = new ()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly ApiError? error;
    private readonly List<ApiError>? errors;

    internal ApiResult(bool succeeded, ApiError? error, List<ApiError>? errors, int statusCode)
    {
        Succeeded = succeeded;
        this.error = error;
        this.errors = errors;
        StatusCode = statusCode;
    }

    public static ApiResult Success
        => new (true, null, null, StatusCodes.Status200OK);
    public static ApiResult SuccessWith(int statusCode)
        => new(true, null, null, statusCode);

    public bool Succeeded { get; }
    public int StatusCode { get; }

    public ApiError? Error
        => Succeeded
            ? null
            : error;

    public List<ApiError>? Errors
        => this.Succeeded
            ? []
            : this.errors;

    public string? ErrorJson
    => Succeeded
        ? null
        : JsonSerializer.Serialize<ApiError>(error!, Options);

    public static implicit operator ActionResult(ApiResult result)
    {
        if (!result.Succeeded)
        {
            if (result.Error != null)
            {
                return CreateErrorJsonResult(result.Error.Code, result.Error.Message, result.StatusCode);
            }
            else if (result.Errors != null)
            {
                return CreateErrorJsonResult(result.Errors, result.StatusCode);
            }

            return CreateErrorJsonResult(ErrorMessagesConstants.UnexpectedError.Code, ErrorMessagesConstants.UnexpectedError.Message, result.StatusCode);
        }

        // use switch of result.StatusCode to return based on it
        return result.StatusCode switch
        {
            StatusCodes.Status201Created => new CreatedResult(string.Empty, null),
            StatusCodes.Status204NoContent => new NoContentResult(),
            _ => new OkResult(),
        };
    }

    public static ApiResult Failure(ApiError publicError, params object[] messageArgs)
    {
        if (messageArgs.Length > 0)
        {
            return new ApiResult(false, publicError.WithMessageArgs(messageArgs), null, publicError.StatusCode);
        }

        return new ApiResult(false, publicError, null, publicError.StatusCode);
    }

    public static ApiResult Failure(List<ApiError> publicErrors)
    {
        return new ApiResult(false, null, publicErrors, StatusCodes.Status400BadRequest);
    }

    protected static JsonResult CreateErrorJsonResult(string resultCode, string resultMessage, int statusCode)
    {
        return new JsonResult(
            new ApiError(resultCode, resultMessage, statusCode),
            Options)
            {
                StatusCode = statusCode,
            };
    }

    protected static JsonResult CreateErrorJsonResult(List<ApiError> errors, int statusCode)
    {
        var result = new JsonResult(
            errors.Select(e => new ApiError(e.Code, e.Message, e.StatusCode)),
            Options)
            {
                StatusCode = statusCode,
            };
        return result;
    }
}

public class ApiResult<TData> : ApiResult
{
    private readonly TData data;

    private ApiResult(bool succeeded, TData data, ApiError? error, List<ApiError>? errors, int statusCode)
        : base(succeeded, error, errors, statusCode)
        => this.data = data;

    public TData Data
        => data;

    public static implicit operator ActionResult<TData>(ApiResult<TData> result)
    {
        if (!result.Succeeded)
        {
            if (result.Error is not null)
            {
                return CreateErrorJsonResult(result.Error.Code, result.Error.Message, result.StatusCode);
            }
            else if (result.Errors != null)
            {
                return CreateErrorJsonResult(result.Errors, result.StatusCode);
            }

            return CreateErrorJsonResult(ErrorMessagesConstants.UnexpectedError.Code, ErrorMessagesConstants.UnexpectedError.Message, result.StatusCode);
        }
        else if (result.Data is null)
        {
            return new NoContentResult();
        }

        // use switch of result.StatusCode to return based on it
        return result.StatusCode switch
        {
            StatusCodes.Status201Created => new CreatedResult(string.Empty, result.Data),
            _ => new OkObjectResult(result.Data),
        };
    }

    public static ApiResult<TData> SuccessWith(TData data)
        => new (true, data, null, null, StatusCodes.Status200OK);
    public static ApiResult<TData> SuccessWith(TData data, int statusCode)
    => new(true, data, null, null, statusCode);

    public static new ApiResult<TData> Failure(ApiError publicError, params object[] messageArgs)
    {
        if (messageArgs.Length > 0)
        {
            return new (false, default!, publicError.WithMessageArgs(messageArgs), null, publicError.StatusCode);
        }

        return new (false, default!, publicError, null, publicError.StatusCode);
    }

    public static new ApiResult<TData> Failure(List<ApiError> publicErrors)
    {
        return new ApiResult<TData>(false, default!, null, publicErrors, StatusCodes.Status400BadRequest);
    }

    public static ApiResult<TData> Failure(ILogger logger, ApiError publicError, params object[] messageArgs)
    {
        var result = Failure(publicError, messageArgs);
        logger.LogError("{ErrorMessage}", result?.Error?.Message);

        return result!;
    }
}