using Meetlr.Application.Interfaces;

namespace Meetlr.Infrastructure.Services;

public class TimeZoneService : ITimeZoneService
{
    public DateTime ConvertToTimeZone(DateTime dateTime, string fromTimeZone, string toTimeZone)
    {
        var fromTz = FindTimeZone(fromTimeZone);
        var toTz = FindTimeZone(toTimeZone);

        var utcTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, fromTz);
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, toTz);
    }

    public DateTime ConvertToUtc(DateTime dateTime, string fromTimeZone)
    {
        var tz = FindTimeZone(fromTimeZone);

        // Always treat input as a local time in the specified timezone,
        // regardless of the DateTime.Kind property. The caller is saying
        // "this time is in fromTimeZone, please convert it to UTC".
        var unspecifiedDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecifiedDateTime, tz);
    }

    public DateTime ConvertFromUtc(DateTime dateTime, string toTimeZone)
    {
        var tz = FindTimeZone(toTimeZone);
        // Ensure the input is treated as UTC
        var utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);
    }

    public string DetectTimeZoneFromIp(string ipAddress)
    {
        // This would typically call an external API service like ipapi.co or similar
        // For now, return UTC as default
        return "UTC";
    }

    public List<string> GetAllTimeZones()
    {
        return TimeZoneInfo.GetSystemTimeZones()
            .Select(tz => tz.Id)
            .ToList();
    }

    /// <summary>
    /// Finds a TimeZoneInfo by ID, supporting both IANA (e.g., "Europe/London", "Asia/Kolkata")
    /// and Windows (e.g., "GMT Standard Time", "India Standard Time") timezone IDs.
    /// .NET 6+ supports IANA IDs natively on Windows when ICU is available.
    /// </summary>
    private static TimeZoneInfo FindTimeZone(string timeZoneId)
    {
        // First, try to find directly (works for both IANA and Windows IDs in .NET 6+)
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            // If not found, try to convert IANA to Windows ID
            if (TimeZoneInfo.TryConvertIanaIdToWindowsId(timeZoneId, out var windowsId))
            {
                return TimeZoneInfo.FindSystemTimeZoneById(windowsId);
            }

            // Try to convert Windows to IANA ID (in case it's a Windows ID on a non-Windows system)
            if (TimeZoneInfo.TryConvertWindowsIdToIanaId(timeZoneId, out var ianaId))
            {
                return TimeZoneInfo.FindSystemTimeZoneById(ianaId);
            }

            // If all else fails, throw the original exception
            throw;
        }
    }
}
