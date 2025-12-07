using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Plugins;
using Meetlr.Application.Plugins.MeetingTypes;
using Meetlr.Domain.Entities.Plugins;
using Meetlr.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Api.Endpoints.Communication.VideoConferencing;

[ApiController]
[Route("api/video-conferencing")]
[Authorize]
public class GetAvailableLocationTypes : ControllerBase
{
    private readonly IPluginFactory _pluginFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public GetAvailableLocationTypes(
        IPluginFactory pluginFactory,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _pluginFactory = pluginFactory;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("available-types")]
    [ProducesResponseType(typeof(AvailableLocationTypesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AvailableLocationTypesResponse>> Handle(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get user's installed meeting type plugins
        var installedPlugins = await _unitOfWork.Repository<UserInstalledPlugin>()
            .GetQueryable()
            .Where(p => p.UserId == userId &&
                        p.PluginCategory == PluginCategory.VideoConferencing &&
                        !p.IsDeleted)
            .ToListAsync(cancellationToken);

        var locationTypes = new List<LocationTypeInfo>();
        var connections = new ConnectionStatus();

        // Add always-available types (no plugin required)
        foreach (var alwaysAvailableType in MeetingTypePluginMetadata.AlwaysAvailable)
        {
            locationTypes.Add(new LocationTypeInfo
            {
                Type = alwaysAvailableType,
                Name = GetLocationTypeName(alwaysAvailableType),
                IsAvailable = true,
                RequiresConnection = false,
                RequiresPlugin = false,
                IsPluginInstalled = true // Not applicable, always available
            });
        }

        // Add plugin-based meeting types
        var meetingPlugins = _pluginFactory.GetMeetingTypinsPlugins();

        foreach (var plugin in meetingPlugins)
        {
            // Skip always-available types (already added above)
            if (MeetingTypePluginMetadata.IsAlwaysAvailable(plugin.LocationType))
            {
                continue;
            }

            var installedPlugin = installedPlugins.FirstOrDefault(p => p.PluginId == plugin.PluginId);
            var isInstalled = installedPlugin != null;
            var isEnabled = installedPlugin?.IsEnabled ?? false;
            var isConnected = installedPlugin?.IsConnected ?? false;

            // Check actual connection status from the provider
            if (isInstalled && isEnabled)
            {
                isConnected = await plugin.IsAvailableForUserAsync(userId, cancellationToken);

                // Update plugin connection status if it changed
                if (installedPlugin != null && installedPlugin.IsConnected != isConnected)
                {
                    installedPlugin.IsConnected = isConnected;
                    installedPlugin.ConnectionStatus = isConnected ? "connected" : "disconnected";
                    if (isConnected && !installedPlugin.ConnectedAt.HasValue)
                    {
                        installedPlugin.ConnectedAt = DateTime.UtcNow;
                    }
                }
            }

            var unavailableReason = GetUnavailableReason(isInstalled, isEnabled, isConnected, plugin.PluginName);

            locationTypes.Add(new LocationTypeInfo
            {
                Type = plugin.LocationType,
                Name = plugin.PluginName,
                IsAvailable = isInstalled && isEnabled && isConnected,
                UnavailableReason = unavailableReason,
                ProviderId = plugin.PluginId,
                RequiresConnection = plugin.RequiresConnection,
                RequiresPlugin = true,
                IsPluginInstalled = isInstalled,
                IsPluginEnabled = isEnabled,
                PluginId = plugin.PluginId
            });

            // Update connection status for response
            UpdateConnectionStatus(connections, plugin.LocationType, isConnected, installedPlugin);
        }

        // Save any connection status updates
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Sort: available first, then by type enum value
        locationTypes = locationTypes
            .OrderByDescending(l => l.IsAvailable)
            .ThenBy(l => (int)l.Type)
            .ToList();

        return Ok(new AvailableLocationTypesResponse
        {
            LocationTypes = locationTypes,
            Connections = connections
        });
    }

    private static string GetLocationTypeName(MeetingLocationType type)
    {
        return type switch
        {
            MeetingLocationType.InPerson => "In Person",
            MeetingLocationType.PhoneCall => "Phone Call",
            MeetingLocationType.JitsiMeet => "Jitsi Meet",
            MeetingLocationType.Zoom => "Zoom",
            MeetingLocationType.GoogleMeet => "Google Meet",
            MeetingLocationType.MicrosoftTeams => "Microsoft Teams",
            MeetingLocationType.SlackHuddle => "Slack Huddle",
            _ => type.ToString()
        };
    }

    private static string? GetUnavailableReason(bool isInstalled, bool isEnabled, bool isConnected, string pluginName)
    {
        if (!isInstalled)
        {
            return $"Install the {pluginName} plugin to enable this option";
        }

        if (!isEnabled)
        {
            return $"Enable the {pluginName} plugin to use this option";
        }

        if (!isConnected)
        {
            return $"Connect your {pluginName} account to enable this option";
        }

        return null;
    }

    private static void UpdateConnectionStatus(
        ConnectionStatus connections,
        MeetingLocationType type,
        bool isConnected,
        UserInstalledPlugin? installedPlugin)
    {
        var connectionInfo = new ProviderConnectionInfo
        {
            IsConnected = isConnected,
            Email = null, // Could be populated from provider if needed
            ConnectedAt = installedPlugin?.ConnectedAt,
            NeedsReconnect = false
        };

        switch (type)
        {
            case MeetingLocationType.GoogleMeet:
                connections.GoogleMeet = connectionInfo;
                break;
            case MeetingLocationType.MicrosoftTeams:
                connections.MicrosoftTeams = connectionInfo;
                break;
            case MeetingLocationType.Zoom:
                connections.Zoom = connectionInfo;
                break;
            case MeetingLocationType.SlackHuddle:
                connections.SlackHuddle = connectionInfo;
                break;
        }
    }
}

public record AvailableLocationTypesResponse
{
    public required List<LocationTypeInfo> LocationTypes { get; init; }
    public required ConnectionStatus Connections { get; init; }
}

public record LocationTypeInfo
{
    public required MeetingLocationType Type { get; init; }
    public required string Name { get; init; }
    public required bool IsAvailable { get; init; }
    public string? UnavailableReason { get; init; }
    public string? ProviderId { get; init; }
    public required bool RequiresConnection { get; init; }
    public bool RequiresPlugin { get; init; }
    public bool IsPluginInstalled { get; init; }
    public bool IsPluginEnabled { get; init; }
    public string? PluginId { get; init; }
}

public record ConnectionStatus
{
    public ProviderConnectionInfo GoogleMeet { get; set; } = new() { IsConnected = false };
    public ProviderConnectionInfo MicrosoftTeams { get; set; } = new() { IsConnected = false };
    public ProviderConnectionInfo Zoom { get; set; } = new() { IsConnected = false };
    public ProviderConnectionInfo JitsiMeet { get; init; } = new() { IsConnected = true }; // Always available
    public ProviderConnectionInfo SlackHuddle { get; set; } = new() { IsConnected = false };
}

public record ProviderConnectionInfo
{
    public required bool IsConnected { get; init; }
    public string? Email { get; init; }
    public DateTime? ConnectedAt { get; init; }
    public bool NeedsReconnect { get; init; }
}
