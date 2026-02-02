using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AP.Common.Data.Identity.Entities;

[Table("UserSessions")]
public class UserSession
{
    [Key]
    public long SessionId { get; set; }

    public required int UserId { get; set; }
    public User? User { get; set; }

    [MaxLength(4096)]
    public required string AccessToken { get; set; }

    [MaxLength(4096)]
    public required string RefreshToken { get; set; }

    public required DateTime CreatedOn { get; set; }

    [MaxLength(64)]
    public string? CreatedFomIp { get; set; }
}