using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Stripe;

namespace Soenneker.Stripe.Subscriptions.Abstract;

/// <summary>
/// Interface for Stripe subscription utilities, providing methods for creating, retrieving, updating,
/// and canceling subscriptions with asynchronous disposal support.
/// </summary>
public interface IStripeSubscriptionsUtil : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Creates a new subscription for the specified user.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="name">The name of the user.</param>
    /// <param name="userId">The unique identifier for the user.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> containing the created <see cref="Subscription"/>, or null if creation failed.</returns>
    ValueTask<Subscription?> Create(string email, string name, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all subscriptions from Stripe. Use with caution, as this may retrieve a large number of records.
    /// </summary>
    /// <param name="activeOnly">If true, retrieves only active subscriptions; otherwise, retrieves all subscriptions.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> containing a list of all <see cref="Subscription"/> records.</returns>
    [Pure]
    ValueTask<List<Subscription>> GetAll(bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a subscription by the application's unique user identifier, not the Stripe subscription ID.
    /// </summary>
    /// <param name="userId">The unique identifier for the user.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> containing the <see cref="Subscription"/> for the specified user, or null if not found.</returns>
    [Pure]
    ValueTask<Subscription?> GetByUserId(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels all active subscriptions.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the completion of the operation.</returns>
    ValueTask CancelAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the billing anchor date for a specified subscription.
    /// </summary>
    /// <param name="subscription">The subscription to update.</param>
    /// <param name="dateTime">The new billing anchor date and time.</param>
    /// <param name="timeZoneInfo">The time zone for the specified date and time.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> containing the updated <see cref="Subscription"/>, or null if the update failed.</returns>
    ValueTask<Subscription?> UpdateBillingAnchor(Subscription subscription, DateTime dateTime, TimeZoneInfo timeZoneInfo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the billing anchor date for all active subscriptions.
    /// </summary>
    /// <param name="dateTime">The new billing anchor date and time for all subscriptions.</param>
    /// <param name="timeZoneInfo">The time zone for the specified date and time.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the completion of the operation.</returns>
    ValueTask UpdateBillingAnchorForAll(DateTime dateTime, TimeZoneInfo timeZoneInfo, CancellationToken cancellationToken = default);
}