namespace AP.Identity.Internal.Models;

public record SendEmailRequest
{
    public required string To { get; set; }
    public required string Subject { get; set; }
    public required string Html { get; set; }
}