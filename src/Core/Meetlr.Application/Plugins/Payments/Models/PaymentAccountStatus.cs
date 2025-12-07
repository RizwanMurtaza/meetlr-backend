namespace Meetlr.Application.Plugins.Payments.Models;

/// <summary>
/// Payment account connection status
/// </summary>
public class PaymentAccountStatus
{
    public bool IsConnected { get; set; }
    public bool ChargesEnabled { get; set; }
    public bool PayoutsEnabled { get; set; }
    public bool DetailsSubmitted { get; set; }
    public string? Country { get; set; }
    public string? Currency { get; set; }
    public string? Email { get; set; }
    public string? VerificationStatus { get; set; }
    public string? DisabledReason { get; set; }
    public DateTime? ConnectedAt { get; set; }
}
