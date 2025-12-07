using FluentValidation;
using Meetlr.Domain.Exceptions.Base;

namespace Meetlr.Api.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (ValidationException validationException)
        {
            _logger.LogWarning(validationException,
                "Validation failed: {Errors}",
                string.Join(", ", validationException.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));

            await HandleValidationExceptionAsync(httpContext, validationException);
        }
        catch (ApplicationExceptionBase appException)
        {
            _logger.LogError(appException,
                "Application exception occurred: {ErrorCode} - {Message}",
                appException.FullErrorCode,
                appException.Message);

            // Set trace ID from context
            appException.WithTraceId(httpContext.TraceIdentifier);

            httpContext.Response.StatusCode = (int)appException.HttpStatusCode;
            httpContext.Response.ContentType = "application/json";

            var response = appException.ToApiResponse();
            await httpContext.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleUnhandledExceptionAsync(httpContext, ex);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        var errorResponse = new
        {
            ErrorCode = "400-0-1",
            Message = "Validation failed. Please check the errors and try again.",
            HttpStatusCode = 400,
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Errors = errors
        };

        await context.Response.WriteAsJsonAsync(errorResponse);
    }

    private static async Task HandleUnhandledExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var errorResponse = new
        {
            ErrorCode = "500-0-0",
            Message = "An internal server error occurred. Please try again later.",
            HttpStatusCode = 500,
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            // Include exception details only in development
#if DEBUG
            Details = new
            {
                ExceptionType = exception.GetType().Name,
                ExceptionMessage = exception.Message,
                StackTrace = exception.StackTrace
            }
#endif
        };

        await context.Response.WriteAsJsonAsync(errorResponse);
    }
}
