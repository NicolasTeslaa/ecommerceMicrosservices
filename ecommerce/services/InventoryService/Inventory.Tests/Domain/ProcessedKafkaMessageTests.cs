using Inventory.Domain.Entities;

namespace Inventory.Tests.Domain;

public class ProcessedKafkaMessageTests
{
    [Fact]
    public void Constructor_ShouldStoreKafkaCoordinates()
    {
        var message = new ProcessedKafkaMessage("payment.approved", 1, 20, "inventory-group");

        Assert.Equal("payment.approved", message.Topic);
        Assert.Equal(1, message.Partition);
        Assert.Equal(20, message.Offset);
        Assert.Equal("inventory-group", message.ConsumerGroup);
        Assert.NotEqual(Guid.Empty, message.Id);
    }
}
