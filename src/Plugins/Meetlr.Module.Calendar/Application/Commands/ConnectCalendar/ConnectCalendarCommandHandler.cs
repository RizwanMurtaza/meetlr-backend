using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Calendar.Application.Specifications;
using Meetlr.Module.Calendar.Domain.Entities;

namespace Meetlr.Module.Calendar.Application.Commands.ConnectCalendar;

public class ConnectCalendarCommandHandler : IRequestHandler<ConnectCalendarCommand, ConnectCalendarResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public ConnectCalendarCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ConnectCalendarResponse> Handle(ConnectCalendarCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if integration already exists for this schedule and provider
            var spec = new CalendarIntegrationSpecifications.ByScheduleAndProvider(request.ScheduleId, request.Provider);
            var existingIntegration = await _unitOfWork.Repository<CalendarIntegration>()
                .FirstOrDefaultAsync(spec, cancellationToken);

            if (existingIntegration != null)
            {
                // Update existing integration
                existingIntegration.AccessToken = request.AccessToken;
                existingIntegration.RefreshToken = request.RefreshToken;
                existingIntegration.TokenExpiresAt = request.TokenExpiresAt;
                existingIntegration.IsActive = true;
                existingIntegration.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(request.ProviderEmail))
                {
                    existingIntegration.ProviderEmail = request.ProviderEmail;
                }

                _unitOfWork.Repository<CalendarIntegration>().Update(existingIntegration);
            }
            else
            {
                // Create new integration linked to the schedule
                var integration = new CalendarIntegration
                {
                    AvailabilityScheduleId = request.ScheduleId,
                    Provider = request.Provider,
                    ProviderEmail = request.ProviderEmail ?? string.Empty,
                    AccessToken = request.AccessToken,
                    RefreshToken = request.RefreshToken,
                    TokenExpiresAt = request.TokenExpiresAt,
                    IsActive = true
                };

                _unitOfWork.Repository<CalendarIntegration>().Add(integration);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ConnectCalendarResponse
            {
                Success = true,
                Message = "Calendar connected successfully"
            };
        }
        catch (Exception ex)
        {
            return new ConnectCalendarResponse
            {
                Success = false,
                Message = $"Failed to connect calendar: {ex.Message}"
            };
        }
    }
}
