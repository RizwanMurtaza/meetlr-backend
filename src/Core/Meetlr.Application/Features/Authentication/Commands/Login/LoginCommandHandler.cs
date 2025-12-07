using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Tenancy;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Authentication.Commands.Login;

/// <summary>
/// Handler for user login
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginCommandResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IIdentityService identityService,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginCommandResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Validate credentials
        var (succeeded, userId) = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);
        if (!succeeded)
        {
            throw AuthenticationErrors.InvalidCredentials();
        }

        // Get domain user entity by email
        var user = await _unitOfWork.Repository<User>().GetQueryable()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            throw AuthenticationErrors.InvalidCredentials();
        }

        // Check if email is confirmed
        if (!user.EmailConfirmed)
        {
            throw AuthenticationErrors.EmailNotConfirmed();
        }
        // Check if user is admin
        var isAdmin = await _unitOfWork.Repository<UserGroup>()
            .GetQueryable()
            .AnyAsync(ug => ug.UserId == user.Id && ug.Group.IsAdminGroup, cancellationToken);
        
        // Generate JWT token
        var token = await _identityService.GenerateTokenAsync(userId, user.Email ?? throw AuthenticationErrors.InvalidCredentials(), isAdmin, cancellationToken);
        var expiresAt = await _identityService.GetTokenExpiryAsync();

        // Generate refresh token
        var refreshToken = await _identityService.GenerateRefreshTokenAsync(userId, request.IpAddress, request.DeviceInfo, cancellationToken);
        var refreshTokenExpiresAt = _identityService.GetRefreshTokenExpiry();

        // Update last login
        await _identityService.UpdateLastLoginAsync(userId);

        // Return response
        return new LoginCommandResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MeetlrUsername = user.MeetlrUsername,
            IsAdmin = isAdmin
        };
    }
}
