using AP.Common.Data.Identity.Entities;
using AP.Common.Services.Contracts;

namespace AP.Identity.Internal.Models.Tenants;

public record TenantOutput : IMapFrom<Tenant>
{
    public int TenantId { get; set; }
    public required string TenantName { get; set; }
    public required string TenantBIC { get; set; }
    public required byte TenantType { get; set; }
    public required byte Ownership { get; set; }
    public string? Domain { get; set; }
    public string? Summary { get; set; }
    public string? LogoUrl { get; set; }
    public required bool Active { get; set; }
    public required bool Enabled { get; set; }
    public required DateTime CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public int? UpdatedBy { get; set; }

    public List<ContactOutput>? Contacts { get; set; }

    ////public List<UserOutput>? Users { get; set; }
}