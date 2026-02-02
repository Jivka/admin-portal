using System.Reflection;

namespace AP.Common.Utilities.Extensions;

public static class ObjectExtensions
{
    public static T TrimStringProperties<T>(this object obj)
    {
        var stringProperties = obj.GetType().GetProperties().Where(p => p.PropertyType == typeof(string));

        foreach (PropertyInfo propertyInfo in stringProperties)
        {
            var value = (string?)propertyInfo.GetValue(obj, null);
            if (value != null)
            {
                propertyInfo.SetValue(obj, value.Trim(), null);
            }
        }

        return (T)obj;
    }
}