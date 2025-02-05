using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.DateTime;
using Soenneker.Extensions.Enumerable;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Stripe.Client.Abstract;
using Soenneker.Stripe.Customers.Abstract;
using Soenneker.Stripe.Subscriptions.Abstract;
using Soenneker.Utils.AsyncSingleton;
using Stripe;

namespace Soenneker.Stripe.Subscriptions;

///<inheritdoc cref="IStripeSubscriptionsUtil"/>
public class StripeSubscriptionsUtil : IStripeSubscriptionsUtil
{
    private readonly ILogger<StripeSubscriptionsUtil> _logger;
    private readonly IStripeCustomersUtil _stripeCustomerUtil;

    private readonly AsyncSingleton<SubscriptionService> _service;

    public StripeSubscriptionsUtil(ILogger<StripeSubscriptionsUtil> logger, IStripeClientUtil stripeUtil, IStripeCustomersUtil stripeCustomerUtil)
    {
        _logger = logger;
        _stripeCustomerUtil = stripeCustomerUtil;

        _service = new AsyncSingleton<SubscriptionService>(async (cancellationToken, _) =>
        {
            StripeClient client = await stripeUtil.Get(cancellationToken).NoSync();

            return new SubscriptionService(client);
        });
    }

    public async ValueTask<Subscription?> Create(string email, string name, string userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(")) StripeSubscriptionsUtil: Creating subscription {email} ...", email);

        var options = new SubscriptionCreateOptions
        {
            Metadata = new Dictionary<string, string>
            {
                {"userId", userId}
            }
        };

        Subscription? subscription = await (await _service.Get(cancellationToken).NoSync()).CreateAsync(options, cancellationToken: cancellationToken).NoSync();

        _logger.LogDebug(")) StripeSubscriptionsUtil: Created subscription {email}", email);

        return subscription;
    }

    public async ValueTask<List<Subscription>> GetAll(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(")) StripeSubscriptionsUtil: Getting all Stripe subscriptions...");

        IAsyncEnumerable<Subscription>? response = (await _service.Get(cancellationToken).NoSync()).ListAutoPagingAsync(cancellationToken: cancellationToken);

        if (response == null)
            throw new NullReferenceException("Stripe subscription response is null");

        List<Subscription> result = await response.ToListAsync(cancellationToken).NoSync();

        if (activeOnly)
            result = result.Where(s => s.Status == "active").ToList();

        _logger.LogDebug(")) StripeSubscriptionsUtil: Finished retrieving all Stripe subscriptions");

        if (result.Empty())
            _logger.LogWarning("Stripe subscription response is empty");

        return result;
    }

    public async ValueTask UpdateBillingAnchorForAll(DateTime dateTime, TimeZoneInfo timeZoneInfo, CancellationToken cancellationToken = default)
    {
        List<Subscription> allSubscriptions = await GetAll(cancellationToken: cancellationToken).NoSync();

        _logger.LogDebug(")) StripeSubscriptionsUtil: Updating billing anchor for all subscriptions to dateTime ({dateTime}) ...", dateTime.ToTzDateTimeFormat(timeZoneInfo));

        foreach (Subscription subscription in allSubscriptions)
        {
            await UpdateBillingAnchor(subscription, dateTime, timeZoneInfo, cancellationToken).NoSync();
        }
    }

    public async ValueTask<Subscription?> UpdateBillingAnchor(Subscription subscription, DateTime dateTime, TimeZoneInfo timeZoneInfo, CancellationToken cancellationToken = default)
    {
        Customer? customer = await _stripeCustomerUtil.Get(subscription.CustomerId, cancellationToken).NoSync();

        if (customer == null)
        {
            _logger.LogDebug("Stripe customer is null for subscription ({subscriptionId}), skipping", subscription.Id);
            return null;
        }

        long unixTime = dateTime.ToUnixTimeSeconds();

        _logger.LogDebug(")) StripeSubscriptionsUtil: Updating billing anchor for customer ({customer}) to ({dateTime}) ...", customer.Email, dateTime.ToTzDateTimeFormat(timeZoneInfo));

        var options = new SubscriptionUpdateOptions
        {
            TrialEnd = DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime,
            ProrationBehavior = "none",
        };

        Subscription? result = await (await _service.Get(cancellationToken).NoSync()).UpdateAsync(subscription.Id, options, cancellationToken: cancellationToken).NoSync();
        return result;
    }

    public async ValueTask<Subscription?> GetByUserId(string userId, CancellationToken cancellationToken = default)
    {
        var options = new SubscriptionSearchOptions
        {
            Query = $"metadata[\"userId\"]:\"{userId}\""
        };

        List<Subscription>? response = (await (await _service.Get(cancellationToken).NoSync()).SearchAsync(options, cancellationToken: cancellationToken).NoSync()).Data;

        if (response.IsNullOrEmpty())
            return null;

        return response.FirstOrDefault();
    }

    public async ValueTask CancelAll(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(")) StripeSubscriptionsUtil: Canceling all subscriptions...");

        List<Subscription>? subscriptions = await GetAll(cancellationToken: cancellationToken).NoSync();

        if (subscriptions == null)
            return;

        SubscriptionService service = await _service.Get(cancellationToken).NoSync();

        foreach (Subscription subscription in subscriptions)
        {
            await service.CancelAsync(subscription.Id, cancellationToken: cancellationToken).NoSync();
        }

        _logger.LogWarning(")) StripeSubscriptionsUtil: Canceling all subscriptions");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _service.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return _service.DisposeAsync();
    }
}