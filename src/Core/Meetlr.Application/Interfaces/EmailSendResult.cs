namespace Meetlr.Application.Interfaces;

/// <summary>
/// Email send result
/// </summary>
public record EmailSendResult
{
    public bool Success { get; init; }
    public string? MessageId { get; init; }
    public string? ErrorMessage { get; init; }
    public string ProviderUsed { get; init; } = string.Empty;

    public static EmailSendResult Successful(string messageId, string providerName) =>
        new() { Success = true, MessageId = messageId, ProviderUsed = providerName };

    public static EmailSendResult Failed(string errorMessage, string providerName) =>
        new() { Success = false, ErrorMessage = errorMessage, ProviderUsed = providerName };
}