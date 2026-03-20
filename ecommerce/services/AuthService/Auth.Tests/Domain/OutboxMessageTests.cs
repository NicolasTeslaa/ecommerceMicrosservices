using Auth.Domain.Entities;

namespace Auth.Tests.Domain;

public class OutboxMessageTests
{
    [Fact]
    public void Create_ShouldPopulateFields_WhenDataIsValid()
    {
        var message = OutboxMessage.Create("auth.user-registered", "key-1", "UserRegisteredIntegrationEvent", "{\"id\":1}");

        Assert.NotEqual(Guid.Empty, message.Id);
        Assert.Equal("auth.user-registered", message.Topic);
        Assert.Equal("key-1", message.Key);
        Assert.Equal("UserRegisteredIntegrationEvent", message.Type);
        Assert.Equal("{\"id\":1}", message.Payload);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenTopicIsEmpty()
    {
        var act = () => OutboxMessage.Create("", "key", "type", "payload");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenTypeIsEmpty()
    {
        var act = () => OutboxMessage.Create("topic", "key", "", "payload");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenPayloadIsEmpty()
    {
        var act = () => OutboxMessage.Create("topic", "key", "type", "");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void MarkAsPublished_ShouldSetPublishedAtAndClearError()
    {
        var message = OutboxMessage.Create("topic", "key", "type", "payload");
        message.RegisterPublishFailure("erro");

        message.MarkAsPublished();

        Assert.NotNull(message.PublishedAtUtc);
        Assert.Null(message.LastError);
    }

    [Fact]
    public void RegisterPublishFailure_ShouldIncrementAttemptsAndStoreError()
    {
        var message = OutboxMessage.Create("topic", "key", "type", "payload");

        message.RegisterPublishFailure("falha");

        Assert.Equal(1, message.PublishAttempts);
        Assert.Equal("falha", message.LastError);
    }

    [Fact]
    public void RegisterPublishFailure_ShouldUseFallbackError_WhenMessageIsEmpty()
    {
        var message = OutboxMessage.Create("topic", "key", "type", "payload");

        message.RegisterPublishFailure("");

        Assert.Equal("Unknown publish error.", message.LastError);
    }

    [Fact]
    public void RegisterPublishFailure_ShouldTruncateLongError_WhenMessageIsLongerThan4000()
    {
        var message = OutboxMessage.Create("topic", "key", "type", "payload");
        var longError = new string('x', 5000);

        message.RegisterPublishFailure(longError);

        Assert.Equal(4000, message.LastError!.Length);
    }
}
