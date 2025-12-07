namespace Meetlr.Application.Common.Settings;

public class ApplicationUrlsSettings
{
    // Default to localhost for development; override in appsettings.{Environment}.json
    public string BaseUrl { get; set; } = "http://localhost:3000";
    public string FrontendUrl { get; set; } = "http://localhost:3000";
    public string ApiUrl { get; set; } = "http://localhost:5000";

    public string BuildBookingUrl(string username, string? slug = null)
    {
        return string.IsNullOrEmpty(slug)
            ? $"{BaseUrl}/{username}"
            : $"{BaseUrl}/{username}/{slug}";
    }

    public string BuildBookingDetailsUrl(string confirmationToken)
    {
        return $"{BaseUrl}/bookings/{confirmationToken}";
    }

    public string BuildCancellationUrl(string cancellationToken)
    {
        return $"{BaseUrl}/bookings/{cancellationToken}/cancel";
    }

    public string BuildRescheduleUrl(Guid bookingId, string confirmationToken)
    {
        return $"{BaseUrl}/booking/{bookingId}/reschedule?token={confirmationToken}";
    }

    public string BuildCalendarCallbackUrl()
    {
        return $"{FrontendUrl}/calendar/callback";
    }
}
