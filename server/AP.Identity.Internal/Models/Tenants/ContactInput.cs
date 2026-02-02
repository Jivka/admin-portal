using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using static AP.Common.Constants.ErrorMessagesConstants;

namespace AP.Identity.Internal.Models.Tenants;

public record ContactInput
{
    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
    };

    // add ctor to validate the model as in TenantInput
    public ContactInput(int? contactId, string contactName, bool active, bool primary, string? email, string? phone, string? title, string? address)
    {
        ContactId = contactId;
        ContactName = contactName;
        Active = active;
        Primary = primary;
        Email = email;
        Phone = phone;
        Title = title;
        Address = address;

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this);
        if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
        {
            var valErrors = validationResults.Select(vr => vr.ErrorMessage).ToList();
            var apiErrors = ContactValidationError.WithMessages(valErrors);

            throw new ValidationException(JsonSerializer.Serialize(apiErrors, options));
        }
    }

    public int? ContactId { get; set; }

    [MaxLength(128)]
    public required string ContactName { get; set; }

    [EmailAddress]
    [MaxLength(128)]
    public string? Email { get; set; }

    [Phone]
    [MaxLength(64)]
    public string? Phone { get; set; }

    [MaxLength(64)]
    public string? Title { get; set; }

    [MaxLength(128)]
    public string? Address { get; set; }

    public required bool Active { get; set; }

    public required bool Primary { get; set; }
}