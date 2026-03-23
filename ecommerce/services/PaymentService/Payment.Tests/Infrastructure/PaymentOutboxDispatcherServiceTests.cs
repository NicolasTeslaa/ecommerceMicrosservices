using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Payment.Domain.Entities;
using Payment.Infrastructure.Messaging;

namespace Payment.Tests.Infrastructure;

public class PaymentOutboxDispatcherServiceTests
{
    [Fact]
    public async Task TryPublishAsync_ShouldReturnFalse_WhenKafkaBootstrapServersAreMissing()
    {
        var service = new PaymentOutboxDispatcherService(
            Mock.Of<IServiceScopeFactory>(),
            new ConfigurationBuilder().Build(),
            Mock.Of<ILogger<PaymentOutboxDispatcherService>>());

        var method = typeof(PaymentOutboxDispatcherService).GetMethod("TryPublishAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var task = (Task<bool>)method.Invoke(service, new object[]
        {
            PaymentOutboxMessage.Create(Guid.NewGuid(), "topic", "key", "type", "{}"),
            CancellationToken.None
        })!;

        var result = await task;

        Assert.False(result);
    }
}
