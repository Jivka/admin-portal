using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using AP.Common.Data.Options;
using static AP.Common.Constants.Constants;

namespace AP.Common.Utilities.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public class EmailDomainValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var emailDomain = string.Empty;
        if (validationContext != null)
        {
            var serviceOptions = (IOptions<IdentitySettings>?)validationContext.GetService(typeof(IOptions<IdentitySettings>));
            var service = serviceOptions?.Value;
            if (service != null && service.EmailDomain != null)
            {
                emailDomain = service.EmailDomain;
            }
        }

        string emailDomainexpression = @"^[a-zA-Z0-9._%+-]+(" + Regex.Escape(emailDomain) + @")$";
        Regex regex = new(emailDomainexpression);
        if (value is string email)
        {
            if (regex.IsMatch(email))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(string.Format(EmailDomainErrorMessage, emailDomain));
            }
        }
        else
        {
            return new ValidationResult(EmailMissingErrorMessage);
        }
    }
}