using System.ComponentModel.DataAnnotations;

namespace AP.Identity.Internal.Models.Tenants;

public record TenantContactsRequest
{
    public required int TenantId { get; set; }

    public List<ContactInput>? Contacts { get; set; }
}