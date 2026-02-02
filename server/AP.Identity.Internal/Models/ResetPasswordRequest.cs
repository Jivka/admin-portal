using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AP.Common.Utilities.Attributes;
using static AP.Common.Constants.ErrorMessagesConstants;

namespace AP.Identity.Internal.Models;

public record ResetPasswordRequest
{
    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
    };

    public ResetPasswordRequest(string email, string resetToken, string password, string confirmPassword)
    {
        Email = email;
        ResetToken = resetToken;
        Password = password;
        ConfirmPassword = confirmPassword;

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this);
        if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
        {
            var valErrors = validationResults.Select(vr => vr.ErrorMessage).ToList();
            var apiErrors = ResetPasswordValidationError.WithMessages(valErrors);

            throw new ValidationException(JsonSerializer.Serialize(apiErrors, options));
        }
    }

    [Required]
    public required string ResetToken { get; set; }

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