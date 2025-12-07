using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Schedule.Commands.SetDefaultSchedule;

public class SetDefaultScheduleCommandHandler : IRequestHandler<SetDefaultScheduleCommand, SetDefaultScheduleCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public SetDefaultScheduleCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<SetDefaultScheduleCommandResponse> Handle(SetDefaultScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _unitOfWork.Repository<AvailabilitySchedule>().GetQueryable()
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.UserId == request.UserId, cancellationToken);

        if (schedule == null)
        {
            throw AvailabilityErrors.ScheduleNotFoundOrNoPermission();
        }

        // Unset other defaults for this user
        var existingDefaults = await _unitOfWork.Repository<AvailabilitySchedule>().GetQueryable()
            .Where(a => a.UserId == request.UserId && a.IsDefault && a.Id != request.Id)
            .ToListAsync(cancellationToken);

        foreach (var defaultSchedule in existingDefaults)
        {
            defaultSchedule.IsDefault = false;
            defaultSchedule.UpdatedAt = DateTime.UtcNow;
        }

        // Set this schedule as default
        schedule.IsDefault = true;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogAsync(
            AuditEntityType.AvailabilitySchedule,
            schedule.Id.ToString(),
            AuditAction.SetDefault,
            new { IsDefault = false },
            new { IsDefault = true },
            cancellationToken);

        return new SetDefaultScheduleCommandResponse
        {
            Success = true
        };
    }
}
