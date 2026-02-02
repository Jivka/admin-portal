using System.ComponentModel.DataAnnotations;

namespace AP.Common.Data.Identity.Enums;

public enum Roles : byte
{
    [Display(Name = "System Administrator", Description = "Administrator of the entire system")]
    SystemAdmin = 1,

    [Display(Name = "Tenant Administrator", Description = "Administrator of a tenant")]
    TenantAdmin,

    [Display(Name = "Power User", Description = "Read-write user for entities within a tenant")]
    PowerUser,

    [Display(Name = "End User", Description = "Read-only user for entities within a tenant")]
    EndUser,
    
    [Display(Name = "Test User", Description = "User for system test purposes")]
    TestUser,
    
    [Display(Name = "Demo User", Description = "User for system demo purposes")]
    DemoUser,
}