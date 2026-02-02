namespace AP.Common.Utilities.Extensions;

public static class StringExtensions
{
    public static bool IsValidUrlFormat(this string url)
    {
        bool result = Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
                      && uriResult.IsWellFormedOriginalString()
                      && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        return result;
    }

    public static HashSet<Guid> ConvertListOfValidStringifiedGuidsToGuidHashSet(this string commaSeparatedListOfGuids)
    {
        HashSet<Guid> retValue = [];

        foreach (var guid in commaSeparatedListOfGuids.SplitByCharacter(','))
        {
            retValue.Add(Guid.Parse(guid));
        }

        return retValue;
    }

    public static List<string> NormalizeTags(this string? value) =>
        value?.ToLower().SplitByCharacter(',').NormalizeTags(toLower: false) ?? [];

    public static List<string> NormalizeTags(this List<string>? value, bool toLower = true)
    {
        if (value is null)
        {
            return [];
        }

        return toLower ? value.Select(t => t.ToLower()).Sanitize() : value.Sanitize();
    }

    public static List<string> SplitByCharacter(this string? value, char c) =>
        string.IsNullOrWhiteSpace(value) ? [] : [.. value!.Split(c)];

    private static List<string> Sanitize(this IEnumerable<string> list) =>
            [.. list.Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct()];
}