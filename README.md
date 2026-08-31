[![](https://img.shields.io/nuget/v/soenneker.stripe.subscriptions.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.stripe.subscriptions/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.stripe.subscriptions/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.stripe.subscriptions/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.stripe.subscriptions.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.stripe.subscriptions/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.stripe.subscriptions/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.stripe.subscriptions/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Stripe.Subscriptions

Create, retrieve, search, update, reactivate, and cancel Stripe subscriptions, including application-user metadata lookup and account-wide operations.

## Installation

```bash
dotnet add package Soenneker.Stripe.Subscriptions
```

## Configuration

```json
{
  "Stripe": {
    "SecretKey": "sk_test_..."
  }
}
```

## Usage

```csharp
using Soenneker.Stripe.Subscriptions.Abstract;
using Soenneker.Stripe.Subscriptions.Registrars;
using Stripe;

services.AddStripeSubscriptionsUtilAsScoped();

Subscription? subscription = await subscriptionsUtil.Create(
    customerId: "cus_...",
    priceId: "price_...",
    userId: applicationUserId,
    defaultPaymentMethodId: "pm_...",
    cancellationToken: cancellationToken);

await subscriptionsUtil.CancelAtPeriodEnd(
    subscription!.Id,
    cancellationToken);
```

The convenience `Create` overload stores `userId` in Stripe metadata, creates one price item, disables automatic tax, and optionally sets a trial end. `GetByUserId` searches that metadata and returns the first match.

`UpdatePrice` replaces the price on the subscription's first item without proration. `UpdateBillingAnchor` and `UpdateBillingAnchorForAll` set `trial_end`, which also moves Stripe's billing anchor and affects trial behavior.

`GetAll`, `GetByCustomerId`, and the account-wide mutation helpers auto-page through all matching subscriptions. `CancelById` and `CancelAll` cancel immediately; `CancelAtPeriodEnd` preserves service through the current period. Treat `CancelAll` and `UpdateBillingAnchorForAll` as destructive account-wide operations.
