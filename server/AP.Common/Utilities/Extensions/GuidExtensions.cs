namespace AP.Common.Utilities.Extensions;

public static class GuidExtensions
{
    public static HashSet<string> ConvertGuidHashSetToStringHashSet(this HashSet<Guid> guids)
    {
        var retHash = new HashSet<string>();

        foreach (var guid in guids)
        {
            retHash.Add(guid.ToString());
        }

        return retHash;
    }
}