using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AP.Common.Data.Identity.Entities;

[Table("TenantTypes")]
public class TenantType
{
    [Key]
    public byte TypeId { get; set; }

    [MaxLength(32)]
    public required string TypeName { get; set; } // e.g., "Company", "Person", "Organization"

    [MaxLength(64)]
    public required string TypeSummary { get; set; }
}