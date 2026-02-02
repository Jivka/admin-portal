using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using AP.Common.Data.Options;
using static AP.Common.Constants.Constants;

namespace AP.Common.Utilities.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public class PasswordStrengthValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var min = "6";
        var max = "16";
        var specialCharacters = "*";

        if (validationContext != null)
        {
            var serviceOptions = (IOptions<IdentitySettings>?)validationContext.GetService(typeof(IOptions<IdentitySettings>));
            var service = serviceOptions?.Value;
            if (service != null && service.MinPasswordLength != null && service.MaxPasswordLength != null)
            {
                min = service.MinPasswordLength;
                max = service.MaxPasswordLength;
                specialCharacters = service.PasswordSpecialCharacters;
            }
        }

        string passwordStrengthExpression = @"((?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[" + specialCharacters + "]).{" + min + "," + max + "})";
        Regex regex = new(passwordStrengthExpression);
        if (value is string password)
        {
            if (regex.IsMatch(password))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(string.Format(PasswordStrengthErrorMessage, min, max, specialCharacters));
            }
        }
        else
        {
            return new ValidationResult(string.Format(PasswordStrengthErrorMessage, min, max, specialCharacters));
        }
    }
}