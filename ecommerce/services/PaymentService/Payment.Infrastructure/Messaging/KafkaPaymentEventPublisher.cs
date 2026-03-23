using System.Text.Json;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Configuration;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Messaging;

public class KafkaPaymentEventPublisher : IPaymentEventPublisher
{
    private readonly IConfiguration _configuration;
    private readonly PaymentDbContext _dbContext;

    public KafkaPaymentEventPublisher(IConfiguration configuration, PaymentDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }

    public async Task PublishApprovedAsync(Payment.Domain.Entities.Payment payment, CancellationToken cancellationToken = default)
    {
        var topic = _configuration["Kafka:PaymentApprovedTopic"] ?? "payment.approved";
        var integrationEvent = new PaymentApprovedIntegrationEvent
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            StripePaymentIntentId = payment.StripePaymentIntentId ?? string.Empty,
            ApprovedAtUtc = DateTime.UtcNow
        };

        await QueueAsync(payment.Id, topic, payment.OrderId.ToString(), integrationEvent, cancellationToken);
    }

    public async Task PublishFailedAsync(Payment.Domain.Entities.Payment payment, CancellationToken cancellationToken = default)
    {
        var topic = _configuration["Kafka:PaymentFailedTopic"] ?? "payment.failed";
        var integrationEvent = new PaymentFailedIntegrationEvent
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            StripePaymentIntentId = payment.StripePaymentIntentId,
            FailureReason = payment.FailureReason?.ToString() ?? "Unknown",
            FailureDetail = payment.FailureDetail ?? "Payment failed.",
            AttemptCount = payment.AttemptCount,
            MaxAttemptsReached = payment.HasReachedMaxAttempts,
            FailedAtUtc = DateTime.UtcNow
        };

        await QueueAsync(payment.Id, topic, payment.OrderId.ToString(), integrationEvent, cancellationToken);
    }

    private async Task QueueAsync(Guid paymentId, string topic, string key, object payload, CancellationToken cancellationToken)
    {
        var message = PaymentOutboxMessage.Create(
            paymentId,
            topic,
            key,
            payload.GetType().Name,
            JsonSerializer.Serialize(payload));

        await _dbContext.PaymentOutboxMessages.AddAsync(message, cancellationToken);
    }
}
