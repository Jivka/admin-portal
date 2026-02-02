namespace AP.Identity.Internal.Constants;

public static class NoticeMessages
{
    public static readonly string AttemptedReuse = "Attempted reuse of revoked ancestor token: {0}";
    public static readonly string RevokedWithNoReplacement = "Revoked without replacement";
    public static readonly string RevokedWithReplacement = "Revoked with replacement by new token";
    public static readonly string RefreshTokenRevoked = "Refresh Token revoked successfully";
    public static readonly string PasswordChangedSuccessfully = "Password is changed successfully";
    public static readonly string EmailWithPasswordResetInstructions = "Email is sent with password reset instructions";
    public static readonly string PasswordResetSuccessfully = "Password is reset successfully";
}