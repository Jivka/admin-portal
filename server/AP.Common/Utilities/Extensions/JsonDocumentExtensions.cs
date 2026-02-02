using System.Text.Json;

namespace AP.Common.Utilities.Extensions;

public static class JsonDocumentExtensions
{
    public static string? GetRootProperty(this JsonDocument jsonDocument, string propertyName)
    {
        if (jsonDocument == null)
        {
            return null;
        }

        var rootElement = jsonDocument.RootElement;
        if (rootElement.TryGetProperty(propertyName, out var element))
        {
            return element.ToString();
        }

        return null;
    }
}