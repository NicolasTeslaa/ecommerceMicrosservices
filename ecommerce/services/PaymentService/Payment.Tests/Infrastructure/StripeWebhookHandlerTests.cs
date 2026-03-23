using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Payment.Application.Interfaces;
using Payment.Domain.Enums;
using Payment.Infrastructure.Configuration;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Webhooks;
using Payment.Tests.Support;

namespace Payment.Tests.Infrastructure;

public class StripeWebhookHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldApprovePayment_WhenSucceededEventArrives()
    {
        await using var context = CreateDbContext();
        var payment = PaymentTestData.CreatePaymentWithIntent();
        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();

        var publisher = new Mock<IPaymentEventPublisher>();
        var notifier = new Mock<IPaymentRealtimeNotifier>();
        var handler = CreateHandler(context, publisher.Object, notifier.Object);

        await handler.HandleAsync(CreateSucceededEventJson("evt_success_1", payment.StripePaymentIntentId!), null);

        Assert.Equal(PaymentStatus.Approved, payment.Status);
        Assert.Single(context.ProcessedStripeWebhookEvents);
        publisher.Verify(item => item.PublishApprovedAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
        notifier.Verify(item => item.NotifyUpdatedAsync(payment.OrderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldFailPayment_WhenFailureEventArrives()
    {
        await using var context = CreateDbContext();
        var payment = PaymentTestData.CreatePaymentWithIntent();
        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();

        var publisher = new Mock<IPaymentEventPublisher>();
        var handler = CreateHandler(context, publisher.Object, Mock.Of<IPaymentRealtimeNotifier>());

        await handler.HandleAsync(CreateFailedEventJson("evt_failed_1", payment.StripePaymentIntentId!, "insufficient_funds", "Your card has insufficient funds."), null);

        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal(PaymentFailureReason.CardDeclined, payment.FailureReason);
        Assert.Equal("Seu cartao nao tem saldo suficiente.", payment.FailureDetail);
        Assert.Equal(1, payment.AttemptCount);
        publisher.Verify(item => item.PublishFailedAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldIgnoreDuplicateWebhookEvents()
    {
        await using var context = CreateDbContext();
        var payment = PaymentTestData.CreatePaymentWithIntent();
        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();

        var publisher = new Mock<IPaymentEventPublisher>();
        var handler = CreateHandler(context, publisher.Object, Mock.Of<IPaymentRealtimeNotifier>());
        var json = CreateSucceededEventJson("evt_duplicate", payment.StripePaymentIntentId!);

        await handler.HandleAsync(json, null);
        await handler.HandleAsync(json, null);

        Assert.Single(context.ProcessedStripeWebhookEvents);
        publisher.Verify(item => item.PublishApprovedAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldIgnoreUnknownPaymentIntent()
    {
        await using var context = CreateDbContext();
        var publisher = new Mock<IPaymentEventPublisher>();
        var handler = CreateHandler(context, publisher.Object, Mock.Of<IPaymentRealtimeNotifier>());

        await handler.HandleAsync(CreateSucceededEventJson("evt_unknown", "pi_unknown"), null);

        Assert.Empty(context.ProcessedStripeWebhookEvents);
        publisher.VerifyNoOtherCalls();
    }

    private static StripeWebhookHandler CreateHandler(
        PaymentDbContext context,
        IPaymentEventPublisher publisher,
        IPaymentRealtimeNotifier notifier)
    {
        return new StripeWebhookHandler(
            new PaymentRepository(context),
            publisher,
            context,
            Options.Create(new StripeOptions()),
            Mock.Of<ILogger<StripeWebhookHandler>>(),
            notifier);
    }

    private static PaymentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PaymentDbContext(options);
    }

    private static string CreateSucceededEventJson(string eventId, string paymentIntentId)
    {
        return $$"""
        {
          "id": "{{eventId}}",
          "object": "event",
          "api_version": "2025-09-30.clover",
          "created": 1742745600,
          "livemode": false,
          "pending_webhooks": 1,
          "request": {
            "id": null,
            "idempotency_key": null
          },
          "type": "payment_intent.succeeded",
          "data": {
            "object": {
              "id": "{{paymentIntentId}}",
              "object": "payment_intent",
              "status": "succeeded",
              "amount": 12550,
              "currency": "brl"
            }
          }
        }
        """;
    }

    private static string CreateFailedEventJson(string eventId, string paymentIntentId, string declineCode, string message)
    {
        return $$"""
        {
          "id": "{{eventId}}",
          "object": "event",
          "api_version": "2025-09-30.clover",
          "created": 1742745600,
          "livemode": false,
          "pending_webhooks": 1,
          "request": {
            "id": null,
            "idempotency_key": null
          },
          "type": "payment_intent.payment_failed",
          "data": {
            "object": {
              "id": "{{paymentIntentId}}",
              "object": "payment_intent",
              "status": "requires_payment_method",
              "amount": 12550,
              "currency": "brl",
              "last_payment_error": {
                "code": "card_declined",
                "decline_code": "{{declineCode}}",
                "message": "{{message}}"
              }
            }
          }
        }
        """;
    }
}
