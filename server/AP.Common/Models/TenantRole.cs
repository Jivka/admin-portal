namespace AP.Common.Models;

public record TenantRole
{
    public int? TenantId { get; init; }
    public byte? RoleId { get; init; }
    public string? RoleName { get; init; }
}
