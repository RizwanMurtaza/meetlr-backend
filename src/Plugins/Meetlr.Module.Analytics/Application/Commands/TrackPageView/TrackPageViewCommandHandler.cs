using System.Security.Cryptography;
using System.Text;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Module.Analytics.Domain.Entities;
using Meetlr.Module.Analytics.Domain.Enums;

namespace Meetlr.Module.Analytics.Application.Commands.TrackPageView;

/// <summary>
/// Handler for tracking page view events
/// Uses the main IUnitOfWork to access the shared ApplicationDbContext
/// </summary>
public class TrackPageViewCommandHandler : IRequestHandler<TrackPageViewCommand, TrackPageViewCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantService  _tenantService;

    public TrackPageViewCommandHandler(IUnitOfWork unitOfWork, ITenantService tenantService)
    {
        _unitOfWork = unitOfWork;
        _tenantService = tenantService;
    }

    public async Task<TrackPageViewCommandResponse> Handle(TrackPageViewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var pageType = ParsePageType(request.PageType);
            var deviceType = DetectDeviceType(request.UserAgent);
            var ipHash = HashIpAddress(request.IpAddress);

            var pageView = new PageView
            {
                Id = Guid.NewGuid(),
                PageType = pageType,
                Username = request.Username,
                EventSlug = request.EventSlug,
                PagePath = BuildPagePath(pageType, request.Username, request.EventSlug),
                SessionId = request.SessionId,
                UserAgent = TruncateString(request.UserAgent, 500),
                ReferrerUrl = TruncateString(request.Referrer, 1000),
                DeviceType = deviceType,
                TenantId = _tenantService.TenantId.HasValue ? _tenantService.TenantId.Value : Guid.Empty,
                IpAddressHash = ipHash,
                ViewedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<PageView>().Add(pageView);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TrackPageViewCommandResponse
            {
                Success = true,
                PageViewId = pageView.Id
            };
        }
        catch
        {
            // Silently fail - analytics should never break the user experience
            return new TrackPageViewCommandResponse
            {
                Success = false
            };
        }
    }

    private static PageViewType ParsePageType(string pageType)
    {
        return pageType.ToLowerInvariant() switch
        {
            "homepage" => PageViewType.UserHomepage,
            "eventlist" => PageViewType.EventList,
            "eventpage" => PageViewType.EventPage,
            _ => PageViewType.UserHomepage
        };
    }

    private static string BuildPagePath(PageViewType pageType, string username, string? eventSlug)
    {
        return pageType switch
        {
            PageViewType.UserHomepage => $"/site/{username}",
            PageViewType.EventList => $"/book/{username}",
            PageViewType.EventPage => $"/book/{username}/{eventSlug}",
            _ => $"/site/{username}"
        };
    }

    private static string? DetectDeviceType(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return null;

        var ua = userAgent.ToLowerInvariant();

        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
            return "Mobile";

        if (ua.Contains("tablet") || ua.Contains("ipad"))
            return "Tablet";

        return "Desktop";
    }

    private static string? HashIpAddress(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return null;

        // Hash IP for privacy - we only need it for approximate uniqueness
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(ipAddress));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string? TruncateString(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
