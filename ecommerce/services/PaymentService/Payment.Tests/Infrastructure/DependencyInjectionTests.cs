using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Infrastructure.Configuration;

namespace Payment.Tests.Infrastructure;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_ShouldRegisterServices_WhenConnectionStringIsMissing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddInfrastructure(configuration);

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(Payment.Application.Interfaces.IPaymentRepository));
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
