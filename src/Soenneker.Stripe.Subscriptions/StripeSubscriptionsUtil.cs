using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Stripe.Client;
using Soenneker.Stripe.Client.Abstract;
using Soenneker.Stripe.OpenApiClient.Models;
using Soenneker.Stripe.Subscriptions.Abstract;

namespace Soenneker.Stripe.Subscriptions;

public sealed class StripeSubscriptionsUtil(IStripeClientUtil client) : IStripeSubscriptionsUtil
{
    public async ValueTask<Subscription?> Create(PostSubscriptionsXWwwFormUrlencodedRequest options, CancellationToken cancellationToken = default)
    {
        var api = await client.Get(cancellationToken);
        return await api.V1.Subscriptions.PostAsync(options, cancellationToken: cancellationToken);
    }

    public ValueTask<Subscription?> Create(string customerId, string priceId, string userId, string? defaultPaymentMethodId = null,
        DateTimeOffset? trialEnd = null, CancellationToken cancellationToken = default) =>
        Create(new PostSubscriptionsXWwwFormUrlencodedRequest
        {
            Customer = customerId, Items = [new() { Price = priceId }], DefaultPaymentMethod = defaultPaymentMethodId,
            TrialEnd = trialEnd.HasValue ? new() { Integer = checked((int)trialEnd.Value.ToUnixTimeSeconds()) } : null,
            AutomaticTax = new() { Enabled = false },
            Metadata = new() { PostSubscriptionsXWwwFormUrlencodedRequestMetadataAnyOf1 = new() { AdditionalData = new Dictionary<string, object> { ["userId"] = userId } } }
        }, cancellationToken);

    public async ValueTask<Subscription?> GetById(string subscriptionId, CancellationToken cancellationToken = default)
    {
        var api = await client.Get(cancellationToken);
        return await api.V1.Subscriptions[subscriptionId].GetAsync(new(), cancellationToken: cancellationToken);
    }

    private async ValueTask<List<Subscription>> List(string? customerId, CancellationToken cancellationToken)
    {
        var api = await client.Get(cancellationToken);
        var result = new List<Subscription>();
        string? cursor = null;
        do
        {
            var page = await api.V1.Subscriptions.GetAsync(new(), c => { c.QueryParameters.Customer = customerId; c.QueryParameters.Limit = 100;
                c.QueryParameters.StartingAfter = cursor; c.QueryParameters.Status = GetSubscriptionsStatusParameter.All; }, cancellationToken);
            if (page?.Data is not { Count: > 0 }) break;
            result.AddRange(page.Data);
            cursor = page.HasMore == true ? page.Data[^1].Id : null;
        } while (cursor != null);
        return result;
    }

    public ValueTask<List<Subscription>> GetByCustomerId(string customerId, CancellationToken cancellationToken = default) => List(customerId, cancellationToken);

    public async ValueTask<Subscription?> GetByUserId(string userId, CancellationToken cancellationToken = default)
    {
        var api = await client.Get(cancellationToken);
        var page = await api.V1.Subscriptions.Search.GetAsync(new(), c => c.QueryParameters.Query = $"metadata[\"userId\"]:\"{userId}\"", cancellationToken);
        return page?.Data?.FirstOrDefault();
    }

    public async ValueTask<List<Subscription>> GetAll(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var result = await List(null, cancellationToken);
        if (activeOnly) result.RemoveAll(subscription => subscription.Status != SubscriptionStatus.Active);
        return result;
    }

    public async ValueTask<Subscription?> Update(string subscriptionId, PostSubscriptionsSubscriptionExposedIdXWwwFormUrlencodedRequest updateOptions, StripeRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        var api = await client.Get(cancellationToken);
        return await api.V1.Subscriptions[subscriptionId].PostAsync(updateOptions, c => requestOptions?.Apply(c.Headers), cancellationToken);
    }

    public async ValueTask<Subscription?> UpdatePrice(string subscriptionId, string newPriceId, CancellationToken cancellationToken = default)
    {
        var existing = (await GetById(subscriptionId, cancellationToken))?.Items?.Data?.FirstOrDefault();
        if (existing == null) return null;
        return await Update(subscriptionId, new() { Items = [new() { Id = existing.Id, Price = newPriceId }],
            ProrationBehavior = PostSubscriptionsSubscriptionExposedIdXWwwFormUrlencodedRequestProrationBehavior.None }, cancellationToken: cancellationToken);
    }

    public ValueTask<Subscription?> UpdateBillingAnchor(Subscription subscription, DateTimeOffset dateTime, TimeZoneInfo timeZoneInfo,
        CancellationToken cancellationToken = default) => Update(subscription.Id!, new()
        {
            TrialEnd = new() { Integer = checked((int)dateTime.ToUnixTimeSeconds()) },
            ProrationBehavior = PostSubscriptionsSubscriptionExposedIdXWwwFormUrlencodedRequestProrationBehavior.None
        }, cancellationToken: cancellationToken);

    public async ValueTask UpdateBillingAnchorForAll(DateTimeOffset dateTime, TimeZoneInfo timeZoneInfo, CancellationToken cancellationToken = default)
    {
        foreach (var subscription in await GetAll(cancellationToken: cancellationToken))
            await UpdateBillingAnchor(subscription, dateTime, timeZoneInfo, cancellationToken);
    }

    public async ValueTask CancelById(string subscriptionId, CancellationToken cancellationToken = default)
    {
        var api = await client.Get(cancellationToken);
        await api.V1.Subscriptions[subscriptionId].DeleteAsync(new(), cancellationToken: cancellationToken);
    }

    public async ValueTask CancelByUserId(string userId, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByUserId(userId, cancellationToken);
        if (subscription?.Id != null) await CancelById(subscription.Id, cancellationToken);
    }

    public async ValueTask CancelAtPeriodEnd(string subscriptionId, CancellationToken cancellationToken = default) =>
        await Update(subscriptionId, new() { CancelAtPeriodEnd = true }, cancellationToken: cancellationToken);

    public ValueTask<Subscription?> Reactivate(string subscriptionId, CancellationToken cancellationToken = default) =>
        Update(subscriptionId, new() { CancelAtPeriodEnd = false }, cancellationToken: cancellationToken);

    public async ValueTask<bool> IsActive(string subscriptionId, CancellationToken cancellationToken = default) =>
        (await GetById(subscriptionId, cancellationToken))?.Status == SubscriptionStatus.Active;

    public async ValueTask CancelAll(CancellationToken cancellationToken = default)
    {
        foreach (var subscription in await GetAll(false, cancellationToken))
            if (subscription.Status is not (SubscriptionStatus.Canceled or SubscriptionStatus.IncompleteExpired))
                await CancelById(subscription.Id!, cancellationToken);
    }
    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
