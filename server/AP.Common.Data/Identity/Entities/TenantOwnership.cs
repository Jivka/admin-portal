using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AP.Common.Data.Identity.Entities;

[Table("TenantOwnerships")]
public class TenantOwnership
{
    [Key]
    public byte OwnershipId { get; set; }

    [MaxLength(32)]
    public required string OwnershipName { get; set; } // TenantOwnerships enum value

    [MaxLength(64)]
    public required string OwnershipSummary { get; set; }
}