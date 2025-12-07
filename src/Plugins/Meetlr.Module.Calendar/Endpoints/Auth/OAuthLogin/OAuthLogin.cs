using Meetlr.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Calendar.Endpoints.Auth.OAuthLogin;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class OAuthLogin : ControllerBase
{
    private readonly GoogleOAuthService _googleOAuthService;
    private readonly MicrosoftOAuthService _microsoftOAuthService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OAuthLogin> _logger;

    public OAuthLogin(
        GoogleOAuthService googleOAuthService,
        MicrosoftOAuthService microsoftOAuthService,
        IConfiguration configuration,
        ILogger<OAuthLogin> logger)
    {
        _googleOAuthService = googleOAuthService;
        _microsoftOAuthService = microsoftOAuthService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Get OAuth authorization URL for Google login
    /// </summary>
    [HttpGet("oauth/google/url")]
    [ProducesResponseType(typeof(OAuthUrlResponse), StatusCodes.Status200OK)]
    public IActionResult GetGoogleAuthUrl([FromQuery] string? redirectUri = null)
    {
        _logger.LogInformation("=== Google OAuth Login Request ===");
        _logger.LogInformation("Received request for Google OAuth URL");
        _logger.LogInformation("RedirectUri parameter: {RedirectUri}", redirectUri ?? "(null)");

        var defaultRedirectUri = _configuration["OAuth:Google:RedirectUri"] ?? "http://localhost:3000/auth/callback";
        var uri = redirectUri ?? defaultRedirectUri;
        // Encode provider in state so callback knows which provider was used
        var state = $"google_{Guid.NewGuid()}";

        _logger.LogInformation("Using redirect URI: {Uri}", uri);
        _logger.LogInformation("Generated state: {State}", state);

        var authUrl = _googleOAuthService.GetAuthorizationUrl(uri, state);

        _logger.LogInformation("Generated authorization URL: {AuthUrl}", authUrl);
        _logger.LogInformation("=== End Google OAuth Request ===");

        return Ok(new OAuthUrlResponse
        {
            AuthorizationUrl = authUrl,
            State = state,
            Provider = "Google"
        });
    }

    /// <summary>
    /// Get OAuth authorization URL for Microsoft login
    /// </summary>
    [HttpGet("oauth/microsoft/url")]
    [ProducesResponseType(typeof(OAuthUrlResponse), StatusCodes.Status200OK)]
    public IActionResult GetMicrosoftAuthUrl([FromQuery] string? redirectUri = null)
    {
        _logger.LogInformation("=== Microsoft OAuth Login Request ===");
        _logger.LogInformation("Received request for Microsoft OAuth URL");
        _logger.LogInformation("RedirectUri parameter: {RedirectUri}", redirectUri ?? "(null)");

        var defaultRedirectUri = _configuration["OAuth:Microsoft:RedirectUri"] ?? "http://localhost:3000/auth/callback";
        var uri = redirectUri ?? defaultRedirectUri;
        // Encode provider in state so callback knows which provider was used
        var state = $"microsoft_{Guid.NewGuid()}";

        _logger.LogInformation("Using redirect URI: {Uri}", uri);
        _logger.LogInformation("Generated state: {State}", state);

        var authUrl = _microsoftOAuthService.GetAuthorizationUrl(uri, state);

        _logger.LogInformation("Generated authorization URL: {AuthUrl}", authUrl);
        _logger.LogInformation("=== End Microsoft OAuth Request ===");

        return Ok(new OAuthUrlResponse
        {
            AuthorizationUrl = authUrl,
            State = state,
            Provider = "Microsoft"
        });
    }
}
