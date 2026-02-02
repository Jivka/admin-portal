using AP.Common.Data.Identity.Entities;
using AP.Common.Services.Contracts;

namespace AP.Identity.Internal.Models;

public record RoleOutput : IMapFrom<Role>
{
    public byte RoleId { get; set; }

    public string? RoleName { get; set; }

    public string? RoleDescription { get; set; }
}