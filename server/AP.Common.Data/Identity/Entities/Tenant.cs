using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AP.Common.Data.Identity.Enums;

namespace AP.Common.Data.Identity.Entities;

[Table("Tenants")]
public class Tenant
{
    [Key]
    public int TenantId { get; set; }

    [MaxLength(128)]
    public required string TenantName { get; set; }

    [MaxLength(128)]
    public required string TenantBIC { get; set; }

    [ForeignKey("TenantTypeObj")]
    public required byte TenantType { get; set; }
    public TenantType? TenantTypeObj { get; set; }

    [ForeignKey("TenantOwnership")]
    public required byte Ownership { get; set; }
    public TenantOwnership? TenantOwnership { get; set; }

    [MaxLength(128)]
    public string? Domain { get; set; }

    [MaxLength(256)]
    public string? Summary { get; set; }

    [MaxLength(256)]
    public string? LogoUrl { get; set; }

    public required bool Active { get; set; }
    public required bool Enabled { get; set; }

    public required DateTime CreatedOn { get; set; }

    [ForeignKey("CreatedByUser")]
    public int? CreatedBy { get; set; }
    public User? CreatedByUser { get; set; }

    public DateTime? UpdatedOn { get; set; }

    [ForeignKey("UpdatedByUser")]
    public int? UpdatedBy { get; set; }
    public User? UpdatedByUser { get; set; }

    public List<TenantContact>? TenantContacts { get; set; } = [];

    ////public List<User>? TenantUsers { get; set; } = [];
}