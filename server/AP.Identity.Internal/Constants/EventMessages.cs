namespace AP.Identity.Internal.Constants;

public static class EventMessages
{
    // add login event messages
    public static readonly string LoginInvalidEmail = "Log-in failed with invalid email";
    public static readonly string LoginUnverifiedEmail = "Log-in failed with unverified email";
    public static readonly string LoginInvalidPassword = "Log-in failed with invalid password";
    public static readonly string LoggedIn = "Logged-in successfully";
}