using Payment.Domain.Entities;

namespace Payment.Tests.Domain;

public class ProcessedKafkaMessageTests
{
    [Fact]
    public void Constructor_ShouldTrimTextFields_AndSetTimestamp()
    {
        var message = new ProcessedKafkaMessage(" topic ", 1, 22, " group ", " key ", " type ");

        Assert.Equal("topic", message.Topic);
        Assert.Equal("group", message.ConsumerGroup);
        Assert.Equal("key", message.MessageKey);
        Assert.Equal("type", message.MessageType);
        Assert.Equal(1, message.Partition);
        Assert.Equal(22, message.Offset);
        Assert.NotEqual(default, message.ProcessedAtUtc);
    }

    [Fact]
    public void Constructor_ShouldAllowNullMessageKey()
    {
        var message = new ProcessedKafkaMessage("topic", 0, 1, "group", null!, "type");

        Assert.Equal(string.Empty, message.MessageKey);
    }
}
