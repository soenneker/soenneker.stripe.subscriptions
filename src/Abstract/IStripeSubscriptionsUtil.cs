using Stripe;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Stripe.Subscriptions.Abstract;

/// <summary>
/// A utility for managing Stripe subscriptions, including creation, retrieval, billing anchor updates, plan changes, cancellation, and state inspection.
/// </summary>
public interface IStripeSubscriptionsUtil : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Creates a new Stripe subscription for a customer with a specific price and optional default payment method and trial.
    /// </summary>
    /// <param name="customerId">The Stripe customer ID to associate the subscription with.</param>
    /// <param name="priceId">The Stripe price ID to subscribe the customer to.</param>
    /// <param name="userId">The internal user ID, stored as metadata for searchability.</param>
    /// <param name="defaultPaymentMethodId">An optional payment method ID to use for the subscription.</param>
    /// <param name="trialEnd">An optional trial end date for the subscription.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created <see cref="Subscription"/>, or null if creation failed.</returns>
    [Pure]
    ValueTask<Subscription?> Create(string customerId, string priceId, string userId, string? defaultPaymentMethodId = null, DateTimeOffset? trialEnd = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the first subscription associated with a specific user ID stored in metadata.
    /// </summary>
    /// <param name="userId">The internal user ID stored in subscription metadata.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching <see cref="Subscription"/> if found, otherwise null.</returns>
    [Pure]
    ValueTask<Subscription?> GetByUserId(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all subscriptions associated with a specific Stripe customer ID.
    /// </summary>
    /// <param name="customerId">The Stripe customer ID to look up subscriptions for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="Subscription"/> objects for the customer.</returns>
    [Pure]
    ValueTask<List<Subscription>> GetByCustomerId(string customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all subscriptions in the account, with optional filtering by active status.
    /// </summary>
    /// <param name="activeOnly">If true, only active subscriptions will be returned.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="Subscription"/> objects.</returns>
    [Pure]
    ValueTask<List<Subscription>> GetAll(bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the price (plan) for an existing subscription without proration.
    /// </summary>
    /// <param name="subscriptionId">The ID of the subscription to update.</param>
    /// <param name="newPriceId">The new Stripe price ID to replace the current one.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated <see cref="Subscription"/> if successful, otherwise null.</returns>
    ValueTask<Subscription?> UpdatePrice(string subscriptionId, string newPriceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the trial end date (billing anchor) of a subscription.
    /// </summary>
    /// <param name="subscription">The subscription to update.</param>
    /// <param name="dateTime">The new billing anchor as a DateTime.</param>
    /// <param name="timeZoneInfo">The timezone to format logs/debug output.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated <see cref="Subscription"/>.</returns>
    ValueTask<Subscription?> UpdateBillingAnchor(Subscription subscription, DateTime dateTime, TimeZoneInfo timeZoneInfo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the billing anchor (trial end date) of all active subscriptions to a specified date.
    /// </summary>
    /// <param name="dateTime">The new billing anchor as a DateTime.</param>
    /// <param name="timeZoneInfo">The timezone to format logs/debug output.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    ValueTask UpdateBillingAnchorForAll(DateTime dateTime, TimeZoneInfo timeZoneInfo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a specific subscription immediately.
    /// </summary>
    /// <param name="subscriptionId">The ID of the subscription to cancel.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    ValueTask CancelById(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a subscription associated with a specific user ID (found via metadata).
    /// </summary>
    /// <param name="userId">The internal user ID stored in metadata.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    ValueTask CancelByUserId(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a subscription at the end of the current billing period instead of immediately.
    /// </summary>
    /// <param name="subscriptionId">The ID of the subscription to cancel.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    ValueTask CancelAtPeriodEnd(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reactivates a subscription that was previously marked to cancel at period end.
    /// </summary>
    /// <param name="subscriptionId">The ID of the subscription to reactivate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated <see cref="Subscription"/> if successful.</returns>
    ValueTask<Subscription?> Reactivate(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the given subscription is currently active.
    /// </summary>
    /// <param name="subscriptionId">The ID of the subscription to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the subscription is active, false otherwise.</returns>
    [Pure]
    ValueTask<bool> IsActive(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels all subscriptions in the Stripe account. Use with extreme caution.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    ValueTask CancelAll(CancellationToken cancellationToken = default);
}
