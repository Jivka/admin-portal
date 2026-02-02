using AP.Common.Data.Identity.Entities;
using AP.Common.Services.Contracts;

namespace AP.Identity.Internal.Models.Tenants;

public record ContactOutput
{
    public int ContactId { get; set; }
    public required string ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Title { get; set; }
    public string? Address { get; set; }
    public required bool Active { get; set; }
    public required bool Primary { get; set; }
    public DateTime CreatedOn { get; set; }
}
