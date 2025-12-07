using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Contacts;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Data.Seeding;

/// <summary>
/// Seeds booking data for testing
/// </summary>
public class BookingSeeder : ISeeder
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookingSeeder> _logger;

    public int Order => 6;

    public BookingSeeder(
        IUnitOfWork unitOfWork,
        ILogger<BookingSeeder> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting booking seeding...");

        try
        {
            // Check if bookings already exist
            var existingBookings = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .ToListAsync(cancellationToken);

            if (existingBookings.Any())
            {
                _logger.LogInformation("Bookings already exist ({Count}), skipping seeding",
                    existingBookings.Count);
                return;
            }

            // Get event types
            var meetlrEvents = await _unitOfWork.Repository<MeetlrEvent>()
                .GetQueryable()
                .Where(et => et.Status == MeetlrEventStatus.Active && !et.IsDeleted)
                .Take(5)
                .ToListAsync(cancellationToken);

            if (!meetlrEvents.Any())
            {
                _logger.LogWarning("No event types found, skipping booking seeding");
                return;
            }

            var bookingCount = 0;
            var random = new Random(42); // Fixed seed for reproducible test data

            var sampleNames = new[]
            {
                "John Smith", "Emma Wilson", "Michael Brown", "Sarah Davis", "James Johnson",
                "Emily Martinez", "Robert Garcia", "Jessica Rodriguez", "William Anderson", "Lisa Thomas"
            };

            var sampleEmails = new[]
            {
                "john.smith@example.com", "emma.wilson@example.com", "michael.brown@example.com",
                "sarah.davis@example.com", "james.johnson@example.com", "emily.martinez@example.com",
                "robert.garcia@example.com", "jessica.rodriguez@example.com", "william.anderson@example.com",
                "lisa.thomas@example.com"
            };

            var samplePhones = new[]
            {
                "+12025551234", "+12025552345", "+12025553456",
                "+12025554567", "+12025555678", "+12025556789",
                "+12025557890", "+12025558901", "+12025559012",
                "+12025550123"
            };

            var locations = new[]
            {
                "Zoom Meeting", "Google Meet", "Microsoft Teams", "Phone Call", "In-Person Meeting"
            };

            // Create contacts for sample invitees (one per tenant+email, unique constraint is on TenantId+Email)
            var contactsByTenantAndEmail = new Dictionary<(Guid TenantId, string Email), Contact>();

            foreach (var meetlrEvent in meetlrEvents)
            {
                for (int i = 0; i < sampleNames.Length; i++)
                {
                    var key = (meetlrEvent.TenantId, sampleEmails[i]);
                    if (!contactsByTenantAndEmail.ContainsKey(key))
                    {
                        var contact = new Contact
                        {
                            Id = Guid.NewGuid(),
                            TenantId = meetlrEvent.TenantId,
                            UserId = meetlrEvent.UserId,
                            Name = sampleNames[i],
                            Email = sampleEmails[i],
                            Phone = samplePhones[i],
                            TimeZone = "America/New_York",
                            Source = ContactSource.Booking,
                            CreatedAt = DateTime.UtcNow.AddDays(-30),
                            UpdatedAt = DateTime.UtcNow.AddDays(-30)
                        };
                        _unitOfWork.Repository<Contact>().Add(contact);
                        contactsByTenantAndEmail[key] = contact;
                    }
                }
            }

            // Save contacts first
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created {Count} contacts for bookings", contactsByTenantAndEmail.Count);

            // Create bookings for each event type
            foreach (var meetlrEvent in meetlrEvents)
            {
                var bookingsForEvent = random.Next(2, 6); // 2-5 bookings per event

                for (int i = 0; i < bookingsForEvent; i++)
                {
                    var nameIndex = random.Next(sampleNames.Length);
                    var daysOffset = random.Next(-7, 14); // Past 7 days or future 14 days
                    var startHour = random.Next(9, 16); // Between 9 AM and 4 PM
                    var startMinute = random.Next(0, 4) * 15; // 0, 15, 30, or 45 minutes

                    var startTime = DateTime.UtcNow.Date
                        .AddDays(daysOffset)
                        .AddHours(startHour)
                        .AddMinutes(startMinute);

                    var endTime = startTime.AddMinutes(meetlrEvent.DurationMinutes);

                    // Determine status based on time
                    BookingStatus status;
                    if (daysOffset < -1)
                        status = BookingStatus.Completed;
                    else if (daysOffset < 0)
                        status = random.Next(0, 10) < 8 ? BookingStatus.Completed : BookingStatus.Cancelled;
                    else if (daysOffset == 0 && startHour < DateTime.UtcNow.Hour)
                        status = BookingStatus.Completed;
                    else
                        status = BookingStatus.Confirmed;

                    // Get or create contact for this invitee
                    var contactKey = (meetlrEvent.TenantId, sampleEmails[nameIndex]);
                    var contact = contactsByTenantAndEmail[contactKey];

                    var booking = new Booking
                    {
                        Id = Guid.NewGuid(),
                        TenantId = meetlrEvent.TenantId,
                        MeetlrEventId = meetlrEvent.Id,
                        HostUserId = meetlrEvent.UserId,
                        ContactId = contact.Id,
                        StartTime = startTime,
                        EndTime = endTime,
                        Status = status,
                        Location = locations[random.Next(locations.Length)],
                        MeetingLink = locations[random.Next(locations.Length)].Contains("Zoom")
                            ? $"https://zoom.us/j/{random.Next(100000000, 999999999)}"
                            : null,
                        Notes = random.Next(0, 3) == 0 ? "Looking forward to our meeting!" : null,
                        PaymentStatus = meetlrEvent.RequiresPayment
                            ? (status == BookingStatus.Confirmed ? PaymentStatus.Completed : PaymentStatus.NotRequired)
                            : PaymentStatus.NotRequired,
                        Amount = meetlrEvent.RequiresPayment ? meetlrEvent.Fee : null,
                        Currency = meetlrEvent.RequiresPayment ? meetlrEvent.Currency : null,
                        PaidAt = meetlrEvent.RequiresPayment && status == BookingStatus.Confirmed
                            ? startTime.AddMinutes(-random.Next(10, 60))
                            : null,
                        ConfirmationToken = Guid.NewGuid().ToString(),
                        CancellationToken = Guid.NewGuid().ToString(),
                        ReminderSent = status == BookingStatus.Completed || (status == BookingStatus.Confirmed && daysOffset <= 1),
                        ReminderSentAt = status == BookingStatus.Completed
                            ? startTime.AddHours(-24)
                            : (status == BookingStatus.Confirmed && daysOffset <= 1 ? DateTime.UtcNow.AddHours(-2) : null),
                        FollowUpSent = status == BookingStatus.Completed,
                        FollowUpSentAt = status == BookingStatus.Completed ? endTime.AddHours(2) : null,
                        CancellationReason = status == BookingStatus.Cancelled ? "Schedule conflict" : null,
                        CancelledAt = status == BookingStatus.Cancelled ? startTime.AddDays(-1) : null,
                        CreatedAt = startTime.AddDays(-7),
                        UpdatedAt = status == BookingStatus.Cancelled
                            ? startTime.AddDays(-1)
                            : startTime.AddDays(-7)
                    };

                    _unitOfWork.Repository<Booking>().Add(booking);
                    bookingCount++;

                    _logger.LogInformation(
                        "Created {Status} booking for {InviteeName} with event type: {EventTypeName} at {StartTime}",
                        status, contact.Name, meetlrEvent.Name, startTime);
                }
            }

            // Create multiple bookings for same time slot (for business schedule testing)
            var businessEvent = meetlrEvents.FirstOrDefault();
            if (businessEvent != null)
            {
                var groupStartTime = DateTime.UtcNow.Date.AddDays(3).AddHours(14); // 3 days from now at 2 PM
                var groupEndTime = groupStartTime.AddMinutes(businessEvent.DurationMinutes);

                for (int i = 0; i < 3; i++)
                {
                    var nameIndex = i;
                    var contactKey = (businessEvent.TenantId, sampleEmails[nameIndex]);
                    var contact = contactsByTenantAndEmail[contactKey];

                    var booking = new Booking
                    {
                        Id = Guid.NewGuid(),
                        TenantId = businessEvent.TenantId,
                        MeetlrEventId = businessEvent.Id,
                        HostUserId = businessEvent.UserId,
                        ContactId = contact.Id,
                        StartTime = groupStartTime,
                        EndTime = groupEndTime,
                        Status = BookingStatus.Confirmed,
                        Location = "Group Session - Zoom",
                        MeetingLink = $"https://zoom.us/j/{random.Next(100000000, 999999999)}",
                        PaymentStatus = PaymentStatus.NotRequired,
                        ConfirmationToken = Guid.NewGuid().ToString(),
                        CancellationToken = Guid.NewGuid().ToString(),
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        UpdatedAt = DateTime.UtcNow.AddDays(-2)
                    };

                    _unitOfWork.Repository<Booking>().Add(booking);
                    bookingCount++;

                    _logger.LogInformation(
                        "Created group booking {Index} for {InviteeName} at {StartTime}",
                        i + 1, contact.Name, groupStartTime);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully seeded {Count} bookings", bookingCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding bookings");
            throw;
        }
    }
}
