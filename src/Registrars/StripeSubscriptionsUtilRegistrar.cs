using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Stripe.Customers.Registrars;
using Soenneker.Stripe.Subscriptions.Abstract;

namespace Soenneker.Stripe.Subscriptions.Registrars;

/// <summary>
/// A .NET typesafe implementation of Stripe's Subscription API
/// </summary>
public static class StripeSubscriptionsUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IStripeSubscriptionsUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddStripeSubscriptionsUtilAsSingleton(this IServiceCollection services)
    {
        services.AddStripeCustomersUtilAsSingleton()
                .TryAddSingleton<IStripeSubscriptionsUtil, StripeSubscriptionsUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IStripeSubscriptionsUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddStripeSubscriptionsUtilAsScoped(this IServiceCollection services)
    {
        services.AddStripeCustomersUtilAsScoped()
                .TryAddScoped<IStripeSubscriptionsUtil, StripeSubscriptionsUtil>();

        return services;
    }
}