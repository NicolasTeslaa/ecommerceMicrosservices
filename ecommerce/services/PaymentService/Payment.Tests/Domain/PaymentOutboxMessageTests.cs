using Payment.Domain.Entities;

namespace Payment.Tests.Domain;

public class PaymentOutboxMessageTests
{
    [Fact]
    public void Create_ShouldCreateOutboxMessage_WhenDataIsValid()
    {
        var message = PaymentOutboxMessage.Create(Guid.NewGuid(), "payment.approved", "key-1", "PaymentApproved", "{\"ok\":true}");

        Assert.Equal("payment.approved", message.Topic);
        Assert.Equal("key-1", message.Key);
        Assert.Equal("PaymentApproved", message.Type);
        Assert.Equal("{\"ok\":true}", message.Payload);
        Assert.Equal(0, message.PublishAttempts);
        Assert.Null(message.PublishedAtUtc);
    }

    [Fact]
    public void Create_ShouldThrow_WhenPaymentIdIsEmpty()
    {
        var act = () => PaymentOutboxMessage.Create(Guid.Empty, "topic", "key", "type", "payload");

        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("", "type", "payload")]
    [InlineData("topic", "", "payload")]
    [InlineData("topic", "type", "")]
    public void Create_ShouldThrow_WhenRequiredTextFieldsAreMissing(string topic, string type, string payload)
    {
        var act = () => PaymentOutboxMessage.Create(Guid.NewGuid(), topic, "key", type, payload);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void MarkAsPublished_ShouldSetPublishedAt_AndClearLastError()
    {
        var message = PaymentOutboxMessage.Create(Guid.NewGuid(), "topic", "key", "type", "payload");
        message.RegisterPublishFailure("oops");

        message.MarkAsPublished();

        Assert.NotNull(message.PublishedAtUtc);
        Assert.Null(message.LastError);
    }

    [Fact]
    public void RegisterPublishFailure_ShouldIncrementAttempts_AndStoreError()
    {
        var message = PaymentOutboxMessage.Create(Guid.NewGuid(), "topic", "key", "type", "payload");

        message.RegisterPublishFailure("failure");

        Assert.Equal(1, message.PublishAttempts);
        Assert.Equal("failure", message.LastError);
    }

    [Fact]
    public void RegisterPublishFailure_ShouldUseFallbackMessage_WhenErrorIsBlank()
    {
        var message = PaymentOutboxMessage.Create(Guid.NewGuid(), "topic", "key", "type", "payload");

        message.RegisterPublishFailure(" ");

        Assert.Equal("Unknown publish error.", message.LastError);
    }

    [Fact]
    public void RegisterPublishFailure_ShouldTruncateLongError()
    {
        var message = PaymentOutboxMessage.Create(Guid.NewGuid(), "topic", "key", "type", "payload");
        var longError = new string('x', 5000);

        message.RegisterPublishFailure(longError);

        Assert.Equal(4000, message.LastError!.Length);
    }
}
