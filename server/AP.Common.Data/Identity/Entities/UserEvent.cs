using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AP.Common.Data.Identity.Entities;

[Table("UserEvents")]
public class UserEvent
{
    [Key]
    public long EventId { get; set; }

    public int? UserId { get; set; }

    [MaxLength(128)]
    public string? UserSub { get; set; } // external reference key

    [MaxLength(64)]
    public string? Username { get; set; }

    [MaxLength(128)]
    public required string UserEmail { get; set; }

    [MaxLength(256)]
    public required string Action { get; set; }

    public bool? Success { get; set; }

    public required DateTime ActionOn { get; set; }

    [MaxLength(64)]
    public string? ActionFromIp { get; set; }
}
