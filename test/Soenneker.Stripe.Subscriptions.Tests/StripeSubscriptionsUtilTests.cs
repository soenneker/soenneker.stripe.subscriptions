using Soenneker.Stripe.Subscriptions.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Stripe.Subscriptions.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class StripeSubscriptionsUtilTests : HostedUnitTest
{
    private readonly IStripeSubscriptionsUtil _util;

    public StripeSubscriptionsUtilTests(Host host) : base(host)
    {
        _util = Resolve<IStripeSubscriptionsUtil>(true);
    }

    [Test]
    public void Default()
    {

    }
}
