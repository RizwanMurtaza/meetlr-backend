using Meetlr.Application.Features.Profile.Commands.UpdateProfile;

namespace Meetlr.Api.Endpoints.Profile.Update;

public class UpdateProfileResponse
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
    public bool Success { get; init; }

    public static UpdateProfileResponse FromCommandResponse(UpdateProfileCommandResponse commandResponse)
    {
        return new UpdateProfileResponse
        {
            Id = commandResponse.Id,
            FirstName = commandResponse.FirstName,
            LastName = commandResponse.LastName,
            Email = commandResponse.Email,
            PhoneNumber = commandResponse.PhoneNumber,
            MeetlrUsername = commandResponse.MeetlrUsername,
            TimeZone = commandResponse.TimeZone,
            CompanyName = commandResponse.CompanyName,
            WelcomeMessage = commandResponse.WelcomeMessage,
            Language = commandResponse.Language,
            DateFormat = commandResponse.DateFormat,
            TimeFormat = commandResponse.TimeFormat,
            LogoUrl = commandResponse.LogoUrl,
            BrandColor = commandResponse.BrandColor,
            ProfileImageUrl = commandResponse.ProfileImageUrl,
            Bio = commandResponse.Bio,
            Success = commandResponse.Success
        };
    }
}
