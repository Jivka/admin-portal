using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AP.Common.Utilities.Attributes;
using static AP.Common.Constants.ErrorMessagesConstants;

namespace AP.Identity.Internal.Models;

public record VerifyEmailRequest
{
    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
    };

    public VerifyEmailRequest(string verificationToken, string email, string password, string confirmPassword)
    {
        VerificationToken = verificationToken;
        Email = email;
        Password = password;
        ConfirmPassword = confirmPassword;

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this);
        if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
        {
            var valErrors = validationResults.Select(vr => vr.ErrorMessage).ToList();
            var apiErrors = VerifyEmailValidationError.WithMessages(valErrors);

            throw new ValidationException(JsonSerializer.Serialize(apiErrors, options));
        }
    }

    [Required]
    [StringLength(256)]
    public required string VerificationToken { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(128)]
    public required string Email { get; set; }

    [Required]
    [PasswordStrengthValidation]
    public required string Password { get; set; }

    [Required]
    [Compare("Password")]
    public required string ConfirmPassword { get; set; }
}