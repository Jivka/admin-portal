using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using static AP.Common.Constants.ErrorMessagesConstants;

namespace AP.Identity.Internal.Models.Tenants;

public record TenantRequest
{
    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
    };

    public TenantRequest(
        string tenantName,
        string tenantBic,
        byte tenantType,
        byte ownership,
        List<ContactInput>? contacts,
        TenantRequestOptionalFields? optionalFields = null)
    {
        TenantName = tenantName;
        TenantBIC = tenantBic;
        TenantType = tenantType;
        Ownership = ownership;
        Contacts = contacts;
        OptionalFields = optionalFields;

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this);
        if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
        {
            var valErrors = validationResults.Select(vr => vr.ErrorMessage).ToList();
            var apiErrors = TenantValidationError.WithMessages(valErrors);

            throw new ValidationException(JsonSerializer.Serialize(apiErrors, options));
        }
    }

    [Required]
    [MaxLength(128)]
    public required string TenantName { get; set; }

    [MaxLength(128)]
    public required string TenantBIC { get; set; }

    [Required]
    public required byte TenantType { get; set; }

    [Required]
    public required byte Ownership { get; set; }

    public TenantRequestOptionalFields? OptionalFields { get; set; }

    public List<ContactInput>? Contacts { get; set; }
}

public class TenantRequestOptionalFields
{
    [MaxLength(128)]
    public string? Domain { get; set; }
    [MaxLength(256)]
    public string? Summary { get; set; }
    [MaxLength(256)]
    public string? LogoUrl { get; set; }
}