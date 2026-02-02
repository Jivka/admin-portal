using AP.Common.Data.Identity.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AP.Common.Data.Identity.Entities;

namespace AP.Common.Data.Identity.Entities;

[Table("Users")]
public class User
{
    [Key]
    public int UserId { get; set; }

    [MaxLength(128)]
    public string? UserSub { get; set; } // external reference key

    [MaxLength(64)]
    public string? Username { get; set; }

    [EmailAddress]
    [MaxLength(128)]
    public required string Email { get; set; }

    public bool? EmailVerified { get; set; }

    [MaxLength(256)]
    public required string PasswordHash { get; set; }

    [MaxLength(64)]
    public string? Nickname { get; set; }

    [MaxLength(64)]
    public string? FirstName { get; set; }

    [MaxLength(64)]
    public string? LastName { get; set; }

    [Phone]
    [MaxLength(64)]
    public string? Phone { get; set; }

    [MaxLength(128)]
    public string? PictureUrl { get; set; }

    public required bool Active { get; set; }

    public required bool Enabled { get; set; }

    public DateTime? VerifiedOn { get; set; }

    [MaxLength(256)]
    public string? VerificationToken { get; set; }

    [MaxLength(256)]
    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpires { get; set; }
    public DateTime? PasswordReset { get; set; }
    public DateTime? PasswordChanged { get; set; }

    public required DateTime CreatedOn { get; set; }

    [ForeignKey("CreatedByUser")]
    public int? CreatedBy { get; set; }
    public User? CreatedByUser { get; set; }

    public DateTime? UpdatedOn { get; set; }

    [ForeignKey("UpdatedByUser")]
    public int? UpdatedBy { get; set; }
    public User? UpdatedByUser { get; set; }

    public bool IsVerified => EmailVerified == true || VerifiedOn.HasValue || PasswordReset.HasValue;

    public bool IsSystemAdministrator => UserTenants!.Any(ut => ut.RoleId == (byte)Roles.SystemAdmin);

    public bool OwnsRefreshToken(string refreshToken)
    {
        return this.RefreshTokens?.Find(x => x.Token == refreshToken) != null;
    }

    public string FullName { get => FirstName + " " + LastName; }

    public List<RefreshToken>? RefreshTokens { get; set; }

    public List<UserSession>? Sessions { get; set; }

    public List<UserTenant>? UserTenants { get; set; } = [];
}