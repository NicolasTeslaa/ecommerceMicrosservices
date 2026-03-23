using Microsoft.Extensions.Options;
using Payment.Infrastructure.Clients;
using Payment.Infrastructure.Configuration;
using Stripe;

namespace Payment.Tests.Infrastructure;

public class StripePaymentGatewayTests
{
    [Fact]
    public void Constructor_ShouldSetStripeApiKey_WhenSecretKeyExists()
    {
        var options = Options.Create(new StripeOptions
        {
            SecretKey = "sk_test_example"
        });

        _ = new StripePaymentGateway(options);

        Assert.Equal("sk_test_example", StripeConfiguration.ApiKey);
    }

    [Fact]
    public void Constructor_ShouldNotThrow_WhenSecretKeyIsMissing()
    {
        var options = Options.Create(new StripeOptions());

        var gateway = new StripePaymentGateway(options);

        Assert.NotNull(gateway);
    }
}
