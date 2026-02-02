using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AP.Common.Data.Identity.Enums;

namespace AP.Common.Data.Identity.Entities;

[Table("UserTenants")]
public class UserTenant
{
    [Key]
    public int? UserId { get; set; }
    [Key]
    public int? TenantId { get; set; }

    public User? User { get; set; }
    public Tenant? Tenant { get; set; }

    public required byte RoleId { get; set; }
    public Role? Role { get; set; }

    public required DateTime CreatedOn { get; set; }

    [ForeignKey("CreatedByUser")]
    public int? CreatedBy { get; set; }
    public User? CreatedByUser { get; set; }

    public bool IsSystemAdministrator => RoleId == (byte)Roles.SystemAdmin;

    public bool IsTenantAdministrator => RoleId == (byte)Roles.TenantAdmin;

    public string RoleName => Role?.RoleName != null ? Role.RoleName : string.Empty;
}