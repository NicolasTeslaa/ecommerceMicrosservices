using System.Text.Json;
using Confluent.Kafka;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotaFiscal.Application.Interfaces;
using NotaFiscal.Domain.Entities;

namespace NotaFiscal.Infrastructure.Messaging;

public class KafkaInvoiceEventPublisher : IInvoiceEventPublisher
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaInvoiceEventPublisher> _logger;

    public KafkaInvoiceEventPublisher(IConfiguration configuration, ILogger<KafkaInvoiceEventPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishIssuedAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            _logger.LogWarning("Invoice issued event was not published because Kafka:BootstrapServers was not configured.");
            return;
        }

        var integrationEvent = new InvoiceIssuedIntegrationEvent
        {
            InvoiceId = invoice.Id,
            OrderId = invoice.OrderId,
            CustomerId = invoice.CustomerId,
            Number = invoice.Number,
            Series = invoice.Series,
            AccessKey = invoice.AccessKey,
            Status = invoice.Status.ToString(),
            TotalAmount = invoice.TotalAmount,
            Currency = invoice.Currency,
            IssuedAtUtc = invoice.IssuedAtUtc
        };

        try
        {
            using var producer = new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            }).Build();

            await producer.ProduceAsync(
                _configuration["Kafka:InvoiceIssuedTopic"] ?? "invoice.issued",
                new Message<string, string>
                {
                    Key = invoice.OrderId.ToString(),
                    Value = JsonSerializer.Serialize(integrationEvent)
                },
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to publish invoice.issued event for invoice '{InvoiceId}'.", invoice.Id);
        }
    }
}
