using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Stripe.Client.Registrars;
using Soenneker.Stripe.Subscriptions.Abstract;

namespace Soenneker.Stripe.Subscriptions.Registrars;

/// <summary>
/// Registers Stripe subscription operations.
/// </summary>
public static class StripeSubscriptionsUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IStripeSubscriptionsUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddStripeSubscriptionsUtilAsSingleton(this IServiceCollection services)
    {
        services.AddStripeClientUtilAsSingleton().TryAddSingleton<IStripeSubscriptionsUtil, StripeSubscriptionsUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IStripeSubscriptionsUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddStripeSubscriptionsUtilAsScoped(this IServiceCollection services)
    {
        services.AddStripeClientUtilAsSingleton().TryAddScoped<IStripeSubscriptionsUtil, StripeSubscriptionsUtil>();

        return services;
    }
}
