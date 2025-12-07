using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Profile.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, GetProfileQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProfileQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetProfileQueryResponse> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetQueryable()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw UserErrors.UserNotFound(request.UserId);

        return new GetProfileQueryResponse
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
            Bio = user.Bio
        };
    }
}
