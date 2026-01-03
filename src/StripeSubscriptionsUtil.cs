using Microsoft.Extensions.Logging;
using Soenneker.Extensions.DateTime;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Stripe.Client.Abstract;
using Soenneker.Stripe.Subscriptions.Abstract;
using Soenneker.Utils.AsyncSingleton;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Stripe.Subscriptions;

///<inheritdoc cref="IStripeSubscriptionsUtil"/>
public sealed class StripeSubscriptionsUtil : IStripeSubscriptionsUtil
{
    private readonly ILogger<StripeSubscriptionsUtil> _logger;
    private readonly AsyncSingleton<SubscriptionService> _service;

    public StripeSubscriptionsUtil(ILogger<StripeSubscriptionsUtil> logger, IStripeClientUtil stripeUtil)
    {
        _logger = logger;
        _service = new AsyncSingleton<SubscriptionService>(async cancellationToken =>
        {
            StripeClient client = await stripeUtil.Get(cancellationToken)
                                                  .NoSync();
            return new SubscriptionService(client);
        });
    }

    public async ValueTask<Subscription?> Create(SubscriptionCreateOptions options, CancellationToken cancellationToken = default)
    {
        SubscriptionService service = await _service.Get(cancellationToken)
                                                    .NoSync();
        return await service.CreateAsync(options, cancellationToken: cancellationToken)
                            .NoSync();
    }

    public async ValueTask<Subscription?> Create(string customerId, string priceId, string userId, string? defaultPaymentMethodId = null,
        DateTimeOffset? trialEnd = null, CancellationToken cancellationToken = default)
    {
        var options = new SubscriptionCreateOptions
        {
            Customer = customerId,
            Metadata = new Dictionary<string, string> { { "userId", userId } },
            Items = [new SubscriptionItemOptions { Price = priceId }],
            DefaultPaymentMethod = defaultPaymentMethodId,
            TrialEnd = trialEnd?.UtcDateTime,
            AutomaticTax = new SubscriptionAutomaticTaxOptions { Enabled = false }
        };

        return await Create(options, cancellationToken)
            .NoSync();
    }

    public async ValueTask<Subscription?> GetById(string subscriptionId, CancellationToken cancellationToken = default)
    {
        SubscriptionService service = await _service.Get(cancellationToken)
                                                    .NoSync();
        return await service.GetAsync(subscriptionId, cancellationToken: cancellationToken)
                            .NoSync();
    }

    public async ValueTask<List<Subscription>> GetByCustomerId(string customerId, CancellationToken cancellationToken = default)
    {
        var options = new SubscriptionListOptions { Customer = customerId };
        SubscriptionService service = await _service.Get(cancellationToken)
                                                    .NoSync();
        IAsyncEnumerable<Subscription>? subs = service.ListAutoPagingAsync(options, cancellationToken: cancellationToken);
        return await subs.ToListAsync(cancellationToken)
                         .NoSync();
    }

    public async ValueTask<Subscription?> GetByUserId(string userId, CancellationToken cancellationToken = default)
    {
        var options = new SubscriptionSearchOptions { Query = $"metadata[\"userId\"]:\"{userId}\"" };
        SubscriptionService service = await _service.Get(cancellationToken)
                                                    .NoSync();
        StripeSearchResult<Subscription>? result = await service.SearchAsync(options, cancellationToken: cancellationToken)
                                                                .NoSync();
        return result?.Data?.FirstOrDefault();
    }

    public async ValueTask<List<Subscription>> GetAll(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        SubscriptionService service = await _service.Get(cancellationToken)
                                                    .NoSync();
        List<Subscription> allSubs = await service.ListAutoPagingAsync(cancellationToken: cancellationToken)
                                                  .ToListAsync(cancellationToken)
                                                  .NoSync();
        return activeOnly
            ? allSubs.Where(s => s.Status == "active")
                     .ToList()
            : allSubs;
    }

    public async ValueTask<Subscription?> Update(string subscriptionId, SubscriptionUpdateOptions updateOptions, RequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        SubscriptionService service = await _service.Get(cancellationToken)
                                                    .NoSync();
        return await service.UpdateAsync(subscriptionId, updateOptions, requestOptions, cancellationToken: cancellationToken)
                            .NoSync();
    }

    public async ValueTask<Subscription?> UpdatePrice(string subscriptionId, string newPriceId, CancellationToken cancellationToken = default)
    {
        var options = new SubscriptionUpdateOptions
        {
            Items = [new SubscriptionItemOptions { Price = newPriceId }],
            ProrationBehavior = "none"
        };
        return await Update(subscriptionId, options, null, cancellationToken);
    }

    public async ValueTask<Subscription?> UpdateBillingAnchor(Subscription subscription, DateTimeOffset dateTime, TimeZoneInfo timeZoneInfo,
        CancellationToken cancellationToken = default)
    {
        long unixTime = dateTime.ToUnixTimeSeconds();
        var options = new SubscriptionUpdateOptions
        {
            TrialEnd = DateTimeOffset.FromUnixTimeSeconds(unixTime)
                                     .UtcDateTime,
            ProrationBehavior = "none"
        };
        return await Update(subscription.Id, options, null, cancellationToken);
    }

    public async ValueTask UpdateBillingAnchorForAll(DateTimeOffset dateTime, TimeZoneInfo timeZoneInfo, CancellationToken cancellationToken = default)
    {
        List<Subscription> allSubs = await GetAll(cancellationToken: cancellationToken)
            .NoSync();

        foreach (Subscription sub in allSubs)
        {
            await UpdateBillingAnchor(sub, dateTime, timeZoneInfo, cancellationToken)
                .NoSync();
        }
    }

    public async ValueTask CancelById(string subscriptionId, CancellationToken cancellationToken = default)
    {
        SubscriptionService service = await _service.Get(cancellationToken)
                                                    .NoSync();
        await service.CancelAsync(subscriptionId, cancellationToken: cancellationToken)
                     .NoSync();
    }

    public async ValueTask CancelByUserId(string userId, CancellationToken cancellationToken = default)
    {
        Subscription? sub = await GetByUserId(userId, cancellationToken)
            .NoSync();
        if (sub is not null)
            await CancelById(sub.Id, cancellationToken)
                .NoSync();
    }

    public async ValueTask CancelAtPeriodEnd(string subscriptionId, CancellationToken cancellationToken = default)
    {
        var options = new SubscriptionUpdateOptions { CancelAtPeriodEnd = true };
        await Update(subscriptionId, options, null, cancellationToken)
            .NoSync();
    }

    public async ValueTask<Subscription?> Reactivate(string subscriptionId, CancellationToken cancellationToken = default)
    {
        var options = new SubscriptionUpdateOptions { CancelAtPeriodEnd = false };
        return await Update(subscriptionId, options, null, cancellationToken);
    }

    public async ValueTask<bool> IsActive(string subscriptionId, CancellationToken cancellationToken = default)
    {
        SubscriptionService service = await _service.Get(cancellationToken)
                                                    .NoSync();
        Subscription? sub = await service.GetAsync(subscriptionId, cancellationToken: cancellationToken)
                                         .NoSync();
        return sub.Status == "active";
    }

    public async ValueTask CancelAll(CancellationToken cancellationToken = default)
    {
        List<Subscription> allSubs = await GetAll(activeOnly: false, cancellationToken)
            .NoSync();
        foreach (Subscription sub in allSubs)
        {
            await CancelById(sub.Id, cancellationToken)
                .NoSync();
        }
    }

    public void Dispose() => _service.Dispose();

    public ValueTask DisposeAsync() => _service.DisposeAsync();
}