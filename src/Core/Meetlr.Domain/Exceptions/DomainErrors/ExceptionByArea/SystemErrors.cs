using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines system-level errors.
/// All errors use ApplicationArea.System (area code: 1)
/// </summary>
public static class SystemErrors
{
    private const ApplicationArea Area = ApplicationArea.System;

    public static BadRequestException RepositoryCreationFailed(string typeName, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 1, customMessage ?? $"Could not create repository for type {typeName}");
        exception.WithDetail("TypeName", typeName);
        return exception;
    }

    public static BadRequestException InternalError(string? customMessage = null)
        => new(Area, 2, customMessage ?? "An internal system error occurred");

    public static BadRequestException ServiceUnavailable(string serviceName, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 3, customMessage ?? $"Service '{serviceName}' is temporarily unavailable");
        exception.WithDetail("ServiceName", serviceName);
        return exception;
    }
}
