using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Common.Specifications;
using Meetlr.Application.Features.Availability.Queries.ValidateBookingSlots;
using Meetlr.Application.Features.Contacts.Queries.GetOrCreateContact;
using Meetlr.Application.Features.Payments.Commands.CreatePaymentIntent;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Bookings.Commands.CreateBooking;

/// <summary>
/// Handler for creating bookings - pure CQRS orchestrator (follows SRP and CQRS)
/// Calendar sync, video meeting creation, and notifications are handled via domain events (BookingCompletedEvent)
/// </summary>
public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, CreateBookingCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateBookingCommandHandler> _logger;
    private readonly ITenantService _tenantService;
    private readonly ISlotInvitationService? _slotInvitationService;

    public CreateBookingCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        IMediator mediator,
        ILogger<CreateBookingCommandHandler> logger,
        ITenantService tenantService,
        ISlotInvitationService? slotInvitationService = null)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _mediator = mediator;
        _logger = logger;
        _tenantService = tenantService;
        _slotInvitationService = slotInvitationService;
    }

    public async Task<CreateBookingCommandResponse> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Get event type with user info
        var spec = new MeetlrEventSpecifications.ByIdWithUser(request.MeetlrEventId);
        var meetlrEvent = await _unitOfWork.Repository<MeetlrEvent>()
            .FirstOrDefaultAsync(spec, cancellationToken);

        if (meetlrEvent == null)
            throw MeetlrEventErrors.MeetlrEventNotFound(request.MeetlrEventId);

        if (meetlrEvent.Status != MeetlrEventStatus.Active)
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

        // 1c. Validate slot invitation token if provided
        SlotInvitationDto? slotInvitation = null;
        if (!string.IsNullOrEmpty(request.SlotInvitationToken) && _slotInvitationService != null)
        {
            slotInvitation = await _slotInvitationService.GetByTokenAsync(request.SlotInvitationToken, cancellationToken);

            if (slotInvitation == null)
                throw BookingErrors.SlotInvitationNotFound(request.SlotInvitationToken);

            if (slotInvitation.MeetlrEventId != request.MeetlrEventId)
                throw BookingErrors.SlotInvitationEventMismatch(request.SlotInvitationToken, request.MeetlrEventId);

            if (slotInvitation.Status != "Pending")
                throw BookingErrors.SlotInvitationNotPending(request.SlotInvitationToken);

            if (slotInvitation.ExpiresAt < DateTime.UtcNow)
                throw BookingErrors.SlotInvitationExpired(request.SlotInvitationToken);

            // Override start time with the invitation's slot time
            // Note: The frontend should also use this time, but we enforce it here for security
            _logger.LogInformation(
                "Booking via slot invitation {Token}. Overriding start time from {RequestedTime} to {InvitationTime}",
                request.SlotInvitationToken,
                request.StartTime,
                slotInvitation.SlotStartTime);
        }

        // 2. Validate booking slot using unified query (handles OneOnOne vs Group + capacity)
        // Use slot invitation time if provided, otherwise use request time
        var effectiveStartTime = slotInvitation?.SlotStartTime ?? request.StartTime;

        var validateQuery = new ValidateBookingSlotsQuery
        {
            MeetlrEventId = request.MeetlrEventId,
            RequestedSlots = new List<DateTime> { effectiveStartTime },
            TimeZone = request.InviteeTimeZone ?? meetlrEvent.User.TimeZone,
            // Skip slot invitation reservation check if we're booking via that invitation
            SlotInvitationTokenToExclude = request.SlotInvitationToken
        };
        var validation = await _mediator.Send(validateQuery, cancellationToken);

        // 3. If conflicts exist, throw exception
        if (validation.HasConflicts && validation.Conflicts.Any())
        {
            var conflict = validation.Conflicts.First();
            throw BookingErrors.TimeSlotConflict(conflict.RequestedTime, conflict.RequestedTime.AddMinutes(meetlrEvent.DurationMinutes));
        }

        // Calculate UTC times for booking
        // For FullDay events, normalize to full day (start of day to end of day)
        // Use slot invitation times if available, otherwise calculate from effectiveStartTime
        DateTime startTimeUtc;
        DateTime endTimeUtc;

        if (slotInvitation != null)
        {
            // Use the exact times from the slot invitation
            startTimeUtc = slotInvitation.SlotStartTime;
            endTimeUtc = slotInvitation.SlotEndTime;
        }
        else if (meetlrEvent.MeetingType == MeetingType.FullDay)
        {
            // For FullDay events, use the date from StartTime but set times to full day
            // StartTime = beginning of the day (00:00:00)
            // EndTime = end of the day (23:59:59)
            startTimeUtc = effectiveStartTime.Date;
            endTimeUtc = effectiveStartTime.Date.AddDays(1).AddTicks(-1);
        }
        else
        {
            startTimeUtc = effectiveStartTime;
            endTimeUtc = startTimeUtc.AddMinutes(meetlrEvent.DurationMinutes);
        }

        // 4. Get or create contact for this invitee
        var contact = await _mediator.Send(new GetOrCreateContactQuery
        {
            Email = request.InviteeEmail,
            Name = request.InviteeName,
            Phone = request.InviteePhone,
            TimeZone = request.InviteeTimeZone ?? meetlrEvent.User.TimeZone,
            UserId = meetlrEvent.UserId,
            Source = ContactSource.Booking
        }, cancellationToken);

        // 5. Create booking entity
        // For paid bookings: set Status to Pending to hold the slot until payment is completed
        // For free bookings: set Status to Confirmed immediately
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantService.TenantId ?? throw BookingErrors.TenantIdRequired(),
            MeetlrEventId = meetlrEvent.Id,
            HostUserId = meetlrEvent.UserId,
            SeriesBookingId = request.SeriesBookingId, // Link to series if part of recurring booking
            ContactId = contact.Id, // Link to contact entity - all invitee info comes from Contact
            Contact = contact, // Set navigation property for use in notification service
            StartTime = startTimeUtc,
            EndTime = endTimeUtc,
            Status = meetlrEvent.RequiresPayment ? BookingStatus.Pending : BookingStatus.Confirmed,
            Location = meetlrEvent.LocationDetails,
            Notes = request.Notes,
            PaymentStatus = meetlrEvent.RequiresPayment ? PaymentStatus.Pending : PaymentStatus.NotRequired,
            Amount = meetlrEvent.Fee,
            Currency = meetlrEvent.Currency,
            PaymentProviderType = meetlrEvent.RequiresPayment ? meetlrEvent.PaymentProviderType : null,
            ConfirmationToken = Guid.NewGuid().ToString(),
            CancellationToken = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };
        _unitOfWork.Repository<Booking>().Add(booking);

        // 6. Save answers to custom questions
        if (request.Answers != null && request.Answers.Any())
        {
            foreach (var answerRequest in request.Answers)
            {
                var answer = new BookingAnswer
                {
                    Id = Guid.NewGuid(),
                    TenantId = booking.TenantId,
                    BookingId = booking.Id,
                    MeetlrEventQuestionId = answerRequest.QuestionId,
                    Answer = answerRequest.Answer,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.Repository<BookingAnswer>().Add(answer);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6a. Mark single-use link as used if applicable
        if (singleUseLink != null)
        {
            singleUseLink.IsUsed = true;
            singleUseLink.UsedAt = DateTime.UtcNow;
            singleUseLink.BookingId = booking.Id;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // 6b. Mark slot invitation as accepted if applicable
        if (slotInvitation != null && _slotInvitationService != null)
        {
            await _slotInvitationService.MarkAsAcceptedAsync(
                slotInvitation.Id,
                booking.Id,
                cancellationToken);

            _logger.LogInformation(
                "Slot invitation {InvitationId} accepted via booking {BookingId}",
                slotInvitation.Id,
                booking.Id);
        }

        // 7. Log audit
        await _auditService.LogAsync(
            AuditEntityType.Booking,
            booking.Id.ToString(),
            AuditAction.Create,
            null,
            booking,
            cancellationToken);

        // 8. Create payment intent using plugin system (provider-agnostic)
        string? paymentUrl = null;
        string? clientSecret = null;

        if (meetlrEvent.RequiresPayment && meetlrEvent.Fee.HasValue && !string.IsNullOrEmpty(meetlrEvent.Currency))
        {
            var metadata = new Dictionary<string, string>
            {
                { "booking_id", booking.Id.ToString() },
                { "event_type", meetlrEvent.Name },
                { "invitee_email", contact.Email },
                { "invitee_name", contact.Name },
                { "start_time", booking.StartTime.ToString("o") }
            };

            // Payment plugin handles connected account lookup internally
            var createPaymentCommand = new CreatePaymentIntentCommand
            {
                BookingId = booking.Id,
                MeetlrEventId = meetlrEvent.Id,
                UserId = meetlrEvent.UserId,
                Amount = meetlrEvent.Fee.Value,
                Currency = meetlrEvent.Currency ?? "USD",
                PaymentProviderType = meetlrEvent.PaymentProviderType?.ToString() ?? "unknown",
                Metadata = metadata
            };

            var paymentResult = await _mediator.Send(createPaymentCommand, cancellationToken);

            if (paymentResult.Success)
            {
                clientSecret = paymentResult.ClientSecret;
                paymentUrl = paymentResult.PaymentUrl;
                booking.PaymentIntentId = paymentResult.PaymentIntentId;
                booking.PaymentStatus = PaymentStatus.Initiated;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("Failed to create payment intent: {Error}", paymentResult.ErrorMessage);
            }
        }

        // 9. For free bookings: BookingCompletedEvent is auto-raised when Status = Confirmed (in object initializer)
        // For paid bookings: event will be raised after payment confirmation (when status changes to Confirmed)
        // This triggers: VideoLinkCreation, CalendarSync, Email notifications (queued sequentially via background service)
        // No manual AddDomainEvent needed - Status setter handles this automatically

        // 10. Return response
        return new CreateBookingCommandResponse
        {
            BookingId = booking.Id,
            ConfirmationToken = booking.ConfirmationToken,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            MeetlrEventName = meetlrEvent.Name,
            HostName = $"{meetlrEvent.User.FirstName} {meetlrEvent.User.LastName}",
            Location = booking.Location,
            MeetingLink = booking.MeetingLink,
            RequiresPayment = meetlrEvent.RequiresPayment,
            Amount = booking.Amount,
            Currency = booking.Currency,
            PaymentUrl = paymentUrl,
            PaymentClientSecret = clientSecret
        };
    }
}
