namespace PharmaCore.Infrastructure.Utilities;

public static class DateTimeHelper
{
    public static DateTime? NormalizeTimestamp(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        return DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified);
    }
}