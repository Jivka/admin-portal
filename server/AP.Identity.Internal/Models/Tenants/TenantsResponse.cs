namespace AP.Identity.Internal.Models.Tenants;

public record TenantsResponse
{
    public int Count { get; set; }

    public int? Page { get; set; }

    public int? Size { get; set; }

    public string? Name { get; set; }

    public string? Sort { get; set; }

    public List<TenantOutput>? Tenants { get; set; }
}