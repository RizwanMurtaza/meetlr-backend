using MediatR;
using Meetlr.Application.Interfaces;
using Meetlr.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Meetlr.Module.Calendar.Endpoints.Auth.OAuthCallback;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class OAuthCallback : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IOAuthService _oauthService;
    private readonly GoogleOAuthService _googleOAuth;
    private readonly MicrosoftOAuthService _microsoftOAuth;

    public OAuthCallback(
        IMediator mediator, 
        IOAuthService oauthService,
        GoogleOAuthService googleOAuth,
        MicrosoftOAuthService microsoftOAuth)
    {
        _mediator = mediator;
        _oauthService = oauthService;
        _googleOAuth = googleOAuth;
        _microsoftOAuth = microsoftOAuth;
    }

    /// <summary>
    /// OAuth callback endpoint for Google/Microsoft signup
    /// Creates tenant automatically based on username
    /// </summary>
    [HttpPost("oauth/callback")]
    [ProducesResponseType(typeof(OAuthCallbackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OAuthCallbackResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle([FromBody] OAuthCallbackRequest request)
    {
        try
        {
            string accessToken;
            string? refreshToken;
            DateTime? tokenExpiry;

            // If authorization code is provided, exchange it for tokens server-side
            if (!string.IsNullOrEmpty(request.Code))
            {
                if (string.IsNullOrEmpty(request.RedirectUri))
                {
                    return BadRequest(new { error = "RedirectUri is required when using authorization code" });
                }

                // Log for debugging
                Console.WriteLine($"OAuth Callback - Provider: {request.Provider}");
                Console.WriteLine($"OAuth Callback - Code length: {request.Code.Length}");
                Console.WriteLine($"OAuth Callback - Code first 50 chars: {request.Code.Substring(0, Math.Min(50, request.Code.Length))}...");
                Console.WriteLine($"OAuth Callback - RedirectUri: {request.RedirectUri}");

                if (request.Provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
                {
                    var tokenResponse = await _googleOAuth.ExchangeCodeForTokenAsync(request.Code, request.RedirectUri);
                    accessToken = tokenResponse.AccessToken;
                    refreshToken = tokenResponse.RefreshToken;
                    tokenExpiry = DateTime.UtcNow.AddSeconds(3600); // Google tokens typically last 1 hour
                }
                else if (request.Provider.Equals("Microsoft", StringComparison.OrdinalIgnoreCase))
                {
                    var tokenResponse = await _microsoftOAuth.ExchangeCodeForTokenAsync(request.Code, request.RedirectUri);
                    accessToken = tokenResponse.access_token;
                    refreshToken = tokenResponse.refresh_token;
                    tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in);
                }
                else
                {
                    return BadRequest(new { error = "Unsupported OAuth provider" });
                }
            }
            // Fallback to direct access token (deprecated)
            else if (!string.IsNullOrEmpty(request.AccessToken))
            {
                accessToken = request.AccessToken;
                refreshToken = request.RefreshToken;
                tokenExpiry = request.ExpiresIn.HasValue 
                    ? DateTime.UtcNow.AddSeconds(request.ExpiresIn.Value) 
                    : null;
            }
            else
            {
                return BadRequest(new { error = "Either Code or AccessToken is required" });
            }

            // Step 1: Get user info from OAuth provider
            var userInfo = await _oauthService.GetUserInfoFromTokenAsync(
                accessToken,
                request.Provider);

            // Step 2: Create command with OAuth user info
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var deviceInfo = Request.Headers.UserAgent.ToString();

            var command = OAuthCallbackRequest.ToCommand(
                request,
                userInfo.Email,
                userInfo.FirstName,
                userInfo.LastName,
                userInfo.ProviderId,
                userInfo.ProfileImageUrl,
                accessToken,
                refreshToken,
                tokenExpiry,
                ipAddress,
                deviceInfo);

            // Step 3: Execute signup command (creates tenant if new user)
            var commandResponse = await _mediator.Send(command);

            // Step 4: Return response
            var response = OAuthCallbackResponse.FromCommandResponse(commandResponse);

            if (commandResponse.IsNewUser)
            {
                return CreatedAtAction(nameof(Handle), new { id = response.UserId }, response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            // Log the full exception for debugging
            Console.WriteLine($"OAuth Callback Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            return BadRequest(new { error = ex.Message, details = ex.InnerException?.Message });
        }
    }
}
