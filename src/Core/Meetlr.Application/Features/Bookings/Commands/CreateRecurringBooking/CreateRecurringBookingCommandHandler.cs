using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Common.Settings;
using Meetlr.Application.Common.Specifications;
using Meetlr.Application.Features.Availability.Queries.ValidateBookingSlots;
using Meetlr.Application.Features.Bookings.Commands.CreateBooking;
using Meetlr.Application.Features.Contacts.Queries.GetOrCreateContact;
using Meetlr.Application.Features.Payments.Commands.CreateSeriesPayment;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Interfaces.Models;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Meetlr.Application.Features.Bookings.Commands.CreateRecurringBooking;

/// <summary>
/// Handler for creating recurring bookings - pure CQRS orchestrator (follows SRP and CQRS)
/// </summary>
public class CreateRecurringBookingCommandHandler : IRequestHandler<CreateRecurringBookingCommand, CreateRecurringBookingCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationQueueService _notificationQueueService;
    private readonly IAuditService _auditService;
    private readonly IMediator _mediator;
    private readonly RecurringBookingsSettings _settings;
    private readonly ICalendarService? _calendarService;
    private readonly ILogger<CreateRecurringBookingCommandHandler> _logger;
    private readonly ITenantService _tenantService;

    public CreateRecurringBookingCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationQueueService notificationQueueService,
        IAuditService auditService,
        IMediator mediator,
        IOptions<RecurringBookingsSettings> settings,
        ILogger<CreateRecurringBookingCommandHandler> logger,
        ITenantService tenantService,
        ICalendarService? calendarService = null)
    {
        _unitOfWork = unitOfWork;
        _notificationQueueService = notificationQueueService;
        _auditService = auditService;
        _mediator = mediator;
        _settings = settings.Value;
        _calendarService = calendarService;
        _logger = logger;
        _tenantService = tenantService;
    }

    public async Task<CreateRecurringBookingCommandResponse> Handle(
        CreateRecurringBookingCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get event type with user info and Stripe account
        var spec = new MeetlrEventSpecifications.ByIdWithUserAndStripeAccount(request.MeetlrEventId);
        var eventType = await _unitOfWork.Repository<MeetlrEvent>()
            .FirstOrDefaultAsync(spec, cancellationToken);

        if (eventType == null)
            throw MeetlrEventErrors.MeetlrEventNotFound(request.MeetlrEventId);

        if (eventType.Status != MeetlrEventStatus.Active)
            throw MeetlrEventErrors.MeetlrEventNotActive(request.MeetlrEventId);

        // 1b. Validate single-use booking link token if provided
        SingleUseBookingLink? singleUseLink = null;
        if (!string.IsNullOrEmpty(request.Token))
        {
            singleUseLink = await _unitOfWork.Repository<SingleUseBookingLink>()
                .GetQueryable()
                .FirstOrDefaultAsync(l => l.Token == request.Token && l.MeetlrEventId == request.MeetlrEventId, cancellationToken);

            if (singleUseLink == null)
                throw SingleUseLinkErrors.LinkNotFoundByToken(request.Token);

            if (!singleUseLink.IsActive)
                throw SingleUseLinkErrors.LinkInactive(request.Token);

            if (singleUseLink.IsUsed)
                throw SingleUseLinkErrors.LinkAlreadyUsed(request.Token);

            if (singleUseLink.ExpiresAt.HasValue && singleUseLink.ExpiresAt.Value < DateTime.UtcNow)
                throw SingleUseLinkErrors.LinkExpired(request.Token);
        }

        // 2. Validate selected date times
        if (request.SelectedDateTimes == null || request.SelectedDateTimes.Count == 0)
        {
            throw BookingErrors.NoOccurrencesProvided();
        }

        if (request.SelectedDateTimes.Count > _settings.MaxOccurrences)
        {
            throw BookingErrors.TooManyOccurrences(_settings.MaxOccurrences, request.SelectedDateTimes.Count);
        }

        // Sort dates to ensure chronological order
        var occurrenceDates = request.SelectedDateTimes.OrderBy(d => d).ToList();

        // 3. Get or create contact for this invitee
        var contact = await _mediator.Send(new GetOrCreateContactQuery
        {
            Email = request.InviteeEmail,
            Name = request.InviteeName,
            Phone = request.InviteePhone,
            TimeZone = request.InviteeTimeZone ?? eventType.User.TimeZone,
            UserId = eventType.UserId,
            Source = ContactSource.Booking
        }, cancellationToken);

        // 4. Create minimal series entity (lightweight placeholder for grouping)
        var series = new BookingSeries
        {
            TenantId = _tenantService.TenantId ?? throw BookingErrors.TenantIdRequired(),
            BaseMeetlrEventId = request.MeetlrEventId,
            HostUserId = eventType.UserId,
            ContactId = contact.Id, // Link to contact entity - all invitee info comes from Contact
            TotalOccurrences = occurrenceDates.Count,
            PaymentType = request.PaymentType,
            Status = SeriesStatus.Active
        };

        // 5. Validate all occurrence slots using unified query (handles OneOnOne vs Group + capacity)
        var validateQuery = new ValidateBookingSlotsQuery
        {
            MeetlrEventId = request.MeetlrEventId,
            RequestedSlots = occurrenceDates,
            TimeZone = request.InviteeTimeZone ?? eventType.User.TimeZone
        };
        var validation = await _mediator.Send(validateQuery, cancellationToken);

        // 6. If conflicts exist, return them with alternatives
        if (validation.HasConflicts)
        {
            // Map unified response to recurring booking response format
            var conflictingOccurrences = validation.Conflicts.Select(c => new ConflictingOccurrence
            {
                OccurrenceNumber = c.SlotIndex + 1,
                RequestedDate = c.RequestedTime.Date,
                RequestedTime = c.RequestedTime,
                SuggestedSlots = c.SuggestedAlternatives.Select(a => new AlternativeSlot
                {
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    DisplayTime = a.DisplayTime
                }).ToList()
            }).ToList();

            return new CreateRecurringBookingCommandResponse
            {
                HasConflicts = true,
                ConflictingOccurrences = conflictingOccurrences,
                Message = validation.Message ?? "Some time slots are unavailable. Please review alternatives."
            };
        }

        // 7. Create all bookings inline (TRANSACTIONAL - all or nothing)
        var bookings = new List<Booking>();

        foreach (var date in occurrenceDates)
        {
            // Calculate end time based on event duration
            var startTimeUtc = date;
            var endTimeUtc = startTimeUtc.AddMinutes(eventType.DurationMinutes);

            // Create booking entity
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantService.TenantId ?? throw BookingErrors.TenantIdRequired(),
                MeetlrEventId = eventType.Id,
                HostUserId = eventType.UserId,
                SeriesBookingId = series.Id, // Link to series
                ContactId = contact.Id, // Link to contact entity - all invitee info comes from Contact
                StartTime = startTimeUtc,
                EndTime = endTimeUtc,
                Status = eventType.RequiresPayment ? BookingStatus.Pending : BookingStatus.Confirmed,
                Location = eventType.LocationDetails,
                Notes = request.Notes,
                PaymentStatus = eventType.RequiresPayment ? PaymentStatus.Pending : PaymentStatus.NotRequired,
                Amount = eventType.Fee,
                Currency = eventType.Currency,
                PaymentProviderType = eventType.RequiresPayment ? eventType.PaymentProviderType : null,
                ConfirmationToken = Guid.NewGuid().ToString(),
                CancellationToken = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                OccurrenceIndex = bookings.Count + 1 // 1-based index (1st, 2nd, 3rd, etc.)
            };

            bookings.Add(booking);
            _unitOfWork.Repository<Booking>().Add(booking);
        }

        // Update series with actual occurrence count
        series.OccurrenceCount = bookings.Count;

        // Add series to repository
        _unitOfWork.Repository<BookingSeries>().Add(series);

        // SINGLE ATOMIC TRANSACTION: Save series + all bookings together
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7b. Mark single-use link as used if applicable (link to first booking in series)
        if (singleUseLink != null && bookings.Any())
        {
            singleUseLink.IsUsed = true;
            singleUseLink.UsedAt = DateTime.UtcNow;
            singleUseLink.BookingId = bookings.First().Id;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Created booking series {SeriesId} with {Count} bookings in single transaction",
            series.Id,
            bookings.Count);

        // 8. Create calendar events (SEPARATE TRANSACTION, ONLY for free bookings)
        // For paid bookings, calendar events will be created after payment webhook
        if (!eventType.RequiresPayment && _calendarService != null)
        {
            try
            {
                var calendarRequests = bookings.Select(b => new CalendarServiceEventRequest
                {
                    BookingId = b.Id,
                    Summary = $"{eventType.Name} with {contact.Name}",
                    Description = $"Recurring booking\n\nInvitee: {contact.Name}\nEmail: {contact.Email}",
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    TimeZone = eventType.User?.TimeZone ?? "UTC",
                    AttendeeEmails = new List<string> { contact.Email },
                    Location = b.Location,
                    MeetingLink = null
                }).ToList();

                var calendarResult = await _calendarService.CreateSeriesEventsAsync(
                    eventType.UserId,
                    calendarRequests,
                    cancellationToken);

                // Update bookings with calendar event IDs
                if (calendarResult.Success && calendarResult.Results.Any())
                {
                    for (int i = 0; i < calendarResult.Results.Count && i < bookings.Count; i++)
                    {
                        var result = calendarResult.Results[i];
                        if (result.Success && !string.IsNullOrEmpty(result.EventId))
                        {
                            bookings[i].CalendarEventId = result.EventId;
                        }
                    }
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create calendar events for series {SeriesId}", series.Id);
                // Don't fail the entire series creation if calendar events fail
            }
        }

        // 9. Handle series-level payment (SEPARATE TRANSACTION)
        string? paymentClientSecret = null;
        string? subscriptionId = null;
        decimal totalAmount = 0;

        if (eventType.RequiresPayment && eventType.Fee.HasValue && eventType.Fee.Value > 0)
        {
            string? connectedAccountId = null;
            if (eventType.PaymentProviderType == PaymentProviderType.Stripe && eventType.User.StripeAccount != null)
            {
                connectedAccountId = eventType.User.StripeAccount.StripeAccountId;
            }

            var createPaymentCommand = new CreateSeriesPaymentCommand
            {
                SeriesId = series.Id,
                MeetlrEventId = eventType.Id,
                BookingIds = bookings.Select(b => b.Id).ToList(),
                PaymentType = request.PaymentType,
                FeePerOccurrence = eventType.Fee.Value,
                Currency = eventType.Currency ?? "USD",
                PaymentProviderType = eventType.PaymentProviderType?.ToString() ?? "unknown",
                ConnectedAccountId = connectedAccountId,
                InviteeEmail = request.InviteeEmail,
                meetlrEventName = eventType.Name
            };

            var paymentResult = await _mediator.Send(createPaymentCommand, cancellationToken);

            if (paymentResult.Success)
            {
                paymentClientSecret = paymentResult.ClientSecret;
                subscriptionId = paymentResult.SubscriptionId;
                totalAmount = paymentResult.TotalAmount;
            }
            else
            {
                _logger.LogWarning("Failed to create series payment: {Error}", paymentResult.ErrorMessage);
            }
        }

        // 10. Queue series notifications (ONLY for free bookings)
        // For paid bookings, notifications will be sent after payment webhook
        if (!eventType.RequiresPayment)
        {
            try
            {
                await _notificationQueueService.QueueSeriesNotificationsAsync(
                    series,
                    bookings,
                    eventType,
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue notifications for series {SeriesId}", series.Id);
                // Don't fail the entire series creation if notification queueing fails
            }
        }

        // 11. Log audit
        await _auditService.LogAsync(
            AuditEntityType.BookingSeries,
            series.Id.ToString(),
            AuditAction.Create,
            null,
            series,
            cancellationToken
        );

        // 12. Return response
        return new CreateRecurringBookingCommandResponse
        {
            HasConflicts = false,
            SeriesId = series.Id,
            TotalOccurrences = bookings.Count,
            Message = "Recurring booking series created successfully",
            RequiresPayment = eventType.RequiresPayment && eventType.Fee.HasValue && eventType.Fee.Value > 0,
            TotalAmount = totalAmount > 0 ? totalAmount : null,
            Currency = eventType.Currency,
            PaymentClientSecret = paymentClientSecret,
            SubscriptionId = subscriptionId
        };
    }
}
