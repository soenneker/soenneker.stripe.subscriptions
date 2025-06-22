using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stripe;

namespace Soenneker.Stripe.Subscriptions.Abstract;

/// <summary>
/// Defines CRUD operations for Stripe subscriptions.
/// </summary>
public interface IStripeSubscriptionsUtil : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Creates a new subscription using the provided options.
    /// </summary>
    /// <param name="options">Stripe subscription create options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created <see cref="Subscription"/>, or null if creation failed.</returns>
    ValueTask<Subscription?> Create(SubscriptionCreateOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new subscription for a customer with a single price item.
    /// </summary>
    /// <param name="customerId">The Stripe customer ID.</param>
    /// <param name="priceId">The price ID for the subscription item.</param>
    /// <param name="userId">An application-specific user identifier for metadata.</param>
    /// <param name="defaultPaymentMethodId">Optional default payment method ID.</param>
    /// <param name="trialEnd">Optional trial end date/time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created <see cref="Subscription"/>, or null if creation failed.</returns>
    ValueTask<Subscription?> Create(string customerId, string priceId, string userId, string? defaultPaymentMethodId = null, DateTimeOffset? trialEnd = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a subscription by its Stripe subscription ID.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The <see cref="Subscription"/>, or null if not found.</returns>
    ValueTask<Subscription?> GetById(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all subscriptions for a given customer.
    /// </summary>
    /// <param name="customerId">The Stripe customer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of <see cref="Subscription"/> objects.</returns>
    ValueTask<List<Subscription>> GetByCustomerId(string customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a subscription by the application-specific user ID stored in metadata.
    /// </summary>
    /// <param name="userId">The application user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The <see cref="Subscription"/>, or null if none match.</returns>
    ValueTask<Subscription?> GetByUserId(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all subscriptions, optionally filtering only active ones.
    /// </summary>
    /// <param name="activeOnly">Whether to include only active subscriptions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of <see cref="Subscription"/> objects.</returns>
    ValueTask<List<Subscription>> GetAll(bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a subscription with the specified update options.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="updateOptions">Options describing the update.</param>
    /// <param name="requestOptions">Optional Stripe request options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="Subscription"/>, or null if update failed.</returns>
    ValueTask<Subscription?> Update(string subscriptionId, SubscriptionUpdateOptions updateOptions, RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the price for an existing subscription without prorating.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="newPriceId">The new price ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="Subscription"/>, or null if update failed.</returns>
    ValueTask<Subscription?> UpdatePrice(string subscriptionId, string newPriceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adjusts the billing anchor (trial end) date for a subscription without prorating.
    /// </summary>
    /// <param name="subscription">The subscription to update.</param>
    /// <param name="dateTime">New billing anchor date/time.</param>
    /// <param name="timeZoneInfo">Time zone of the provided date/time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="Subscription"/>, or null if update failed.</returns>
    ValueTask<Subscription?> UpdateBillingAnchor(Subscription subscription, DateTime dateTime, TimeZoneInfo timeZoneInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a billing anchor update to all subscriptions.
    /// </summary>
    /// <param name="dateTime">New billing anchor date/time for all subs.</param>
    /// <param name="timeZoneInfo">Time zone of the provided date/time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask UpdateBillingAnchorForAll(DateTime dateTime, TimeZoneInfo timeZoneInfo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Immediately cancels a subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask CancelById(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a subscription associated with the given user ID.
    /// </summary>
    /// <param name="userId">The application user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask CancelByUserId(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a subscription to cancel at the end of its current period.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask CancelAtPeriodEnd(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reactivates a subscription that was set to cancel at period end.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="Subscription"/>, or null if reactivation failed.</returns>
    ValueTask<Subscription?> Reactivate(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a subscription is currently active.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if active; otherwise, <c>false</c>.</returns>
    ValueTask<bool> IsActive(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels all subscriptions (active and inactive).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask CancelAll(CancellationToken cancellationToken = default);
}