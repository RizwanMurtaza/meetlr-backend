using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Billing.Domain.Entities;
using Meetlr.Module.Billing.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Billing.Infrastructure.Seeding;

/// <summary>
/// Seeds the system credit costs and default packages
/// </summary>
public class BillingSeeder : ISeeder
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BillingSeeder> _logger;

    public int Order => 0; // Run early before user seeders

    public BillingSeeder(
        IUnitOfWork unitOfWork,
        ILogger<BillingSeeder> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting billing data seeding...");

        try
        {
            await SeedSystemCreditCostsAsync(cancellationToken);
            await SeedPackagesAsync(cancellationToken);

            _logger.LogInformation("Billing data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding billing data");
            throw;
        }
    }

    private async Task SeedSystemCreditCostsAsync(CancellationToken cancellationToken)
    {
        // Check if credit costs already exist
        var existingCosts = await _unitOfWork.Repository<SystemCreditCost>()
            .GetQueryable()
            .IgnoreQueryFilters()
            .AnyAsync(cancellationToken);

        if (existingCosts)
        {
            _logger.LogInformation("System credit costs already exist, skipping seeding");
            return;
        }

        var creditCosts = new List<SystemCreditCost>
        {
            new SystemCreditCost
            {
                Id = Guid.NewGuid(),
                ServiceType = ServiceType.Email,
                CreditCost = 1,
                Description = "Cost per email sent",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new SystemCreditCost
            {
                Id = Guid.NewGuid(),
                ServiceType = ServiceType.WhatsApp,
                CreditCost = 5,
                Description = "Cost per WhatsApp message sent",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new SystemCreditCost
            {
                Id = Guid.NewGuid(),
                ServiceType = ServiceType.SMS,
                CreditCost = 6,
                Description = "Cost per SMS message sent",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var cost in creditCosts)
        {
            _unitOfWork.Repository<SystemCreditCost>().Add(cost);
            _logger.LogInformation("Created system credit cost: {ServiceType} = {Cost} credits",
                cost.ServiceType, cost.CreditCost);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully seeded {Count} system credit costs", creditCosts.Count);
    }

    private async Task SeedPackagesAsync(CancellationToken cancellationToken)
    {
        // Check if packages already exist
        var existingPackages = await _unitOfWork.Repository<Package>()
            .GetQueryable()
            .IgnoreQueryFilters()
            .AnyAsync(cancellationToken);

        if (existingPackages)
        {
            _logger.LogInformation("Packages already exist, skipping seeding");
            return;
        }

        var packages = new List<Package>
        {
            // Free starter package - auto-assigned to new users
            new Package
            {
                Id = Guid.NewGuid(),
                Name = "Free Starter",
                Description = "Get started with 1000 free credits. Perfect for trying out our platform.",
                Type = PackageType.PayAsYouGo,
                Price = 0,
                CreditsIncluded = 1000,
                RolloverPercentage = null,
                DurationDays = 36500, // ~100 years (essentially no expiry)
                IsActive = true,
                SortOrder = 0,
                CreatedAt = DateTime.UtcNow
            },
            // Starter package
            new Package
            {
                Id = Guid.NewGuid(),
                Name = "Starter",
                Description = "Perfect for small businesses. Get 100 credits per month.",
                Type = PackageType.Monthly,
                Price = 10.00m,
                CreditsIncluded = 100,
                RolloverPercentage = 50,
                DurationDays = 30,
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            // Pro package
            new Package
            {
                Id = Guid.NewGuid(),
                Name = "Pro",
                Description = "For growing businesses. Get 500 credits per month with rollover.",
                Type = PackageType.Monthly,
                Price = 25.00m,
                CreditsIncluded = 500,
                RolloverPercentage = 75,
                DurationDays = 30,
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            // Enterprise package - Unlimited (Type = Unlimited and CreditsIncluded = null triggers IsUnlimited computed property)
            new Package
            {
                Id = Guid.NewGuid(),
                Name = "Enterprise",
                Description = "Unlimited usage for large organizations. No credit limits.",
                Type = PackageType.Unlimited,
                Price = 99.00m,
                CreditsIncluded = null, // Unlimited
                RolloverPercentage = null,
                DurationDays = 30,
                IsActive = true,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow
            },
            // Annual Pro package
            new Package
            {
                Id = Guid.NewGuid(),
                Name = "Pro Annual",
                Description = "Pro plan billed annually. Save 20% with 6000 credits per year.",
                Type = PackageType.Yearly,
                Price = 240.00m, // $25 x 12 months - 20% = $240
                CreditsIncluded = 6000, // 500 x 12
                RolloverPercentage = 100, // Full rollover within the year
                DurationDays = 365,
                IsActive = true,
                SortOrder = 4,
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var package in packages)
        {
            _unitOfWork.Repository<Package>().Add(package);
            _logger.LogInformation("Created package: {PackageName} - ${Price} - {Credits} credits",
                package.Name, package.Price, package.CreditsIncluded?.ToString() ?? "Unlimited");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully seeded {Count} packages", packages.Count);
    }
}
