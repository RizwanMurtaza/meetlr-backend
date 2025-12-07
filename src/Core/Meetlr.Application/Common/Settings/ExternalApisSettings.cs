namespace Meetlr.Application.Common.Settings;

public class ExternalApisSettings
{
    public GoogleApisSettings Google { get; set; } = new();
    public MicrosoftApisSettings Microsoft { get; set; } = new();
    public StripeApisSettings Stripe { get; set; } = new();
}