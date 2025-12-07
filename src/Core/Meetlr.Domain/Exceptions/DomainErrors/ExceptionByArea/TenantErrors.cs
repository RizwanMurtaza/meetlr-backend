using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to tenant management.
/// All errors use ApplicationArea.Tenants (area code: 4)
/// </summary>
public static class TenantErrors
{
    private const ApplicationArea Area = ApplicationArea.Tenants;

    public static NotFoundException TenantNotFound(string subdomain, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 1, customMessage ?? "Tenant not found");
        exception.WithDetail("Subdomain", subdomain);
        return exception;
    }

    public static NotFoundException TenantNotFoundById(Guid tenantId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 2, customMessage ?? "Tenant not found");
        exception.WithDetail("TenantId", tenantId);
        return exception;
    }

    public static ConflictException SubdomainAlreadyExists(string subdomain, string? customMessage = null)
    {
        var exception = new ConflictException(Area, 3, customMessage ?? "This subdomain is already taken");
        exception.WithDetail("Subdomain", subdomain);
        return exception;
    }

    public static ConflictException CustomDomainAlreadyExists(string customDomain, string? customMessage = null)
    {
        var exception = new ConflictException(Area, 4, customMessage ?? "This custom domain is already in use");
        exception.WithDetail("CustomDomain", customDomain);
        return exception;
    }

    public static ForbiddenException TenantInactive(string? customMessage = null)
        => new(Area, 5, customMessage ?? "This tenant is not active");

    public static BadRequestException InvalidSubdomain(string subdomain, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 6, customMessage ?? "Invalid subdomain format");
        exception.WithDetail("Subdomain", subdomain);
        return exception;
    }

    public static BadRequestException TenantNotResolved(string? customMessage = null)
        => new(Area, 7, customMessage ?? "Tenant context could not be resolved");

    public static BadRequestException TenantIdCannotBeModified(string? customMessage = null)
        => new(Area, 8, customMessage ?? "TenantId cannot be modified once set");
}
