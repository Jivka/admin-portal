namespace AP.Common.Models;

public record TenantRole
{
    public int? TenantId { get; init; }
    public string? TenantName { get; init; }
    public byte? RoleId { get; init; }
    public string? RoleName { get; init; }
    public string? RoleDisplayName { get; init; }
}