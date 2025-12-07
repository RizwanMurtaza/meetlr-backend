using System.Reflection;
using Meetlr.Application.Plugins;
using Meetlr.Application.Plugins.Payments;
using Meetlr.Plugins.Payment.Stripe.Endpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meetlr.Plugins.Payment.Stripe;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Stripe Payment Provider Plugin and its controllers
    /// </summary>
    public static IMvcBuilder AddStripePaymentProvider(
        this IMvcBuilder mvcBuilder,
        IConfiguration configuration)
    {
        var services = mvcBuilder.Services;

        // Configure Stripe settings
        services.Configure<StripeSettings>(options =>
        {
            var stripeSection = configuration.GetSection("Stripe");
            options.SecretKey = stripeSection["SecretKey"] ?? string.Empty;
            options.PublishableKey = stripeSection["PublishableKey"] ?? string.Empty;
            options.ClientId = stripeSection["ClientId"] ?? string.Empty;
            options.WebhookSecret = stripeSection["WebhookSecret"] ?? string.Empty;
            options.ApplicationFeePercent = int.Parse(stripeSection["ApplicationFeePercent"] ?? "10");
        });

        // Register the Stripe plugin as SCOPED (not singleton) because it uses IUnitOfWork
        // Register as concrete type first
        services.AddScoped<StripePaymentProviderPlugin>();

        // Register as IPlugin for unified factory
        services.AddScoped<IPlugin>(sp => sp.GetRequiredService<StripePaymentProviderPlugin>());

        // Register as IPaymentPlugin for payment-specific operations
        services.AddScoped<IPaymentPlugin>(sp => sp.GetRequiredService<StripePaymentProviderPlugin>());

        // Register MediatR handlers from this plugin assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register plugin controllers
        mvcBuilder.AddApplicationPart(typeof(ConnectStripe).Assembly);

        return mvcBuilder;
    }
}
