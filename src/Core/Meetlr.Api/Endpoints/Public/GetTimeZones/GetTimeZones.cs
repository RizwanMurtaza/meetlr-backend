using Meetlr.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Api.Endpoints.Public.GetTimeZones;

[ApiController]
[Route("api/public/availability")]
[AllowAnonymous]
public class GetTimeZones : ControllerBase
{
    private readonly ITimeZoneService _timeZoneService;

    public GetTimeZones(ITimeZoneService timeZoneService)
    {
        _timeZoneService = timeZoneService;
    }

    /// <summary>
    /// Get all available time zones
    /// </summary>
    [HttpGet("timezones")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GetTimeZonesResponse), StatusCodes.Status200OK)]
    public IActionResult Handle()
    {
        var timeZones = TimeZoneInfo.GetSystemTimeZones()
            .Select(tz => new TimeZoneDto
            {
                Id = tz.Id,
                DisplayName = tz.DisplayName,
                StandardName = tz.StandardName,
                BaseUtcOffset = FormatUtcOffset(tz.BaseUtcOffset)
            })
            .OrderBy(tz => tz.BaseUtcOffset)
            .ThenBy(tz => tz.DisplayName)
            .ToList();

        return Ok(new GetTimeZonesResponse
        {
            TimeZones = timeZones
        });
    }

    private static string FormatUtcOffset(TimeSpan offset)
    {
        var sign = offset >= TimeSpan.Zero ? "+" : "-";
        var absOffset = offset.Duration();
        return $"UTC{sign}{absOffset.Hours:D2}:{absOffset.Minutes:D2}";
    }
}
