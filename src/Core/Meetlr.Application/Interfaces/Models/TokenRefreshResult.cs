namespace Meetlr.Application.Interfaces.Models;

/// <summary>
/// Result of token refresh operation
/// </summary>
public class TokenRefreshResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}
