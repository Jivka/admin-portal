using AP.Common.Services.Contracts;

namespace AP.Common.Services;

public class CurrentToken : ICurrentToken
{
    private string? currentToken;

    public string? Get() => currentToken;

    public void Set(string token) => currentToken = token;
}