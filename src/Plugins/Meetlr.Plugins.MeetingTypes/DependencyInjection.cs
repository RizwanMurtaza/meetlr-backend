using Meetlr.Application.Plugins;
using Meetlr.Application.Plugins.MeetingTypes;
using Meetlr.Application.Plugins.Services;
using Meetlr.Infrastructure.Services;
using Meetlr.Plugins.MeetingTypes.Providers.GoogleMeet;
using Meetlr.Plugins.MeetingTypes.Providers.Jitsi;
using Meetlr.Plugins.MeetingTypes.Providers.MicrosoftTeams;
using Meetlr.Plugins.MeetingTypes.Providers.Slack;
using Meetlr.Plugins.MeetingTypes.Providers.Zoom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meetlr.Plugins.MeetingTypes;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Meeting Types plugin services
    /// </summary>
    public static IServiceCollection AddMeetingTypesPlugin(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Jitsi settings
        services.Configure<JitsiSettings>(options =>
        {
            var jitsiSection = configuration.GetSection("VideoConferencing:Jitsi");
            options.ServerUrl = jitsiSection["ServerUrl"] ?? "https://meet.jit.si";
            options.RoomPrefix = jitsiSection["RoomPrefix"] ?? "meetlr";
        });

        // Configure Zoom settings
        services.Configure<ZoomSettings>(options =>
        {
            var zoomSection = configuration.GetSection("VideoConferencing:Zoom");
            options.ClientId = zoomSection["ClientId"] ?? string.Empty;
            options.ClientSecret = zoomSection["ClientSecret"] ?? string.Empty;
            options.RedirectUri = zoomSection["RedirectUri"] ?? string.Empty;
            options.AuthorizationUrl = zoomSection["AuthorizationUrl"] ?? "https://zoom.us/oauth/authorize";
            options.TokenUrl = zoomSection["TokenUrl"] ?? "https://zoom.us/oauth/token";
            options.ApiBaseUrl = zoomSection["ApiBaseUrl"] ?? "https://api.zoom.us/v2";
        });

        // Configure Slack settings
        services.Configure<SlackSettings>(options =>
        {
            var slackSection = configuration.GetSection("VideoConferencing:Slack");
            options.ClientId = slackSection["ClientId"] ?? string.Empty;
            options.ClientSecret = slackSection["ClientSecret"] ?? string.Empty;
            options.RedirectUri = slackSection["RedirectUri"] ?? string.Empty;
            options.AuthorizationUrl = slackSection["AuthorizationUrl"] ?? "https://slack.com/oauth/v2/authorize";
            options.TokenUrl = slackSection["TokenUrl"] ?? "https://slack.com/api/oauth.v2.access";
            options.ApiBaseUrl = slackSection["ApiBaseUrl"] ?? "https://slack.com/api";
            options.Scopes = slackSection["Scopes"] ?? "calls:read,calls:write,users:read,users:read.email";
        });

        // Register concrete provider types
        services.AddScoped<JitsiMeetProvider>();
        services.AddScoped<GoogleMeetProvider>();
        services.AddScoped<MicrosoftTeamsProvider>();
        services.AddScoped<ZoomProvider>();
        services.AddScoped<SlackHuddleProvider>();

        // Register as IPlugin for unified factory
        services.AddScoped<IPlugin>(sp => sp.GetRequiredService<JitsiMeetProvider>());
        services.AddScoped<IPlugin>(sp => sp.GetRequiredService<GoogleMeetProvider>());
        services.AddScoped<IPlugin>(sp => sp.GetRequiredService<MicrosoftTeamsProvider>());
        services.AddScoped<IPlugin>(sp => sp.GetRequiredService<ZoomProvider>());
        services.AddScoped<IPlugin>(sp => sp.GetRequiredService<SlackHuddleProvider>());

        // Register as IMeetingTypesPlugin for meeting-specific operations
        services.AddScoped<IMeetingTypesPlugin>(sp => sp.GetRequiredService<JitsiMeetProvider>());
        services.AddScoped<IMeetingTypesPlugin>(sp => sp.GetRequiredService<GoogleMeetProvider>());
        services.AddScoped<IMeetingTypesPlugin>(sp => sp.GetRequiredService<MicrosoftTeamsProvider>());
        services.AddScoped<IMeetingTypesPlugin>(sp => sp.GetRequiredService<ZoomProvider>());
        services.AddScoped<IMeetingTypesPlugin>(sp => sp.GetRequiredService<SlackHuddleProvider>());

        // Register the service interface for Application layer consumption
        services.AddScoped<IMeetingTypeService, MeetingTypeService>();

        // Register plugin auto-install service
        services.AddScoped<IPluginAutoInstallService, PluginAutoInstallService>();

        return services;
    }
}
