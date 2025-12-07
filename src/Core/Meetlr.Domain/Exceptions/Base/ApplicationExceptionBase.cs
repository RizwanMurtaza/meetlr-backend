using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.Base;

/// <summary>
/// Base class for all application exceptions
/// </summary>
public abstract class ApplicationExceptionBase : Exception
{
    /// <summary>
    /// HTTP status code associated with this exception
    /// </summary>
    public HttpStatusCode HttpStatusCode { get; }

    /// <summary>
    /// Application area where the exception occurred
    /// </summary>
    public ApplicationArea ApplicationArea { get; }

    /// <summary>
    /// Specific message code within the application area
    /// </summary>
    public int MessageCode { get; }

    /// <summary>
    /// Full error code in format: {HttpStatusCode}-{ApplicationArea}-{MessageCode}
    /// </summary>
    public string FullErrorCode => $"{(int)HttpStatusCode}-{(int)ApplicationArea}-{MessageCode}";

    /// <summary>
    /// Additional details about the exception
    /// </summary>
    public Dictionary<string, object?> Details { get; set; } = new();

    /// <summary>
    /// Trace ID for tracking the error across systems
    /// </summary>
    public string? TraceId { get; set; }

    protected ApplicationExceptionBase(
        HttpStatusCode httpStatusCode,
        ApplicationArea applicationArea,
        int messageCode,
        string message)
        : base(message)
    {
        if (messageCode <= 0)
            throw new ArgumentException("Message code must be positive", nameof(messageCode));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));

        HttpStatusCode = httpStatusCode;
        ApplicationArea = applicationArea;
        MessageCode = messageCode;
    }

    /// <summary>
    /// Add a single detail to the exception
    /// </summary>
    public ApplicationExceptionBase WithDetail(string key, object? value)
    {
        Details[key] = value;
        return this;
    }

    /// <summary>
    /// Add multiple details to the exception
    /// </summary>
    public ApplicationExceptionBase WithDetails(Dictionary<string, object?> details)
    {
        foreach (var detail in details)
        {
            Details[detail.Key] = detail.Value;
        }
        return this;
    }

    /// <summary>
    /// Set the trace ID for this exception
    /// </summary>
    public ApplicationExceptionBase WithTraceId(string? traceId)
    {
        TraceId = traceId;
        return this;
    }

    /// <summary>
    /// Convert the exception to an API response format
    /// </summary>
    public object ToApiResponse()
    {
        return new
        {
            ErrorCode = FullErrorCode,
            Message,
            HttpStatusCode = (int)HttpStatusCode,
            ApplicationArea = ApplicationArea.ToString(),
            Details = Details.Count > 0 ? Details : null,
            TraceId = !string.IsNullOrWhiteSpace(TraceId) ? TraceId : null,
            Timestamp = DateTime.UtcNow
        };
    }
}
