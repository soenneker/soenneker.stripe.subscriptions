using Soenneker.Stripe.Subscriptions.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Stripe.Subscriptions.Tests;

[Collection("Collection")]
public class StripeSubscriptionsUtilTests : FixturedUnitTest
{
    private readonly IStripeSubscriptionsUtil _util;

    public StripeSubscriptionsUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IStripeSubscriptionsUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
