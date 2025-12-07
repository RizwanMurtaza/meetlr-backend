using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Schedule.Commands.DeleteDateOverride;

public class DeleteDateOverrideCommandHandler : IRequestHandler<DeleteDateOverrideCommand, DeleteDateOverrideResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDateOverrideCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteDateOverrideResponse> Handle(DeleteDateOverrideCommand request, CancellationToken cancellationToken)
    {
        // Verify the schedule belongs to the user
        var schedule = await _unitOfWork.Repository<AvailabilitySchedule>()
            .GetQueryable()
            .FirstOrDefaultAsync(s => s.Id == request.ScheduleId && s.UserId == request.UserId, cancellationToken);

        if (schedule == null)
            throw AvailabilityErrors.AvailabilityScheduleNotFound(request.ScheduleId);

        // Get the date override
        var dateOverride = await _unitOfWork.Repository<DateSpecificHours>()
            .GetQueryable()
            .FirstOrDefaultAsync(d => d.Id == request.OverrideId && d.AvailabilityScheduleId == request.ScheduleId, cancellationToken);

        if (dateOverride == null)
        {
            return new DeleteDateOverrideResponse
            {
                Success = false,
                Message = "Date override not found"
            };
        }

        _unitOfWork.Repository<DateSpecificHours>().Delete(dateOverride);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteDateOverrideResponse
        {
            Success = true,
            Message = "Date override deleted successfully"
        };
    }
}
