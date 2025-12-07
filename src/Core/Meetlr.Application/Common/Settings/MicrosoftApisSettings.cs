namespace Meetlr.Application.Common.Settings;

public class MicrosoftApisSettings
{
    public string Authority { get; set; } = "https://login.microsoftonline.com/common";
    public string CalendarScope { get; set; } = "https://graph.microsoft.com/Calendars.ReadWrite";
    public string EmailScope { get; set; } = "https://graph.microsoft.com/User.Read";
    public string UserInfoEndpoint { get; set; } = "https://graph.microsoft.com/v1.0/me";
}