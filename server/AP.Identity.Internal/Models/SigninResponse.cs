using System.Text.Json.Serialization;
using AP.Identity.Internal.Models;
using AP.Common.Services.Contracts;

namespace AP.Identity.Internal.Models;

public record SigninResponse : IMapFrom<UserOutput>
{
    public int UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public byte RoleId { get; set; }
    public string? RoleName { get; set; }
    public bool Active { get; set; }
    public bool IsVerified { get; set; }
    public DateTime Created { get; set; }

    public string? FullName => $"{FirstName} {LastName}";
}