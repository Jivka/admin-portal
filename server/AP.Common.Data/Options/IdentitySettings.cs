namespace AP.Common.Data.Options;

public class IdentitySettings
{
    public required string Secret { get; set; }

    public required string EmailDomain { get; set; }

    public required string MinPasswordLength { get; set; }

    public required string MaxPasswordLength { get; set; }

    public required string PasswordSpecialCharacters { get; set; }

    public required string InitialPassword { get; set; }

    // access token time to expire (in days),
    public required int JwtTokenTTE { get; set; }

    // refresh token time to expire (in days),
    public required int RefreshTokenTTE { get; set; }

    // refresh token time to live (in days),
    // deleted from the database after this time
    public required int RefreshTokenTTL { get; set; }

    // reset token time to expire (in days),
    public required int ResetTokenTTE { get; set; }

    public required int SystemTenantId { get; set; }
}