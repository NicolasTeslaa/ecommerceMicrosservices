using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Infrastructure.Configuration;

namespace Payment.Tests.Infrastructure;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_ShouldThrow_WhenConnectionStringIsMissing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var act = () => services.AddInfrastructure(configuration);

        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void StripeOptions_ShouldHaveExpectedDefaults()
    {
        var options = new StripeOptions();

        Assert.Equal(string.Empty, options.SecretKey);
        Assert.Equal(string.Empty, options.PublishableKey);
        Assert.Equal(string.Empty, options.WebhookSecret);
        Assert.Equal("brl", options.Currency);
    }
}
