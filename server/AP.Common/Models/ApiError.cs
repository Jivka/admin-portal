using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace AP.Common.Models;

public class ApiError(string code, string message, int statusCode = StatusCodes.Status400BadRequest)
{
    [JsonPropertyName("error-code")]
    public string Code { get; } = code;

    [JsonPropertyName("error-message")]
    public string Message { get; set; } = message;

    [JsonPropertyName("error-details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string?>? Messages { get; set; }

    [JsonIgnore]
    public int StatusCode { get; } = statusCode;

    public ApiError WithMessageArgs(params object[] messageArgs)
    {
        this.Message = string.Format(this.Message, messageArgs);

        return this;
    }

    public ApiError WithMessages(List<string?>? messages)
    {
        this.Messages = messages;

        return this;
    }

}