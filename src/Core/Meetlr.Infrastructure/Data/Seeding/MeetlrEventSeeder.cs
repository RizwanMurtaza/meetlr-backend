using Bogus;
using  Meetlr.Domain.Entities.Events;
using  Meetlr.Domain.Entities.Users;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Data.Seeding;

/// <summary>
/// Seeds Meetlr event data for testing
/// </summary>
public class MeetlrEventSeeder : ISeeder
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MeetlrEventSeeder> _logger;

    public int Order => 4;

    public MeetlrEventSeeder(
        IUnitOfWork unitOfWork,
        ILogger<MeetlrEventSeeder> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Meetlr event seeding...");

        try
        {
            // Check if Meetlr events already exist
            var existingMeetlrEvents = await _unitOfWork.Repository<MeetlrEvent>()
                .GetQueryable()
                .ToListAsync(cancellationToken);

            if (existingMeetlrEvents.Any())
            {
                _logger.LogInformation("Meetlr events already exist ({Count}), skipping seeding", existingMeetlrEvents.Count);
                return;
            }

            // Get all users (excluding admin)
            var users = await _unitOfWork.Repository<User>()
                .GetQueryable()
                .Where(u => !u.Email.Equals("admin@calendly.com"))
                .ToListAsync(cancellationToken);

            if (!users.Any())
            {
                _logger.LogWarning("No users found, skipping Meetlr event seeding");
                return;
            }

            var faker = new Faker();
            var meetlrEventCount = 0;

            // Common event type configurations
            var meetlrEventTemplates = new[]
            {
                new { Name = "15 Minute Meeting", Duration = 15, Description = "Quick 15-minute consultation or check-in" },
                new { Name = "30 Minute Call", Duration = 30, Description = "Standard 30-minute discussion or consultation" },
                new { Name = "1 Hour Consultation", Duration = 60, Description = "In-depth 1-hour consultation or meeting" },
                new { Name = "Coffee Chat", Duration = 30, Description = "Casual 30-minute coffee chat" },
                new { Name = "Discovery Call", Duration = 45, Description = "45-minute discovery and planning session" },
                new { Name = "Strategy Session", Duration = 90, Description = "Deep-dive 90-minute strategy planning session" }
            };

            foreach (var user in users)
            {
                // Get user's availability schedule
                var availabilitySchedule = await _unitOfWork.Repository< Meetlr.Domain.Entities.Scheduling.AvailabilitySchedule>()
                    .GetQueryable()
                    .Where(a => a.UserId == user.Id && a.IsDefault)
                    .FirstOrDefaultAsync(cancellationToken);

                if (availabilitySchedule == null)
                {
                    _logger.LogWarning("No availability schedule found for user {UserId}, skipping", user.Id);
                    continue;
                }

                // Create 2-3 Meetlr events per user
                var meetlrEventsToCreate = faker.Random.Int(2, 3);
                var selectedTemplates = faker.PickRandom(meetlrEventTemplates, meetlrEventsToCreate).ToList();

                foreach (var template in selectedTemplates)
                {
                    var slug = $"{template.Name.ToLower().Replace(" ", "-")}-{faker.Random.Int(1000, 9999)}";

                    var meetlrEvent = new MeetlrEvent
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        AvailabilityScheduleId = availabilitySchedule.Id,
                        Name = template.Name,
                        Description = template.Description,
                        MeetingLocationType = faker.PickRandom(MeetingLocationType.Zoom, MeetingLocationType.GoogleMeet, MeetingLocationType.MicrosoftTeams, MeetingLocationType.PhoneCall, MeetingLocationType.InPerson),
                        LocationDetails = faker.Address.FullAddress(),
                        DurationMinutes = template.Duration,
                        SlotIntervalMinutes = template.Duration >= 60 ? 60 : (template.Duration >= 30 ? 30 : 15),
                        BufferTimeBeforeMinutes = faker.Random.Int(0, 15),
                        BufferTimeAfterMinutes = faker.Random.Int(0, 15),
                        Color = faker.PickRandom("#0069ff", "#ff6b6b", "#51cf66", "#ffd43b", "#845ef7"),
                        Status = MeetlrEventStatus.Active,
                        Slug = slug,
                        IsActive = true,
                        MeetingType = MeetingType.OneOnOne,
                        MinBookingNoticeMinutes = faker.PickRandom(60, 120, 240, 1440),
                        MaxBookingDaysInFuture = faker.PickRandom(30, 60, 90),
                        MaxBookingsPerDay = faker.Random.Int(3, 10),
                        RequiresPayment = false,
                        SendConfirmationEmail = true,
                        SendReminderEmail = true,
                        ReminderHoursBefore = 24,
                        SendFollowUpEmail = faker.Random.Bool(),
                        FollowUpHoursAfter = 2,
                        CreatedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(7, 30)),
                        UpdatedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(0, 7))
                    };

                    _unitOfWork.Repository<MeetlrEvent>().Add(meetlrEvent);
                    meetlrEventCount++;

                    _logger.LogInformation("Created Meetlr event: {MeetlrEventName} for user: {UserEmail}",
                        meetlrEvent.Name, user.Email);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully seeded {Count} Meetlr events for {UserCount} users",
                meetlrEventCount, users.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding Meetlr events");
            throw;
        }
    }
}
