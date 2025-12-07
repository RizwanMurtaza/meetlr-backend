using Meetlr.Domain.Common;
using Meetlr.Domain.Entities.Users;

namespace Meetlr.Domain.Entities.Payments;

/// <summary>
/// Stores Stripe Connect account information for users who want to receive payments
/// </summary>
public class StripeAccount : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    
    // Stripe Connect Account Details
    public string StripeAccountId { get; set; } = string.Empty; // Stripe Connect Account ID
    public bool IsConnected { get; set; }
    public bool ChargesEnabled { get; set; } // Can accept payments
    public bool PayoutsEnabled { get; set; } // Can receive payouts
    public bool DetailsSubmitted { get; set; } // Completed onboarding
    
    // Account Information
    public string? Country { get; set; }
    public string? Currency { get; set; }
    public string? Email { get; set; }
    public string? BusinessType { get; set; } // individual, company, etc.
    
    // Verification Status
    public string? VerificationStatus { get; set; } // pending, verified, unverified
    public string? DisabledReason { get; set; } // Why account is disabled (if applicable)
    public DateTime? ConnectedAt { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    
    // OAuth Access
    public string? AccessToken { get; set; } // Encrypted in production
    public string? RefreshToken { get; set; } // Encrypted in production
    public string? Scope { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}
