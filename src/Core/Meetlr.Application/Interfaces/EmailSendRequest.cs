namespace Meetlr.Application.Interfaces;

/// <summary>
/// Email send request
/// </summary>
public record EmailSendRequest
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public string? PlainTextBody { get; init; }
    public string? FromEmail { get; init; }
    public string? FromName { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? UserId { get; init; }
    public List<EmailAttachment> Attachments { get; init; } = new();
}

/// <summary>
/// Email attachment
/// </summary>
public record EmailAttachment
{
    public string FileName { get; init; } = string.Empty;
    public byte[] Content { get; init; } = Array.Empty<byte>();
    public string ContentType { get; init; } = "application/octet-stream";
}