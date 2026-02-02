namespace AP.Common.Utilities.Extensions;

public static class DateTimeExtensions
{
    public static long ToUnixTimestamp(this DateTime date)
    {
        return ((DateTimeOffset)date).ToUnixTimeSeconds();
    }
}