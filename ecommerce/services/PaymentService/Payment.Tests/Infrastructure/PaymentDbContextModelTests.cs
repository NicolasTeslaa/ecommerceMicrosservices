using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Persistence;

namespace Payment.Tests.Infrastructure;

public class PaymentDbContextModelTests
{
    [Fact]
    public void Model_ShouldConfigurePaymentEntity()
    {
        using var context = CreateDbContext();
        var entity = context.Model.FindEntityType(typeof(Payment.Domain.Entities.Payment));

        Assert.NotNull(entity);
        Assert.Equal("Payments", entity!.GetTableName());
        Assert.Contains(entity.GetIndexes(), index => index.Properties.Any(property => property.Name == nameof(Payment.Domain.Entities.Payment.OrderId)) && index.IsUnique);
    }

    [Fact]
    public void Model_ShouldConfigurePaymentOutboxEntity()
    {
        using var context = CreateDbContext();
        var entity = context.Model.FindEntityType(typeof(Payment.Domain.Entities.PaymentOutboxMessage));

        Assert.NotNull(entity);
        Assert.Equal("PaymentOutboxMessages", entity!.GetTableName());
    }

    [Fact]
    public void Model_ShouldConfigureProcessedKafkaMessageEntity()
    {
        using var context = CreateDbContext();
        var entity = context.Model.FindEntityType(typeof(Payment.Domain.Entities.ProcessedKafkaMessage));

        Assert.NotNull(entity);
        Assert.Equal("ProcessedKafkaMessages", entity!.GetTableName());
    }

    [Fact]
    public void Model_ShouldConfigureProcessedStripeWebhookEventEntity()
    {
        using var context = CreateDbContext();
        var entity = context.Model.FindEntityType(typeof(Payment.Domain.Entities.ProcessedStripeWebhookEvent));

        Assert.NotNull(entity);
        Assert.Equal("ProcessedStripeWebhookEvents", entity!.GetTableName());
    }

    private static PaymentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PaymentDbContext(options);
    }
}
