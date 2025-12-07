using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Schedule.Commands.UpdateScheduleTimeZone;

public class UpdateScheduleTimeZoneCommandHandler : IRequestHandler<UpdateScheduleTimeZoneCommand, UpdateScheduleTimeZoneResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateScheduleTimeZoneCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateScheduleTimeZoneResponse> Handle(UpdateScheduleTimeZoneCommand request, CancellationToken cancellationToken)
    {
        // Validate timezone
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ArgumentException("Invalid time zone identifier", nameof(request.TimeZone));
        }

        var schedule = await _unitOfWork.Repository<AvailabilitySchedule>()
            .GetQueryable()
            .FirstOrDefaultAsync(s => s.Id == request.ScheduleId && s.UserId == request.UserId && !s.IsDeleted, cancellationToken);

        if (schedule == null)
        {
            return new UpdateScheduleTimeZoneResponse
            {
                Success = false
            };
        }

        schedule.TimeZone = request.TimeZone;
        schedule.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<AvailabilitySchedule>().Update(schedule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateScheduleTimeZoneResponse
        {
            Id = schedule.Id,
            TimeZone = schedule.TimeZone,
            Success = true
        };
    }
}
