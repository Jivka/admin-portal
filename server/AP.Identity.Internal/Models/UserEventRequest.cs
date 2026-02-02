namespace AP.Identity.Internal.Models;

public record UserEventRequest
{
    public long EventId { get; set; }
    public int? UserId { get; set; }
    public string? UserSub { get; set; } // external reference key
    public string? Username { get; set; }
    public required string UserEmail { get; set; }
    public required string Action { get; set; }
    public bool? Success { get; set; }
    public required DateTime ActionOn { get; set; }
    public string? ActionFromIp { get; set; }
}