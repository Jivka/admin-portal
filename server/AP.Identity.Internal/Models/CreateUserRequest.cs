using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AP.Common.Utilities.Attributes;
using static AP.Common.Constants.ErrorMessagesConstants;

namespace AP.Identity.Internal.Models;

public record CreateUserRequest
{
    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
    };

    public CreateUserRequest(string firstName, string lastName, string email, string? phone, byte roleId, int tenantId)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        RoleId = roleId;
        TenantId = tenantId;

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this);
        if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
        {
            var valErrors = validationResults.Select(vr => vr.ErrorMessage).ToList();
            var apiErrors = CreateUserValidationError.WithMessages(valErrors);

            throw new ValidationException(JsonSerializer.Serialize(apiErrors, options));
        }
    }

    [Required]
    [StringLength(64)]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(64)]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(128)]
    public required string Email { get; set; }

    [Phone]
    [StringLength(64)]
    public string? Phone { get; set; }

    [Required]
    public required byte RoleId { get; set; }

    [Required]
    public required int TenantId { get; set; }
}