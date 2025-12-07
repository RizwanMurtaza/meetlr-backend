namespace Meetlr.Application.Common.Settings;

public class GoogleApisSettings
{
    public string CalendarScope { get; set; } = "https://www.googleapis.com/auth/calendar";
    public string EmailScope { get; set; } = "https://www.googleapis.com/auth/userinfo.email";
    public string ProfileScope { get; set; } = "https://www.googleapis.com/auth/userinfo.profile";
    public string UserInfoEndpoint { get; set; } = "https://www.googleapis.com/oauth2/v2/userinfo";
}