using Order.Domain.Entities;

namespace Order.Tests.Domain;

public class OrderProcessingOutboxMessageTests
{
    [Fact]
    public void Create_ShouldPopulateFields_WhenDataIsValid()
    {
        var requestedAtUtc = DateTime.UtcNow;

        var message = OrderProcessingOutboxMessage.Create(
            Guid.NewGuid(),
            "order.processing.requested",
            "OrderProcessingRequestDto",
            "{\"orderId\":\"1\"}",
            requestedAtUtc);

        Assert.NotEqual(Guid.Empty, message.Id);
        Assert.Equal("order.processing.requested", message.Topic);
        Assert.Equal("OrderProcessingRequestDto", message.Type);
        Assert.Equal("{\"orderId\":\"1\"}", message.Payload);
        Assert.Equal(requestedAtUtc, message.RequestedAtUtc);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenOrderIdIsEmpty()
    {
        var act = () => OrderProcessingOutboxMessage.Create(Guid.Empty, "topic", "type", "payload", DateTime.UtcNow);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenTopicIsEmpty()
    {
        var act = () => OrderProcessingOutboxMessage.Create(Guid.NewGuid(), string.Empty, "type", "payload", DateTime.UtcNow);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenTypeIsEmpty()
    {
        var act = () => OrderProcessingOutboxMessage.Create(Guid.NewGuid(), "topic", string.Empty, "payload", DateTime.UtcNow);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenPayloadIsEmpty()
    {
        var act = () => OrderProcessingOutboxMessage.Create(Guid.NewGuid(), "topic", "type", string.Empty, DateTime.UtcNow);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void MarkDispatchAttempt_ShouldSetTimestamp()
    {
        var message = CreateMessage();

        message.MarkDispatchAttempt();

        Assert.NotNull(message.LastDispatchAttemptAtUtc);
    }

    [Fact]
    public void MarkAsDispatched_ShouldSetDispatchedAtAndClearError()
    {
        var message = CreateMessage();
        message.RegisterDispatchFailure("erro");

        message.MarkAsDispatched();

        Assert.NotNull(message.DispatchedAtUtc);
        Assert.Null(message.LastDispatchError);
    }

    [Fact]
    public void MarkAsProcessed_ShouldSetProcessedAtAndClearError()
    {
        var message = CreateMessage();
        message.RegisterProcessingFailure("erro");

        message.MarkAsProcessed();

        Assert.NotNull(message.ProcessedAtUtc);
        Assert.Null(message.LastProcessingError);
    }

    [Fact]
    public void RegisterDispatchFailure_ShouldIncrementAttemptsAndPersistMessage()
    {
        var message = CreateMessage();

        message.RegisterDispatchFailure("falha de envio");

        Assert.Equal(1, message.DispatchAttempts);
        Assert.Equal("falha de envio", message.LastDispatchError);
    }

    [Fact]
    public void RegisterDispatchFailure_ShouldUseFallbackMessage_WhenErrorIsEmpty()
    {
        var message = CreateMessage();

        message.RegisterDispatchFailure(string.Empty);

        Assert.Equal("Unknown dispatch error.", message.LastDispatchError);
    }

    [Fact]
    public void RegisterProcessingFailure_ShouldIncrementAttemptsAndPersistMessage()
    {
        var message = CreateMessage();

        message.RegisterProcessingFailure("falha de processamento");

        Assert.Equal(1, message.ProcessingAttempts);
        Assert.Equal("falha de processamento", message.LastProcessingError);
    }

    [Fact]
    public void RegisterProcessingFailure_ShouldUseFallbackMessage_WhenErrorIsEmpty()
    {
        var message = CreateMessage();

        message.RegisterProcessingFailure(string.Empty);

        Assert.Equal("Unknown processing error.", message.LastProcessingError);
    }

    [Fact]
    public void RegisterProcessingFailure_ShouldTruncateLongError_WhenErrorExceeds4000Characters()
    {
        var message = CreateMessage();
        var longMessage = new string('x', 5000);

        message.RegisterProcessingFailure(longMessage);

        Assert.Equal(4000, message.LastProcessingError!.Length);
    }

    private static OrderProcessingOutboxMessage CreateMessage()
    {
        return OrderProcessingOutboxMessage.Create(
            Guid.NewGuid(),
            "topic",
            "type",
            "payload",
            DateTime.UtcNow);
    }
}
