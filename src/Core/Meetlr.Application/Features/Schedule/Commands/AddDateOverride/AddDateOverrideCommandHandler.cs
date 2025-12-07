using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Schedule.Commands.AddDateOverride;

public class AddDateOverrideCommandHandler : IRequestHandler<AddDateOverrideCommand, AddDateOverrideCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public AddDateOverrideCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<AddDateOverrideCommandResponse> Handle(AddDateOverrideCommand request, CancellationToken cancellationToken)
    {
        // Verify schedule belongs to user
        var schedule = await _unitOfWork.Repository<AvailabilitySchedule>().GetQueryable()
            .FirstOrDefaultAsync(a => a.Id == request.AvailabilityScheduleId && a.UserId == request.UserId, cancellationToken);

        if (schedule == null)
            throw AvailabilityErrors.NotScheduleOwner("Availability schedule not found or you don't have permission");

        // Check if override already exists for this date
        var existingOverride = await _unitOfWork.Repository<DateSpecificHours>().GetQueryable()
            .FirstOrDefaultAsync(d => d.AvailabilityScheduleId == request.AvailabilityScheduleId && d.Date.Date == request.Date.Date, cancellationToken);

        if (existingOverride != null)
        {
            // Update existing override
            existingOverride.IsAvailable = request.IsAvailable;
            existingOverride.StartTime = request.StartTime;
            existingOverride.EndTime = request.EndTime;
            existingOverride.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AddDateOverrideCommandResponse
            {
                Id = existingOverride.Id,
                Date = existingOverride.Date,
                IsAvailable = existingOverride.IsAvailable,
                Success = true
            };
        }

        // Create new override
        var dateOverride = new DateSpecificHours
        {
            Id = Guid.NewGuid(),
            AvailabilityScheduleId = request.AvailabilityScheduleId,
            Date = request.Date.Date,
            IsAvailable = request.IsAvailable,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Repository<DateSpecificHours>().Add(dateOverride);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogAsync(
            AuditEntityType.DateOverride,
            dateOverride.Id.ToString(),
            AuditAction.Create,
            null,
            dateOverride,
            cancellationToken);

        return new AddDateOverrideCommandResponse
        {
            Id = dateOverride.Id,
            Date = dateOverride.Date,
            IsAvailable = dateOverride.IsAvailable,
            Success = true
        };
    }
}
