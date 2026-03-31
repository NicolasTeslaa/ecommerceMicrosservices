using Microsoft.EntityFrameworkCore;
using ECommerce.Shared.Messaging;
using NotaFiscal.Application.Interfaces;
using NotaFiscal.Domain.Entities;
using NotaFiscal.Infrastructure.Persistence;

namespace NotaFiscal.Infrastructure.Messaging;

public class OrderConfirmedMessageProcessor
{
    private readonly NotaFiscalDbContext _dbContext;
    private readonly IInvoiceRepository _repository;
    private readonly IInvoiceEventPublisher _eventPublisher;
    private readonly IMockInvoiceFactory _mockInvoiceFactory;

    public OrderConfirmedMessageProcessor(
        NotaFiscalDbContext dbContext,
        IInvoiceRepository repository,
        IInvoiceEventPublisher eventPublisher,
        IMockInvoiceFactory mockInvoiceFactory)
    {
        _dbContext = dbContext;
        _repository = repository;
        _eventPublisher = eventPublisher;
        _mockInvoiceFactory = mockInvoiceFactory;
    }

    public async Task ProcessAsync(
        OrderConfirmedIntegrationEvent integrationEvent,
        string topic,
        int partition,
        long offset,
        string groupId,
        CancellationToken cancellationToken)
    {
        var alreadyProcessed = await _dbContext.ProcessedKafkaMessages.AnyAsync(
            item => item.Topic == topic
                && item.Partition == partition
                && item.Offset == offset,
            cancellationToken);

        if (alreadyProcessed)
            return;

        var invoice = await _repository.GetEntityByOrderIdAsync(integrationEvent.OrderId, cancellationToken);

        if (invoice is null)
        {
            invoice = _mockInvoiceFactory.Create(integrationEvent);
            await _repository.AddAsync(invoice, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        await _eventPublisher.PublishIssuedAsync(invoice, cancellationToken);

        await _dbContext.ProcessedKafkaMessages.AddAsync(
            new ProcessedKafkaMessage(topic, partition, offset, groupId),
            cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
