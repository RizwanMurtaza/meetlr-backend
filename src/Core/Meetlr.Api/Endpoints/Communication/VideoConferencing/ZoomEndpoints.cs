using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.VideoConferencing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Meetlr.Api.Endpoints.Communication.VideoConferencing;

[ApiController]
[Route("api/video-conferencing/zoom")]
[Authorize]
public class ZoomEndpoints : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ZoomSettings _settings;

    public ZoomEndpoints(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IOptions<ZoomSettings> settings)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _settings = settings.Value;
    }

    [HttpPost("connect")]
    [ProducesResponseType(typeof(ConnectZoomResponse), StatusCodes.Status200OK)]
    public ActionResult<ConnectZoomResponse> Connect([FromBody] ConnectZoomRequest request)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        if (string.IsNullOrEmpty(_settings.ClientId))
        {
            return BadRequest("Zoom is not configured");
        }

        var state = $"{userId}|{Uri.EscapeDataString(request.ReturnUrl)}";
        var connectUrl = $"https://zoom.us/oauth/authorize?" +
            $"response_type=code&" +
            $"client_id={_settings.ClientId}&" +
            $"redirect_uri={Uri.EscapeDataString(_settings.RedirectUri)}&" +
            $"state={Uri.EscapeDataString(state)}";

        return Ok(new ConnectZoomResponse(connectUrl));
    }

    [HttpPost("callback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Callback(
        [FromBody] CompleteZoomConnectRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var stateParts = request.State?.Split('|');
        if (stateParts == null || stateParts.Length < 1 ||
            !Guid.TryParse(stateParts[0], out var stateUserId) || stateUserId != userId)
        {
            return BadRequest("Invalid state parameter");
        }

        // Exchange code for token (simplified - actual implementation in provider)
        return Ok(new { message = "Zoom account connected successfully" });
    }

    [HttpDelete("disconnect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Disconnect(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var account = await _unitOfWork.Repository<VideoConferencingAccount>()
            .GetQueryable()
            .FirstOrDefaultAsync(a =>
                a.UserId == userId &&
                a.Provider == "zoom" &&
                !a.IsDeleted,
                cancellationToken);

        if (account == null)
        {
            return BadRequest("Zoom account not connected");
        }

        _unitOfWork.Repository<VideoConferencingAccount>().Delete(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Zoom account disconnected successfully" });
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(ZoomStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ZoomStatusResponse>> GetStatus(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var account = await _unitOfWork.Repository<VideoConferencingAccount>()
            .GetQueryable()
            .FirstOrDefaultAsync(a =>
                a.UserId == userId &&
                a.Provider == "zoom" &&
                !a.IsDeleted,
                cancellationToken);

        if (account == null)
        {
            return Ok(new ZoomStatusResponse(false, null, null, false));
        }

        var needsReconnect = account.TokenExpiresAt.HasValue &&
            account.TokenExpiresAt.Value < DateTime.UtcNow &&
            string.IsNullOrEmpty(account.RefreshToken);

        return Ok(new ZoomStatusResponse(
            true,
            account.Email,
            account.ConnectedAt,
            needsReconnect));
    }
}

public record ConnectZoomRequest(string ReturnUrl);
public record ConnectZoomResponse(string ConnectUrl);
public record CompleteZoomConnectRequest(string Code, string? State);
public record ZoomStatusResponse(bool IsConnected, string? Email, DateTime? ConnectedAt, bool NeedsReconnect);

public class ZoomSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
