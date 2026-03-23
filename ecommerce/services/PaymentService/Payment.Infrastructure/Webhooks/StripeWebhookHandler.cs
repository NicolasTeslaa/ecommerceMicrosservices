using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Configuration;
using Payment.Infrastructure.Persistence;
using Stripe;

namespace Payment.Infrastructure.Webhooks;

public class StripeWebhookHandler : IStripeWebhookHandler
{
    private readonly IPaymentRepository _repository;
    private readonly IPaymentEventPublisher _eventPublisher;
    private readonly PaymentDbContext _dbContext;
    private readonly StripeOptions _options;
    private readonly ILogger<StripeWebhookHandler> _logger;
    private readonly IPaymentRealtimeNotifier _realtimeNotifier;

    public StripeWebhookHandler(
        IPaymentRepository repository,
        IPaymentEventPublisher eventPublisher,
        PaymentDbContext dbContext,
        Microsoft.Extensions.Options.IOptions<StripeOptions> options,
        ILogger<StripeWebhookHandler> logger,
        IPaymentRealtimeNotifier realtimeNotifier)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _dbContext = dbContext;
        _options = options.Value;
        _logger = logger;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task HandleAsync(string jsonPayload, string? signatureHeader, CancellationToken cancellationToken = default)
    {
        var stripeEvent = !string.IsNullOrWhiteSpace(_options.WebhookSecret)
            ? EventUtility.ConstructEvent(jsonPayload, signatureHeader, _options.WebhookSecret)
            : EventUtility.ParseEvent(jsonPayload);

        if (string.IsNullOrWhiteSpace(stripeEvent.Id))
            return;

        var alreadyProcessed = await _dbContext.ProcessedStripeWebhookEvents
            .AnyAsync(item => item.EventId == stripeEvent.Id, cancellationToken);

        if (alreadyProcessed)
        {
            _logger.LogInformation("Stripe webhook event '{EventId}' was already processed.", stripeEvent.Id);
            return;
        }

        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
            return;

        var payment = await _repository.GetByStripePaymentIntentIdAsync(paymentIntent.Id, cancellationToken);

        if (payment is null)
        {
            _logger.LogWarning("Stripe webhook received for unknown PaymentIntent '{PaymentIntentId}'.", paymentIntent.Id);
            return;
        }

        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                if (payment.Status != PaymentStatus.Approved)
                {
                    payment.MarkApproved();
                }

                await _eventPublisher.PublishApprovedAsync(payment, cancellationToken);
                break;

            case "payment_intent.payment_failed":
                var failureReason = MapFailureReason(paymentIntent.LastPaymentError?.Code, paymentIntent.LastPaymentError?.DeclineCode);
                var declineMessage = TranslateFailureDetail(
                    paymentIntent.LastPaymentError?.Code,
                    paymentIntent.LastPaymentError?.DeclineCode,
                    paymentIntent.LastPaymentError?.Message);

                if (payment.Status != PaymentStatus.Failed
                    || payment.FailureReason != failureReason
                    || !string.Equals(payment.FailureDetail, declineMessage, StringComparison.Ordinal))
                {
                    payment.MarkFailed(failureReason, declineMessage);
                }

                await _eventPublisher.PublishFailedAsync(payment, cancellationToken);
                break;
        }

        await _dbContext.ProcessedStripeWebhookEvents.AddAsync(
            new ProcessedStripeWebhookEvent(stripeEvent.Id, stripeEvent.Type),
            cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _realtimeNotifier.NotifyUpdatedAsync(payment.OrderId, cancellationToken);
    }

    private static PaymentFailureReason MapFailureReason(string? code, string? declineCode)
    {
        return (declineCode ?? code) switch
        {
            "authentication_required" => PaymentFailureReason.RequiresCustomerAction,
            "insufficient_funds" => PaymentFailureReason.CardDeclined,
            "card_declined" => PaymentFailureReason.CardDeclined,
            _ => PaymentFailureReason.ProcessorError
        };
    }

    private static string TranslateFailureDetail(string? code, string? declineCode, string? message)
    {
        var normalizedCode = declineCode ?? code;

        return normalizedCode switch
        {
            "insufficient_funds" => "Seu cartao nao tem saldo suficiente.",
            "expired_card" => "Seu cartao esta vencido.",
            "incorrect_cvc" => "O codigo de seguranca do cartao esta incorreto.",
            "authentication_required" => "Seu banco exige uma etapa adicional de autenticacao para concluir o pagamento.",
            "processing_error" => "Nao foi possivel processar o pagamento agora. Tente novamente em alguns instantes.",
            "card_declined" => "O pagamento foi recusado pela operadora do cartao.",
            _ => TranslateKnownMessage(message)
        };
    }

    private static string TranslateKnownMessage(string? message)
    {
        return message switch
        {
            null or "" => "Nao foi possivel concluir o pagamento.",
            "Your card has insufficient funds." => "Seu cartao nao tem saldo suficiente.",
            "Your card was declined." => "O pagamento foi recusado pela operadora do cartao.",
            "Your card has expired." => "Seu cartao esta vencido.",
            "Your card's security code is incorrect." => "O codigo de seguranca do cartao esta incorreto.",
            _ => message
        };
    }
}
