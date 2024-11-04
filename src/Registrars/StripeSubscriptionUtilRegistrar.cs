using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Stripe.Client.Registrars;
using Soenneker.Stripe.Customers.Registrars;
using Soenneker.Stripe.Subscriptions.Abstract;

namespace Soenneker.Stripe.Subscriptions.Registrars;

/// <summary>
/// A .NET typesafe implementation of Stripe's Subscription API
/// </summary>
public static class StripeSubscriptionUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IStripeSubscriptionsUtil"/> as a singleton service. <para/>
    /// </summary>
    public static void AddStripeSubscriptionUtilAsSingleton(this IServiceCollection services)
    {
        services.AddStripeCustomersUtilAsSingleton();
        services.TryAddSingleton<IStripeSubscriptionsUtil, StripeSubscriptionsUtil>();
    }

    /// <summary>
    /// Adds <see cref="IStripeSubscriptionsUtil"/> as a scoped service. <para/>
    /// </summary>
    public static void AddStripeSubscriptionUtilAsScoped(this IServiceCollection services)
    {
        services.AddStripeCustomersUtilAsScoped();
        services.TryAddScoped<IStripeSubscriptionsUtil, StripeSubscriptionsUtil>();
    }
}
