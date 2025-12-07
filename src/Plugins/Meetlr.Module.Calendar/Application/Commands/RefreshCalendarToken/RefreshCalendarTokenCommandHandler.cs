using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Calendar.Application.Interfaces;
using Meetlr.Module.Calendar.Domain.Entities;
using Meetlr.Module.Calendar.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Calendar.Application.Commands.RefreshCalendarToken;

public class RefreshCalendarTokenCommandHandler : IRequestHandler<RefreshCalendarTokenCommand, RefreshCalendarTokenResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<ICalendarProviderService> _calendarProviders;

    public RefreshCalendarTokenCommandHandler(
        IUnitOfWork unitOfWork,
        IEnumerable<ICalendarProviderService> calendarProviders)
    {
        _unitOfWork = unitOfWork;
        _calendarProviders = calendarProviders;
    }

    public async Task<RefreshCalendarTokenResponse> Handle(
        RefreshCalendarTokenCommand request,
        CancellationToken cancellationToken)
    {
        var integration = await _unitOfWork.Repository<CalendarIntegration>()
            .GetQueryable()
            .FirstOrDefaultAsync(ci => ci.Id == request.IntegrationId, cancellationToken);

        if (integration == null)
            throw CalendarErrors.CalendarIntegrationNotFound(request.IntegrationId);

        if (string.IsNullOrEmpty(integration.RefreshToken))
            throw CalendarErrors.RefreshTokenNotAvailable();

        var provider = _calendarProviders.FirstOrDefault(p => p.Provider == integration.Provider);
        if (provider == null)
            throw CalendarErrors.CalendarProviderNotFound(integration.Provider.ToString());

        var refreshResult = await provider.RefreshAccessTokenAsync(
            integration.RefreshToken,
            cancellationToken);

        // Update integration with new token
        integration.AccessToken = refreshResult.AccessToken;
        if (!string.IsNullOrEmpty(refreshResult.RefreshToken))
        {
            integration.RefreshToken = refreshResult.RefreshToken;
        }
        integration.TokenExpiresAt = refreshResult.ExpiresAt;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RefreshCalendarTokenResponse
        {
            Success = true,
            AccessToken = refreshResult.AccessToken,
            ExpiresAt = refreshResult.ExpiresAt
        };
    }
}
