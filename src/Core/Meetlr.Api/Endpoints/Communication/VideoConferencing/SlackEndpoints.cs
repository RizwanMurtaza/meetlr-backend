using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Plugins;
using Meetlr.Application.Plugins.MeetingTypes;
using Meetlr.Domain.Entities.Plugins;
using Meetlr.Domain.Entities.VideoConferencing;
using Meetlr.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Api.Endpoints.Communication.VideoConferencing;

[ApiController]
[Route("api/video-conferencing/slack")]
[Authorize]
public class SlackEndpoints : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPluginFactory _pluginFactory;

    public SlackEndpoints(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPluginFactory pluginFactory)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _pluginFactory = pluginFactory;
    }

    [HttpPost("connect")]
    [ProducesResponseType(typeof(ConnectSlackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConnectSlackResponse>> Connect(
        [FromBody] ConnectSlackRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var slackPlugin = _pluginFactory.GetMeetingTypesPlugin("slackhuddle");

        if (slackPlugin == null || !slackPlugin.IsEnabled)
        {
            return BadRequest("Slack Huddle is not configured");
        }

        // Ensure plugin is installed for user
        await EnsurePluginInstalledAsync(userId, slackPlugin, cancellationToken);

        var connectUrl = await slackPlugin.GenerateConnectUrlAsync(userId, request.ReturnUrl, cancellationToken);

        if (string.IsNullOrEmpty(connectUrl))
        {
            return BadRequest("Failed to generate Slack authorization URL");
        }

        return Ok(new ConnectSlackResponse(connectUrl));
    }

    [HttpPost("callback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Callback(
        [FromBody] CompleteSlackConnectRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Validate state parameter
        var stateParts = request.State?.Split('|');
        if (stateParts == null || stateParts.Length < 1 ||
            !Guid.TryParse(stateParts[0], out var stateUserId) || stateUserId != userId)
        {
            return BadRequest("Invalid state parameter");
        }

        var slackPlugin = _pluginFactory.GetMeetingTypesPlugin("slackhuddle");

        if (slackPlugin == null)
        {
            return BadRequest("Slack Huddle plugin not found");
        }

        var success = await slackPlugin.CompleteConnectAsync(request.Code, request.RedirectUri, userId, cancellationToken);

        if (!success)
        {
            return BadRequest("Failed to connect Slack account");
        }

        // Update plugin installation status
        var installedPlugin = await _unitOfWork.Repository<UserInstalledPlugin>()
            .GetQueryable()
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.PluginId == "slackhuddle" &&
                !p.IsDeleted,
                cancellationToken);

        if (installedPlugin != null)
        {
            installedPlugin.IsConnected = true;
            installedPlugin.ConnectionStatus = "connected";
            installedPlugin.ConnectedAt = DateTime.UtcNow;
            installedPlugin.ErrorMessage = null;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Ok(new { message = "Slack account connected successfully" });
    }

    [HttpDelete("disconnect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Disconnect(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var slackPlugin = _pluginFactory.GetMeetingTypesPlugin("slackhuddle");

        if (slackPlugin == null)
        {
            return BadRequest("Slack Huddle plugin not found");
        }

        var success = await slackPlugin.DisconnectAsync(userId, cancellationToken);

        if (!success)
        {
            return BadRequest("Slack account not connected or failed to disconnect");
        }

        // Update plugin installation status
        var installedPlugin = await _unitOfWork.Repository<UserInstalledPlugin>()
            .GetQueryable()
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.PluginId == "slackhuddle" &&
                !p.IsDeleted,
                cancellationToken);

        if (installedPlugin != null)
        {
            installedPlugin.IsConnected = false;
            installedPlugin.ConnectionStatus = "disconnected";
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Ok(new { message = "Slack account disconnected successfully" });
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(SlackStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SlackStatusResponse>> GetStatus(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var slackPlugin = _pluginFactory.GetMeetingTypesPlugin("slackhuddle");

        if (slackPlugin == null)
        {
            return Ok(new SlackStatusResponse(false, null, null, false));
        }

        var connectionStatus = await slackPlugin.GetConnectionStatusAsync(userId, cancellationToken);

        if (connectionStatus == null)
        {
            return Ok(new SlackStatusResponse(false, null, null, false));
        }

        return Ok(new SlackStatusResponse(
            connectionStatus.IsConnected,
            connectionStatus.Email,
            connectionStatus.ConnectedAt,
            connectionStatus.NeedsReconnect));
    }

    private async Task EnsurePluginInstalledAsync(
        Guid userId,
        IMeetingTypesPlugin plugin,
        CancellationToken cancellationToken)
    {
        var existingInstall = await _unitOfWork.Repository<UserInstalledPlugin>()
            .GetQueryable()
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.PluginId == plugin.PluginId &&
                !p.IsDeleted,
                cancellationToken);

        if (existingInstall == null)
        {
            var newInstall = new UserInstalledPlugin
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PluginCategory = PluginCategory.VideoConferencing,
                PluginId = plugin.PluginId,
                PluginName = plugin.PluginName,
                PluginVersion = plugin.Version,
                IsEnabled = true,
                IsConnected = false,
                ConnectionStatus = "pending",
                InstalledAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<UserInstalledPlugin>().Add(newInstall);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

public record ConnectSlackRequest(string ReturnUrl);
public record ConnectSlackResponse(string ConnectUrl);
public record CompleteSlackConnectRequest(string Code, string? State, string RedirectUri);
public record SlackStatusResponse(bool IsConnected, string? Email, DateTime? ConnectedAt, bool NeedsReconnect);
