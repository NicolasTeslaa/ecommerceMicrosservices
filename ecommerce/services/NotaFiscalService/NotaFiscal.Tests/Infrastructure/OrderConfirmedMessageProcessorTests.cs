using Microsoft.EntityFrameworkCore;
using Moq;
using NotaFiscal.Application.Interfaces;
using NotaFiscal.Infrastructure.Messaging;
using NotaFiscal.Infrastructure.Persistence;
using NotaFiscal.Tests.Support;

namespace NotaFiscal.Tests.Infrastructure;

public class OrderConfirmedMessageProcessorTests
{
    [Fact]
    public async Task ProcessAsync_ShouldCreateInvoicePublishEventAndMarkMessageProcessed_WhenInvoiceDoesNotExist()
    {
        await using var context = CreateDbContext();
        var repository = new InvoiceRepository(context);
        var eventPublisher = new Mock<IInvoiceEventPublisher>();
        var factory = new Mock<IMockInvoiceFactory>();
        var integrationEvent = NotaFiscalTestData.CreateOrderConfirmedEvent();
        var createdInvoice = NotaFiscalTestData.CreateInvoice(integrationEvent.OrderId, integrationEvent.CustomerId);
        factory.Setup(item => item.Create(integrationEvent)).Returns(createdInvoice);
        var processor = new OrderConfirmedMessageProcessor(context, repository, eventPublisher.Object, factory.Object);

        await processor.ProcessAsync(integrationEvent, "order.confirmed", 0, 10, "nota-fiscal", CancellationToken.None);

        Assert.Equal(1, await context.Invoices.CountAsync());
        Assert.Equal(1, await context.ProcessedKafkaMessages.CountAsync());
        eventPublisher.Verify(item => item.PublishIssuedAsync(
            It.Is<NotaFiscal.Domain.Entities.Invoice>(invoice => invoice.OrderId == integrationEvent.OrderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ShouldReuseExistingInvoice_WhenOrderAlreadyHasInvoice()
    {
        await using var context = CreateDbContext();
        var repository = new InvoiceRepository(context);
        var eventPublisher = new Mock<IInvoiceEventPublisher>();
        var factory = new Mock<IMockInvoiceFactory>();
        var integrationEvent = NotaFiscalTestData.CreateOrderConfirmedEvent();
        var existingInvoice = NotaFiscalTestData.CreateInvoice(integrationEvent.OrderId, integrationEvent.CustomerId);
        await context.Invoices.AddAsync(existingInvoice);
        await context.SaveChangesAsync();
        var processor = new OrderConfirmedMessageProcessor(context, repository, eventPublisher.Object, factory.Object);

        await processor.ProcessAsync(integrationEvent, "order.confirmed", 0, 11, "nota-fiscal", CancellationToken.None);

        Assert.Equal(1, await context.Invoices.CountAsync());
        factory.Verify(item => item.Create(It.IsAny<ECommerce.Shared.Messaging.OrderConfirmedIntegrationEvent>()), Times.Never);
        eventPublisher.Verify(item => item.PublishIssuedAsync(
            It.Is<NotaFiscal.Domain.Entities.Invoice>(invoice => invoice.Id == existingInvoice.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ShouldDoNothing_WhenKafkaMessageWasAlreadyProcessed()
    {
        await using var context = CreateDbContext();
        var repository = new InvoiceRepository(context);
        var eventPublisher = new Mock<IInvoiceEventPublisher>();
        var factory = new Mock<IMockInvoiceFactory>();
        var integrationEvent = NotaFiscalTestData.CreateOrderConfirmedEvent();
        await context.ProcessedKafkaMessages.AddAsync(new NotaFiscal.Domain.Entities.ProcessedKafkaMessage("order.confirmed", 1, 12, "nota-fiscal"));
        await context.SaveChangesAsync();
        var processor = new OrderConfirmedMessageProcessor(context, repository, eventPublisher.Object, factory.Object);

        await processor.ProcessAsync(integrationEvent, "order.confirmed", 1, 12, "nota-fiscal", CancellationToken.None);

        Assert.Equal(0, await context.Invoices.CountAsync());
        eventPublisher.Verify(item => item.PublishIssuedAsync(It.IsAny<NotaFiscal.Domain.Entities.Invoice>(), It.IsAny<CancellationToken>()), Times.Never);
        factory.Verify(item => item.Create(It.IsAny<ECommerce.Shared.Messaging.OrderConfirmedIntegrationEvent>()), Times.Never);
    }

    private static NotaFiscalDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<NotaFiscalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new NotaFiscalDbContext(options);
    }
}
