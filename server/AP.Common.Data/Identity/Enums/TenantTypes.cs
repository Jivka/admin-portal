using System.ComponentModel.DataAnnotations;

namespace AP.Common.Data.Identity.Enums;

public enum TenantTypes : byte
{
    [Display(Name = "Company Tenant")]
    Company = 1,

    [Display(Name = "Personal Tenant")]
    Person,

    [Display(Name = "Organization Tenant")]
    Organization
}