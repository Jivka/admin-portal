using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using static AP.Common.Constants.ErrorMessagesConstants;

namespace AP.Identity.Internal.Models;

public record RevokeTokenRequest
{
    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
    };

    public RevokeTokenRequest(string email, string refreshToken)
    {
        Email = email;
        RefreshToken = refreshToken;
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this);

        if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
        {
            var valErrors = validationResults.Select(vr => vr.ErrorMessage).ToList();
            var apiErrors = RevokeTokenValidationError.WithMessages(valErrors);

            throw new ValidationException(JsonSerializer.Serialize(apiErrors, options));
        }
    }

    [Required]
    [StringLength(256)]
    public required string RefreshToken { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(128)]
    public required string Email { get; set; }
}