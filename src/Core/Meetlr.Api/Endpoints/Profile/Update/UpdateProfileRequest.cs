using Meetlr.Application.Features.Profile.Commands.UpdateProfile;

namespace Meetlr.Api.Endpoints.Profile.Update;

public class UpdateProfileRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public string? CompanyName { get; init; }
    public string? WelcomeMessage { get; init; }
    public string? Language { get; init; }
    public string? DateFormat { get; init; }
    public string? TimeFormat { get; init; }
    public string? BrandColor { get; init; }
    public string? ProfileImageUrl { get; init; }
    public string? Bio { get; init; }
    public string? PhoneNumber { get; init; }

    public static UpdateProfileCommand ToCommand(UpdateProfileRequest request, Guid userId)
    {
        return new UpdateProfileCommand
        {
            UserId = userId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            TimeZone = request.TimeZone,
            CompanyName = request.CompanyName,
            WelcomeMessage = request.WelcomeMessage,
            Language = request.Language,
            DateFormat = request.DateFormat,
            TimeFormat = request.TimeFormat,
            BrandColor = request.BrandColor,
            ProfileImageUrl = request.ProfileImageUrl,
            Bio = request.Bio,
            PhoneNumber = request.PhoneNumber
        };
    }
}
