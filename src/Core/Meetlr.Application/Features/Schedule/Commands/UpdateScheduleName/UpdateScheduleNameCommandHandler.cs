using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Schedule.Commands.UpdateScheduleName;

public class UpdateScheduleNameCommandHandler : IRequestHandler<UpdateScheduleNameCommand, UpdateScheduleNameResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateScheduleNameCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateScheduleNameResponse> Handle(UpdateScheduleNameCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _unitOfWork.Repository<AvailabilitySchedule>()
            .GetQueryable()
            .FirstOrDefaultAsync(s => s.Id == request.ScheduleId && s.UserId == request.UserId, cancellationToken);

        if (schedule == null)
            throw AvailabilityErrors.AvailabilityScheduleNotFound(request.ScheduleId);

        schedule.Name = request.Name;
        schedule.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<AvailabilitySchedule>().Update(schedule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateScheduleNameResponse
        {
            Id = schedule.Id,
            Name = schedule.Name,
            Success = true
        };
    }
}
