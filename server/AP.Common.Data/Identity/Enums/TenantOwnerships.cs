using System.ComponentModel.DataAnnotations;

namespace AP.Common.Data.Identity.Enums;

public enum TenantOwnerships : byte
{
    [Display(Name = "Tenant Owner")]
    Owner = 1,

    [Display(Name = "Tenant Installer")]
    Installer
}