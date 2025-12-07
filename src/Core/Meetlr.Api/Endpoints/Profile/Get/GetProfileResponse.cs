using Meetlr.Application.Features.Profile.Queries.GetProfile;

namespace Meetlr.Api.Endpoints.Profile.Get;

public class GetProfileResponse
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string MeetlrUsername { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public string? CompanyName { get; init; }
    public string? WelcomeMessage { get; init; }
    public string? Language { get; init; }
    public string? DateFormat { get; init; }
    public string? TimeFormat { get; init; }
    public string? LogoUrl { get; init; }
    public string? BrandColor { get; init; }
    public string? ProfileImageUrl { get; init; }
    public string? Bio { get; init; }

    public static GetProfileResponse FromQueryResponse(GetProfileQueryResponse queryResponse)
    {
        return new GetProfileResponse
        {
            Id = queryResponse.Id,
            FirstName = queryResponse.FirstName,
            LastName = queryResponse.LastName,
            Email = queryResponse.Email,
            PhoneNumber = queryResponse.PhoneNumber,
            MeetlrUsername = queryResponse.MeetlrUsername,
            TimeZone = queryResponse.TimeZone,
            CompanyName = queryResponse.CompanyName,
            WelcomeMessage = queryResponse.WelcomeMessage,
            Language = queryResponse.Language,
            DateFormat = queryResponse.DateFormat,
            TimeFormat = queryResponse.TimeFormat,
            LogoUrl = queryResponse.LogoUrl,
            BrandColor = queryResponse.BrandColor,
            ProfileImageUrl = queryResponse.ProfileImageUrl,
            Bio = queryResponse.Bio
        };
    }
}
