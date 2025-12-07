using Meetlr.Domain.Common;

namespace Meetlr.Module.Billing.Domain.Entities;

/// <summary>
/// Tracks a user's current credit balance.
/// Each user has one UserCredits record.
/// </summary>
public class UserCredits : BaseAuditableEntity
{
    /// <summary>
    /// The user who owns these credits
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Current credit balance
    /// </summary>
    public int Balance { get; set; } = 0;

    /// <summary>
    /// Whether the user has an unlimited plan (bypass credit checks)
    /// </summary>
    public bool IsUnlimited { get; set; } = false;

    /// <summary>
    /// When the unlimited plan expires (null if not unlimited or never expires)
    /// </summary>
    public DateTime? UnlimitedExpiresAt { get; set; }

    /// <summary>
    /// Version for optimistic concurrency control
    /// Prevents race conditions during concurrent credit deductions
    /// </summary>
    public int Version { get; set; } = 1;

    // Helper Methods

    /// <summary>
    /// Check if user can use services (has credits or unlimited)
    /// </summary>
    public bool CanUseCredits(int amount)
    {
        if (IsUnlimited && (!UnlimitedExpiresAt.HasValue || UnlimitedExpiresAt.Value > DateTime.UtcNow))
            return true;

        return Balance >= amount;
    }

    /// <summary>
    /// Deduct credits from balance (does not check - caller must verify first)
    /// </summary>
    public void DeductCredits(int amount)
    {
        if (!IsUnlimited)
        {
            Balance -= amount;
        }
        Version++;
    }

    /// <summary>
    /// Add credits to balance
    /// </summary>
    public void AddCredits(int amount)
    {
        Balance += amount;
        Version++;
    }
}
