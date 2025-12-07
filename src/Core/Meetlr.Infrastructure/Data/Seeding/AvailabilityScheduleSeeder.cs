using  Meetlr.Domain.Entities.Scheduling;
using  Meetlr.Domain.Entities.Users;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Data.Seeding;

/// <summary>
/// Seeds availability schedule data for testing
/// </summary>
public class AvailabilityScheduleSeeder : ISeeder
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AvailabilityScheduleSeeder> _logger;

    public int Order => 5;

    public AvailabilityScheduleSeeder(
        IUnitOfWork unitOfWork,
        ILogger<AvailabilityScheduleSeeder> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting availability schedule seeding...");

        try
        {
            // Check if availability schedules already exist
            var existingSchedules = await _unitOfWork.Repository<AvailabilitySchedule>()
                .GetQueryable()
                .ToListAsync(cancellationToken);

            if (existingSchedules.Any())
            {
                _logger.LogInformation("Availability schedules already exist ({Count}), skipping seeding",
                    existingSchedules.Count);
                return;
            }

            // Get all users (excluding admin)
            var users = await _unitOfWork.Repository<User>()
                .GetQueryable()
                .Where(u => !u.Email.Equals("admin@calendly.com"))
                .ToListAsync(cancellationToken);

            if (!users.Any())
            {
                _logger.LogWarning("No users found, skipping availability schedule seeding");
                return;
            }

            var scheduleCount = 0;

            foreach (var user in users)
            {
                // Create default Personal availability schedule for each user
                var schedule = new AvailabilitySchedule
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    TenantId = user.TenantId,
                    Name = "Working Hours (Default)",
                    TimeZone = user.TimeZone,
                    IsDefault = true,
                    ScheduleType = ScheduleType.Personal,
                    MaxBookingsPerSlot = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _unitOfWork.Repository<AvailabilitySchedule>().Add(schedule);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Created Personal availability schedule for user: {UserEmail}", user.Email);

                // Create Business availability schedule for first two users
                if (scheduleCount < 2)
                {
                    var businessSchedule = new AvailabilitySchedule
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        TenantId = user.TenantId,
                        Name = "Group Sessions",
                        TimeZone = user.TimeZone,
                        IsDefault = false,
                        ScheduleType = ScheduleType.Business,
                        MaxBookingsPerSlot = 5,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _unitOfWork.Repository<AvailabilitySchedule>().Add(businessSchedule);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Created Business availability schedule for user: {UserEmail}", user.Email);

                    // Create weekly hours for business schedule (Mon-Thu 2pm - 6pm)
                    var businessDays = new[]
                    {
                        DayOfWeekEnum.Monday,
                        DayOfWeekEnum.Tuesday,
                        DayOfWeekEnum.Wednesday,
                        DayOfWeekEnum.Thursday
                    };

                    foreach (var dayOfWeek in businessDays)
                    {
                        var weeklyHour = new WeeklyHours
                        {
                            Id = Guid.NewGuid(),
                            AvailabilityScheduleId = businessSchedule.Id,
                            DayOfWeek = dayOfWeek,
                            IsAvailable = true,
                            StartTime = new TimeSpan(14, 0, 0),  // 2:00 PM
                            EndTime = new TimeSpan(18, 0, 0)     // 6:00 PM
                        };

                        _unitOfWork.Repository<WeeklyHours>().Add(weeklyHour);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                // Create weekly hours for Monday through Friday (9am - 5pm)
                var weekdayHours = new[]
                {
                    DayOfWeekEnum.Monday,
                    DayOfWeekEnum.Tuesday,
                    DayOfWeekEnum.Wednesday,
                    DayOfWeekEnum.Thursday,
                    DayOfWeekEnum.Friday
                };

                foreach (var dayOfWeek in weekdayHours)
                {
                    var weeklyHour = new WeeklyHours
                    {
                        Id = Guid.NewGuid(),
                        AvailabilityScheduleId = schedule.Id,
                        DayOfWeek = dayOfWeek,
                        IsAvailable = true,
                        StartTime = new TimeSpan(9, 0, 0),  // 9:00 AM
                        EndTime = new TimeSpan(17, 0, 0)    // 5:00 PM
                    };

                    _unitOfWork.Repository<WeeklyHours>().Add(weeklyHour);
                }

                // Create weekend hours (marked as unavailable)
                var weekendDays = new[] { DayOfWeekEnum.Saturday, DayOfWeekEnum.Sunday };

                foreach (var dayOfWeek in weekendDays)
                {
                    var weeklyHour = new WeeklyHours
                    {
                        Id = Guid.NewGuid(),
                        AvailabilityScheduleId = schedule.Id,
                        DayOfWeek = dayOfWeek,
                        IsAvailable = false,
                        StartTime = new TimeSpan(0, 0, 0),
                        EndTime = new TimeSpan(0, 0, 0)
                    };

                    _unitOfWork.Repository<WeeklyHours>().Add(weeklyHour);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                scheduleCount++;

                _logger.LogInformation("Created weekly hours (Mon-Fri 9am-5pm) for user: {UserEmail}", user.Email);
            }

            _logger.LogInformation("Successfully seeded {Count} availability schedules with default working hours",
                scheduleCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding availability schedules");
            throw;
        }
    }
}
