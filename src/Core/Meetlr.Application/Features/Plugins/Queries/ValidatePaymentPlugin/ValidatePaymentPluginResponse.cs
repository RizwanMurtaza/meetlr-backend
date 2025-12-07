namespace Meetlr.Application.Features.Plugins.Queries.ValidatePaymentPlugin;

public class ValidatePaymentPluginResponse
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}