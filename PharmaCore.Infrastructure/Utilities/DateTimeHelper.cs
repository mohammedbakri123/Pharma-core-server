namespace PharmaCore.Infrastructure.Utilities;

public static class DateTimeHelper
{
    public static DateTime? NormalizeTimestamp(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        return DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified);
    }

    public static DateTime NormalizeTimestamp(DateTime value)
    {
        return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
    }

    public static DateTime GetCurrentTimestamp()
    {
        return DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    }
}