using NotaFiscal.Domain.Entities;

namespace NotaFiscal.Tests.Domain;

public class ProcessedKafkaMessageTests
{
    [Fact]
    public void Constructor_ShouldCaptureKafkaMetadata()
    {
        var message = new ProcessedKafkaMessage("order.confirmed", 2, 15, "nota-fiscal-group");

        Assert.Equal("order.confirmed", message.Topic);
        Assert.Equal(2, message.Partition);
        Assert.Equal(15, message.Offset);
        Assert.Equal("nota-fiscal-group", message.ConsumerGroup);
        Assert.NotEqual(Guid.Empty, message.Id);
    }
}
