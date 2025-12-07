using System.Security.Claims;
using Meetlr.Module.Calendar.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CalendarProvider = Meetlr.Module.Calendar.Domain.Enums.CalendarProvider;
using Enums_CalendarProvider = Meetlr.Module.Calendar.Domain.Enums.CalendarProvider;

namespace Meetlr.Module.Calendar.Endpoints.GetAuthorizationUrl;

[ApiController]
[Route("api/calendar")]
[Authorize]
public class GetAuthorizationUrl : ControllerBase
{
    private readonly IEnumerable<ICalendarProviderService> _calendarProviders;

    public GetAuthorizationUrl(IEnumerable<ICalendarProviderService> calendarProviders)
    {
        _calendarProviders = calendarProviders;
    }

    [HttpGet("auth-url")]
    [ProducesResponseType(typeof(GetAuthorizationUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<IActionResult> Handle([FromQuery] string provider, [FromQuery] string redirectUri, [FromQuery] Guid? scheduleId = null)
    {
        if (!Enum.TryParse<Enums_CalendarProvider>(provider, true, out var calendarProvider))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "Invalid calendar provider" }));
        }

        // Get the current user's ID from claims
        var userIdString = User.FindFirstValue("MeetlrUserId");
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Task.FromResult<IActionResult>(Unauthorized(new { error = "User ID not found in token" }));
        }

        // Get the calendar provider service
        var providerService = _calendarProviders.FirstOrDefault(p => p.Provider == calendarProvider);
        if (providerService == null)
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = $"Calendar provider {calendarProvider} is not supported" }));
        }

        // Encode provider and scheduleId in state parameter (format: "Provider:ScheduleId:RedirectUri")
        // If scheduleId is not provided, use a placeholder that frontend will replace
        var stateScheduleId = scheduleId?.ToString() ?? "SCHEDULE_ID_PLACEHOLDER";
        var state = $"{provider}:{stateScheduleId}:{redirectUri}";
        var authUrl = providerService.GetAuthorizationUrl(redirectUri, state);

        return Task.FromResult<IActionResult>(Ok(new GetAuthorizationUrlResponse
        {
            AuthorizationUrl = authUrl,
            Provider = provider
        }));
    }
}
