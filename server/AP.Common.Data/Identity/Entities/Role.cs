using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AP.Common.Data.Identity.Entities;

[Table("Roles")]
public class Role
{
    [Key]
    public byte RoleId { get; set; }

    [MaxLength(32)]
    public required string RoleName { get; set; }

    [MaxLength(64)]
    public required string RoleDisplayName { get; set; }

    [MaxLength(128)]
    public required string RoleSummary { get; set; }
}