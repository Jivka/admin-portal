namespace AP.Common.Services.Contracts;

public interface ICurrentToken
{
    string? Get();

    void Set(string token);
}
