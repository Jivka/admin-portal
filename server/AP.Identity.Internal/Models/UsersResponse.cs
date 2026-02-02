namespace AP.Identity.Internal.Models;

public record UsersResponse
{
    public int Count { get; set; }

    public int? Page { get; set; }

    public int? Size { get; set; }

    public string? Name { get; set; }

    public byte? Role { get; set; }

    public string? Sort { get; set; }

    public List<UserOutput>? Users { get; set; }
}