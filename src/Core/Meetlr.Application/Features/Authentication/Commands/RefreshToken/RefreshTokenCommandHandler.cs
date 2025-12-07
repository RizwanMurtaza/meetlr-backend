using MediatR;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

namespace Meetlr.Application.Features.Authentication.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenCommandResponse>
{
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<RefreshTokenCommandResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.RefreshTokenAsync(
            request.RefreshToken,
            isAdmin: null,
            request.IpAddress,
            request.DeviceInfo,
            cancellationToken);

        if (!result.IsValid || result.NewAccessToken == null || result.NewRefreshToken == null)
        {
            throw AuthenticationErrors.InvalidRefreshToken();
        }

        return new RefreshTokenCommandResponse
        {
            AccessToken = result.NewAccessToken,
            RefreshToken = result.NewRefreshToken,
            AccessTokenExpiresAt = result.AccessTokenExpiry ?? DateTime.UtcNow,
            RefreshTokenExpiresAt = result.RefreshTokenExpiry ?? DateTime.UtcNow,
            UserId = result.UserId
        };
    }
}
