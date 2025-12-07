using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Module.Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Calendar.Application.Commands.DisconnectCalendar;

public class DisconnectCalendarCommandHandler : IRequestHandler<DisconnectCalendarCommand, DisconnectCalendarResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public DisconnectCalendarCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DisconnectCalendarResponse> Handle(DisconnectCalendarCommand request, CancellationToken cancellationToken)
    {
        // Find the integration
        var integration = await _unitOfWork.Repository<CalendarIntegration>()
            .GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == request.CalendarIntegrationId, cancellationToken);

        if (integration == null)
        {
            return new DisconnectCalendarResponse
            {
                Success = false,
                Message = "Calendar integration not found"
            };
        }

        // Verify the user owns the schedule this calendar is linked to
        var schedule = await _unitOfWork.Repository<AvailabilitySchedule>()
            .GetQueryable()
            .FirstOrDefaultAsync(s => s.Id == integration.AvailabilityScheduleId && s.UserId == request.UserId, cancellationToken);

        if (schedule == null)
        {
            return new DisconnectCalendarResponse
            {
                Success = false,
                Message = "You don't have permission to disconnect this calendar"
            };
        }

        integration.IsActive = false;
        integration.IsDeleted = true;
        integration.DeletedAt = DateTime.UtcNow;
        integration.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<CalendarIntegration>().Update(integration);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DisconnectCalendarResponse
        {
            Success = true,
            Message = "Calendar disconnected successfully"
        };
    }
}
