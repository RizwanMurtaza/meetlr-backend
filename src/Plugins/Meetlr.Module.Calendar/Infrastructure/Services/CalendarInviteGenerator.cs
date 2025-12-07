using System.Text;
using Meetlr.Application.Interfaces;

namespace Meetlr.Module.Calendar.Infrastructure.Services;

/// <summary>
/// Generates ICS calendar invite files for email attachments
/// </summary>
public class CalendarInviteGenerator : ICalendarInviteGenerator
{
    public byte[] GenerateIcsFile(CalendarInviteRequest request)
    {
        var sb = new StringBuilder();

        // ICS file header
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//Meetlr//Calendar//EN");
        sb.AppendLine($"METHOD:{GetMethodString(request.Method)}");

        // Event
        sb.AppendLine("BEGIN:VEVENT");
        sb.AppendLine($"UID:{request.EventUid}");
        sb.AppendLine($"DTSTAMP:{FormatDateTime(DateTime.UtcNow)}");
        sb.AppendLine($"DTSTART:{FormatDateTime(request.StartTimeUtc)}");
        sb.AppendLine($"DTEND:{FormatDateTime(request.EndTimeUtc)}");
        sb.AppendLine($"SUMMARY:{EscapeText(request.Title)}");

        if (!string.IsNullOrEmpty(request.Description))
        {
            sb.AppendLine($"DESCRIPTION:{EscapeText(request.Description)}");
        }

        if (!string.IsNullOrEmpty(request.Location))
        {
            sb.AppendLine($"LOCATION:{EscapeText(request.Location)}");
        }

        if (!string.IsNullOrEmpty(request.MeetingUrl))
        {
            sb.AppendLine($"URL:{request.MeetingUrl}");
        }

        // Organizer
        sb.AppendLine($"ORGANIZER;CN={EscapeText(request.OrganizerName)}:mailto:{request.OrganizerEmail}");

        // Attendee
        sb.AppendLine($"ATTENDEE;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN={EscapeText(request.AttendeeName)}:mailto:{request.AttendeeEmail}");

        // Sequence for updates
        sb.AppendLine($"SEQUENCE:{request.Sequence}");

        // Status based on method
        if (request.Method == CalendarInviteMethod.Cancel)
        {
            sb.AppendLine("STATUS:CANCELLED");
        }
        else
        {
            sb.AppendLine("STATUS:CONFIRMED");
        }

        sb.AppendLine("END:VEVENT");
        sb.AppendLine("END:VCALENDAR");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("yyyyMMddTHHmmssZ");
    }

    private static string EscapeText(string text)
    {
        return text
            .Replace("\\", "\\\\")
            .Replace(",", "\\,")
            .Replace(";", "\\;")
            .Replace("\n", "\\n")
            .Replace("\r", "");
    }

    private static string GetMethodString(CalendarInviteMethod method)
    {
        return method switch
        {
            CalendarInviteMethod.Request => "REQUEST",
            CalendarInviteMethod.Cancel => "CANCEL",
            _ => "REQUEST"
        };
    }
}
