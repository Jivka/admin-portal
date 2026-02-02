using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using static AP.Common.Constants.ErrorMessagesConstants;

namespace AP.Identity.Internal.Models;

public record ForgotPasswordRequest
{
    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
    };

    public ForgotPasswordRequest(string email)
    {
        Email = email;

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this);
        if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
        {
            var valErrors = validationResults.Select(vr => vr.ErrorMessage).ToList();
            var apiErrors = ForgotPasswordValidationError.WithMessages(valErrors);

            throw new ValidationException(JsonSerializer.Serialize(apiErrors, options));
        }
    }

    [Required]
    [EmailAddress]
    [StringLength(128)]
    public required string Email { get; set; }
}