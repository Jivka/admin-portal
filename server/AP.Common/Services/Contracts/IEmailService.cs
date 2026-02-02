namespace AP.Common.Services.Contracts;

public interface IEmailService
{
    Task Send(string to, string subject, string html, string? from = null);
}