namespace AP.Identity.Internal.Models.Tenants;

public record TenantContactsResponse
{
    public int TenantId { get; set; }

    public List<ContactOutput>? Contacts { get; set; }
}