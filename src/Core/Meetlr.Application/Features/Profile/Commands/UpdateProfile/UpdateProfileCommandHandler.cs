using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Profile.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public UpdateProfileCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<UpdateProfileCommandResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetQueryable()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw UserErrors.UserNotFound(request.UserId);

        var oldValues = new
        {
            user.FirstName,
            user.LastName,
            user.TimeZone,
            user.CompanyName,
            user.WelcomeMessage,
            user.Language,
            user.DateFormat,
            user.TimeFormat,
            user.BrandColor,
            user.ProfileImageUrl,
            user.Bio,
            user.PhoneNumber
        };

        // Update properties
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.TimeZone = request.TimeZone;
        user.CompanyName = request.CompanyName;
        user.WelcomeMessage = request.WelcomeMessage;
        user.Language = request.Language;
        user.DateFormat = request.DateFormat;
        user.TimeFormat = request.TimeFormat;
        user.BrandColor = request.BrandColor;
        user.ProfileImageUrl = request.ProfileImageUrl;
        user.Bio = request.Bio;
        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogAsync(
            AuditEntityType.User,
            user.Id.ToString(),
            AuditAction.Update,
            oldValues,
            user,
            cancellationToken);

        return new UpdateProfileCommandResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            MeetlrUsername = user.MeetlrUsername ?? string.Empty,
            TimeZone = user.TimeZone,
            CompanyName = user.CompanyName,
            WelcomeMessage = user.WelcomeMessage,
            Language = user.Language,
            DateFormat = user.DateFormat,
            TimeFormat = user.TimeFormat,
            LogoUrl = user.LogoUrl,
            BrandColor = user.BrandColor,
            ProfileImageUrl = user.ProfileImageUrl,
            Bio = user.Bio,
            Success = true
        };
    }
}
