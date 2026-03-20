using Customer.Domain.Entities;

namespace Customer.Tests.Domain;

public class ProcessedKafkaMessageTests
{
    [Fact]
    public void Constructor_ShouldCreateProcessedMessage_WhenDataIsValid()
    {
        var message = new ProcessedKafkaMessage("topic", 1, 10, "group", "key", "type");

        Assert.NotEqual(Guid.Empty, message.Id);
        Assert.Equal("topic", message.Topic);
        Assert.Equal(1, message.Partition);
        Assert.Equal(10, message.Offset);
        Assert.Equal("group", message.ConsumerGroup);
        Assert.Equal("key", message.MessageKey);
        Assert.Equal("type", message.MessageType);
    }

    [Fact]
    public void Constructor_ShouldTrimTextFields()
    {
        var message = new ProcessedKafkaMessage(" topic ", 1, 10, " group ", " key ", " type ");

        Assert.Equal("topic", message.Topic);
        Assert.Equal("group", message.ConsumerGroup);
        Assert.Equal("key", message.MessageKey);
        Assert.Equal("type", message.MessageType);
    }

    [Fact]
    public void Constructor_ShouldAllowNullMessageKey_AndFallbackToEmptyString()
    {
        var message = new ProcessedKafkaMessage("topic", 1, 10, "group", null!, "type");

        Assert.Equal(string.Empty, message.MessageKey);
    }
}
