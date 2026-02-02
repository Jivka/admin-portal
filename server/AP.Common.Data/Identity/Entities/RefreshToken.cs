using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AP.Common.Data.Identity.Entities;

[Owned]
[Table("RefreshTokens")]
public class RefreshToken
{
    [Key]
    public int TokenId { get; set; }

    [Required]
    [MaxLength(256)]
    public string? Token { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime ExpiresOn { get; set; }

    public DateTime CreatedOn { get; set; }

    [MaxLength(256)]
    public string? CreatedByIp { get; set; }

    public DateTime? RevokedOn { get; set; }

    [MaxLength(256)]
    public string? RevokedByIp { get; set; }

    [MaxLength(256)]
    public string? ReplacedByToken { get; set; }

    [MaxLength(256)]
    public string? ReasonRevoked { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresOn;

    public bool IsRevoked => RevokedOn is not null;

    public bool IsActive => RevokedOn is null && !IsExpired;
}
