using Bogus;
using  Meetlr.Domain.Entities.Tenancy;
using Meetlr.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Data.Seeding;

/// <summary>
/// Seeds tenant data for testing
/// </summary>
public class TenantSeeder : ISeeder
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TenantSeeder> _logger;

    public int Order => 2;

    public TenantSeeder(
        IUnitOfWork unitOfWork,
        ILogger<TenantSeeder> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting tenant seeding...");

        try
        {
            // Check if test tenants already exist (excluding admin tenant)
            var existingTenants = await _unitOfWork.Repository<Tenant>()
                .GetQueryable()
                .Where(t => !t.IsSuperTenant)
                .ToListAsync(cancellationToken);

            if (existingTenants.Any())
            {
                _logger.LogInformation("Test tenants already exist ({Count}), skipping seeding", existingTenants.Count);
                return;
            }

            // Create realistic tenant data using Bogus
            var tenantFaker = new Faker<Tenant>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.Subdomain, (f, t) => t.Name.ToLower().Replace(" ", "").Replace("&", "and").Replace(",", "").Replace(".", ""))
                .RuleFor(t => t.MainText, f => f.Company.CatchPhrase())
                .RuleFor(t => t.Description, f => f.Lorem.Sentence(10))
                .RuleFor(t => t.Email, (f, t) => $"info@{t.Subdomain}.com")
                .RuleFor(t => t.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(t => t.Website, (f, t) => $"https://www.{t.Subdomain}.com")
                .RuleFor(t => t.Address, f => f.Address.StreetAddress())
                .RuleFor(t => t.City, f => f.Address.City())
                .RuleFor(t => t.Country, f => f.Address.Country())
                .RuleFor(t => t.PostalCode, f => f.Address.ZipCode())
                .RuleFor(t => t.PrimaryColor, f => f.Internet.Color())
                .RuleFor(t => t.SecondaryColor, f => f.Internet.Color())
                .RuleFor(t => t.AccentColor, f => f.Internet.Color())
                .RuleFor(t => t.TimeZone, f => f.PickRandom("America/New_York", "America/Los_Angeles", "Europe/London", "Europe/Paris", "Asia/Tokyo"))
                .RuleFor(t => t.Language, f => "en")
                .RuleFor(t => t.DateFormat, f => "MM/dd/yyyy")
                .RuleFor(t => t.TimeFormat, f => "12h")
                .RuleFor(t => t.Currency, f => f.PickRandom("USD", "EUR", "GBP"))
                .RuleFor(t => t.IsActive, f => true)
                .RuleFor(t => t.IsSuperTenant, f => false)
                .RuleFor(t => t.SubscriptionStartDate, f => DateTime.UtcNow.AddDays(-f.Random.Int(30, 180)))
                .RuleFor(t => t.SubscriptionEndDate, f => DateTime.UtcNow.AddDays(f.Random.Int(180, 365)))
                .RuleFor(t => t.SubscriptionPlan, f => f.PickRandom("Free", "Pro", "Enterprise"))
                .RuleFor(t => t.MaxUsers, f => f.Random.Int(10, 100))
                .RuleFor(t => t.MaxBookingsPerMonth, f => f.Random.Int(100, 1000))
                .RuleFor(t => t.MaxServices, f => f.Random.Int(5, 50))
                .RuleFor(t => t.MetaTitle, (f, t) => $"{t.Name} - Schedule Your Appointment")
                .RuleFor(t => t.MetaDescription, (f, t) => $"Book appointments with {t.Name}. {t.MainText}")
                .RuleFor(t => t.CreatedAt, f => DateTime.UtcNow.AddDays(-f.Random.Int(30, 180)))
                .RuleFor(t => t.UpdatedAt, f => DateTime.UtcNow.AddDays(-f.Random.Int(0, 30)));

            // Generate 3 test tenants
            var tenants = tenantFaker.Generate(3);

            // Ensure unique subdomains
            var existingSubdomains = await _unitOfWork.Repository<Tenant>()
                .GetQueryable()
                .Select(t => t.Subdomain)
                .ToListAsync(cancellationToken);

            foreach (var tenant in tenants)
            {
                var baseSubdomain = tenant.Subdomain;
                var counter = 1;
                while (existingSubdomains.Contains(tenant.Subdomain))
                {
                    tenant.Subdomain = $"{baseSubdomain}{counter}";
                    counter++;
                }
                existingSubdomains.Add(tenant.Subdomain);

                _unitOfWork.Repository<Tenant>().Add(tenant);
                _logger.LogInformation("Created tenant: {TenantName} (Subdomain: {Subdomain})",
                    tenant.Name, tenant.Subdomain);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully seeded {Count} tenants", tenants.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding tenants");
            throw;
        }
    }
}
