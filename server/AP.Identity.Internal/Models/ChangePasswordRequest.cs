using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AP.Common.Utilities.Attributes;
using static AP.Common.Constants.ErrorMessagesConstants;

namespace AP.Identity.Internal.Models;

public record ChangePasswordRequest
{
    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
    };

    public ChangePasswordRequest(int userId, string email, string currentPassword, string newPassword)
    {
        UserId = userId;
        Email = email;
        CurrentPassword = currentPassword;
        NewPassword = newPassword;

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this);
        if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
        {
            var valErrors = validationResults.Select(vr => vr.ErrorMessage).ToList();
            var apiErrors = ChangePasswordValidationError.WithMessages(valErrors);

            throw new ValidationException(JsonSerializer.Serialize(apiErrors, options));
        }
    }

    [Required]
    public required int UserId { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(128)]
    public required string Email { get; set; }

    [Required]
    public required string CurrentPassword { get; set; }

    [Required]
    [PasswordStrengthValidation]
    public required string NewPassword { get; set; }
}