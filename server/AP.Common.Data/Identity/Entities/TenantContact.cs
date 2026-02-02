using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AP.Common.Data.Identity.Entities;

[Table("TenantContacts")]
public class TenantContact
{
    [Key]
    public int? TenantId { get; set; }
    [Key]
    public int? ContactId { get; set; }

    public Tenant? Tenant { get; set; }
    public Contact? Contact { get; set; }

    public required bool Active { get; set; }

    public required bool Primary { get; set; }

    public required DateTime CreatedOn { get; set; }
}