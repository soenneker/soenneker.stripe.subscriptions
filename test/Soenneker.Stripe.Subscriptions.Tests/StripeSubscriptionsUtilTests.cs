using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Soenneker.Stripe.Subscriptions.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;
using Xunit.Abstractions;

namespace Soenneker.Stripe.Subscriptions.Tests;

[Collection("Collection")]
public class StripeSubscriptionsUtilTests : FixturedUnitTest
{
    private readonly IStripeSubscriptionsUtil _util;

    public StripeSubscriptionsUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IStripeSubscriptionsUtil>();
    }
}
