using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AP.Common.Data.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        string? displayName;
        displayName = enumValue.GetType()
            .GetMember(enumValue.ToString())?
            .FirstOrDefault()?
            .GetCustomAttribute<DisplayAttribute>()?
            .GetName();

        if (string.IsNullOrEmpty(displayName))
        {
            displayName = enumValue.ToString();
        }

        return displayName;
    }

    public static string GetDescription(this Enum enumValue)
    {
        string? displayName;
        displayName = enumValue.GetType()
            .GetMember(enumValue.ToString())?
            .FirstOrDefault()?
            .GetCustomAttribute<DisplayAttribute>()?
            .GetDescription();

        if (string.IsNullOrEmpty(displayName))
        {
            displayName = enumValue.ToString();
        }

        return displayName;
    }
}
