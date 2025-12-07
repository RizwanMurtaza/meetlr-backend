using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to application configuration and system setup.
/// All errors use ApplicationArea.Configuration (area code: 17)
/// </summary>
public static class ConfigurationErrors
{
    private const ApplicationArea Area = ApplicationArea.Configuration;

    public static BadRequestException MissingConfiguration(string configKey, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 1, customMessage ?? $"Required configuration '{configKey}' is not set");
        exception.WithDetail("ConfigKey", configKey);
        return exception;
    }

    public static BadRequestException InvalidConfiguration(string configKey, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 2, customMessage ?? $"Configuration '{configKey}' has an invalid value");
        exception.WithDetail("ConfigKey", configKey);
        return exception;
    }

    public static BadRequestException JwtSecretNotConfigured(string? customMessage = null)
        => new(Area, 3, customMessage ?? "JWT SecretKey is not configured");

    public static BadRequestException S3BucketNotConfigured(string? customMessage = null)
        => new(Area, 4, customMessage ?? "S3 bucket name is not configured");

    public static BadRequestException DatabaseConnectionFailed(string? customMessage = null)
        => new(Area, 5, customMessage ?? "Failed to connect to database");

    public static BadRequestException SeedingFailed(string entityType, string errors, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 6, customMessage ?? $"Failed to seed {entityType}: {errors}");
        exception.WithDetail("EntityType", entityType);
        exception.WithDetail("Errors", errors);
        return exception;
    }
}
