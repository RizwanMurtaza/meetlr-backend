using MediatR;
using Meetlr.Api.Endpoints.Plugins.Models;
using Meetlr.Application.Features.Plugins.Commands.ConnectPlugin;
using Meetlr.Application.Features.Plugins.Commands.DisablePlugin;
using Meetlr.Application.Features.Plugins.Commands.DisconnectPlugin;
using Meetlr.Application.Features.Plugins.Commands.EnablePlugin;
using Meetlr.Application.Features.Plugins.Commands.InstallPlugin;
using Meetlr.Application.Features.Plugins.Commands.UninstallPlugin;
using Meetlr.Application.Features.Plugins.Queries.GetAvailablePlugins;
using Meetlr.Application.Features.Plugins.Queries.GetInstalledPlugins;
using Meetlr.Application.Features.Plugins.Queries.GetPluginAuthUrl;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Plugins.Payments;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Plugins;

/// <summary>
/// Plugin marketplace and management endpoints
/// </summary>
[ApiController]
[Route("api/plugins")]
[Authorize]
public class PluginsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly ILogger<PluginsController> _logger;

    public PluginsController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        IPaymentProviderFactory paymentProviderFactory,
        ILogger<PluginsController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _paymentProviderFactory = paymentProviderFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get all available plugins with user's installation status
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PluginInfoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailablePlugins()
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Unauthorized();

        var query = new GetAvailablePluginsQuery
        {
            UserId = userId.Value
        };

        var result = await _mediator.Send(query);

        var response = result.Plugins.Select(p => new PluginInfoResponse
        {
            PluginId = p.PluginId,
            PluginName = p.PluginName,
            Category = p.Category.ToString(),
            Description = p.Description,
            Version = p.Version,
            Author = p.Author,
            IconUrl = p.IconUrl,
            RequiresConnection = p.RequiresConnection,
            IsAvailable = p.IsAvailable,
            IsInstalled = p.IsInstalled,
            IsEnabled = p.IsEnabled,
            IsConnected = p.IsConnected,
            ConnectionStatus = p.ConnectionStatus,
            InstalledAt = p.InstalledAt,
            ConnectedAt = p.ConnectedAt,
            LastUsedAt = p.LastUsedAt,
            UsageCount = p.UsageCount,
            HealthStatus = p.HealthStatus,
            HealthMessage = p.HealthMessage
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Get user's installed plugins
    /// </summary>
    [HttpGet("installed")]
    [ProducesResponseType(typeof(List<InstalledPluginResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInstalledPlugins()
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Unauthorized();

        var query = new GetInstalledPluginsQuery
        {
            UserId = userId.Value
        };

        var result = await _mediator.Send(query);

        var response = result.Plugins.Select(p => new InstalledPluginResponse
        {
            Id = p.Id,
            PluginId = p.PluginId,
            PluginName = p.PluginName,
            Category = p.Category.ToString(),
            Version = p.Version,
            IsEnabled = p.IsEnabled,
            IsConnected = p.IsConnected,
            ConnectionStatus = p.ConnectionStatus,
            InstalledAt = p.InstalledAt,
            ConnectedAt = p.ConnectedAt,
            LastUsedAt = p.LastUsedAt,
            UsageCount = p.UsageCount,
            ErrorMessage = p.ErrorMessage
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Install a plugin for the current user
    /// </summary>
    [HttpPost("install")]
    [ProducesResponseType(typeof(Models.InstallPluginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InstallPlugin([FromBody] InstallPluginRequest request)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Unauthorized();

        var command = new InstallPluginCommand
        {
            UserId = userId.Value,
            PluginId = request.PluginId,
            Settings = request.Settings
        };

        var result = await _mediator.Send(command);

        _logger.LogInformation("User {UserId} installed plugin {PluginId}", userId, request.PluginId);

        return Ok(new Models.InstallPluginResponse
        {
            Success = result.Success,
            Message = result.Message,
            PluginId = request.PluginId,
            RequiresConnection = result.RequiresConnection
        });
    }

    /// <summary>
    /// Enable a plugin
    /// </summary>
    [HttpPost("{pluginId}/enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnablePlugin([FromRoute] string pluginId)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Unauthorized();

        var command = new EnablePluginCommand
        {
            UserId = userId.Value,
            PluginId = pluginId
        };

        var result = await _mediator.Send(command);

        _logger.LogInformation("User {UserId} enabled plugin {PluginId}", userId, pluginId);

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Disable a plugin
    /// </summary>
    [HttpPost("{pluginId}/disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisablePlugin([FromRoute] string pluginId)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Unauthorized();

        var command = new DisablePluginCommand
        {
            UserId = userId.Value,
            PluginId = pluginId
        };

        var result = await _mediator.Send(command);

        _logger.LogInformation("User {UserId} disabled plugin {PluginId}", userId, pluginId);

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Uninstall a plugin
    /// </summary>
    [HttpDelete("{pluginId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UninstallPlugin([FromRoute] string pluginId)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Unauthorized();

        var command = new UninstallPluginCommand
        {
            UserId = userId.Value,
            PluginId = pluginId
        };

        var result = await _mediator.Send(command);

        _logger.LogInformation("User {UserId} uninstalled plugin {PluginId}", userId, pluginId);

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Get plugin health status
    /// </summary>
    [HttpGet("{pluginId}/health")]
    [ProducesResponseType(typeof(PluginHealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPluginHealth([FromRoute] string pluginId)
    {
        var plugin = _paymentProviderFactory.GetProvider(pluginId);
        if (plugin == null)
            throw PluginErrors.PluginNotFound(pluginId);

        var health = await plugin.GetHealthStatusAsync();

        return Ok(new PluginHealthResponse
        {
            PluginId = plugin.PluginId,
            PluginName = plugin.PluginName,
            IsHealthy = health.IsHealthy,
            Status = health.Status,
            Message = health.Message,
            LastChecked = DateTime.UtcNow,
            Details = health.Details?.ToDictionary(k => k.Key, k => (object)k.Value)
        });
    }

    /// <summary>
    /// Get OAuth authorization URL for a plugin that requires OAuth connection
    /// </summary>
    [HttpGet("{pluginId}/auth-url")]
    [ProducesResponseType(typeof(PluginAuthUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPluginAuthUrl([FromRoute] string pluginId, [FromQuery] string redirectUri)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Unauthorized();

        var query = new GetPluginAuthUrlQuery
        {
            UserId = userId.Value,
            PluginId = pluginId,
            RedirectUri = redirectUri
        };

        var result = await _mediator.Send(query);

        return Ok(new PluginAuthUrlResponse
        {
            AuthorizationUrl = result.AuthorizationUrl,
            PluginId = pluginId
        });
    }

    /// <summary>
    /// Complete OAuth connection for a plugin (callback endpoint)
    /// </summary>
    [HttpPost("{pluginId}/connect")]
    [ProducesResponseType(typeof(PluginConnectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConnectPlugin([FromRoute] string pluginId, [FromBody] PluginConnectRequest request)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Unauthorized();

        var command = new ConnectPluginCommand
        {
            UserId = userId.Value,
            PluginId = pluginId,
            Code = request.Code,
            RedirectUri = request.RedirectUri
        };

        var result = await _mediator.Send(command);

        _logger.LogInformation("User {UserId} connected plugin {PluginId}", userId, pluginId);

        return Ok(new PluginConnectResponse
        {
            Success = result.Success,
            Message = result.Message,
            PluginId = pluginId,
            Email = result.Email
        });
    }

    /// <summary>
    /// Disconnect OAuth connection for a plugin
    /// </summary>
    [HttpPost("{pluginId}/disconnect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisconnectPlugin([FromRoute] string pluginId)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Unauthorized();

        var command = new DisconnectPluginCommand
        {
            UserId = userId.Value,
            PluginId = pluginId
        };

        var result = await _mediator.Send(command);

        _logger.LogInformation("User {UserId} disconnected plugin {PluginId}", userId, pluginId);

        return Ok(new { message = result.Message });
    }
}
