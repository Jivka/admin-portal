using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using static AP.Common.Constants.ErrorMessagesConstants;

namespace AP.Identity.Internal.Models;

public record SignupRequest
{
    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
    };

    public SignupRequest(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this);
        if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
        {
            var valErrors = validationResults.Select(vr => vr.ErrorMessage).ToList();
            var apiErrors = SignupValidationError.WithMessages(valErrors);

            throw new ValidationException(JsonSerializer.Serialize(apiErrors, options));
        }
    }

    [Required]
    [StringLength(256)]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(256)]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public required string Email { get; set; }
}