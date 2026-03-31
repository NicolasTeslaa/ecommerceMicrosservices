using System.Text.Json;
using Confluent.Kafka;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Configuration;
using NotaFiscal.Application.Interfaces;
using NotaFiscal.Domain.Entities;

namespace NotaFiscal.Infrastructure.Messaging;

public class KafkaInvoiceEventPublisher : IInvoiceEventPublisher
{
    private readonly IConfiguration _configuration;

    public KafkaInvoiceEventPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task PublishIssuedAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
            throw new InvalidOperationException("Kafka:BootstrapServers was not configured for NotaFiscalService.");

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
}
