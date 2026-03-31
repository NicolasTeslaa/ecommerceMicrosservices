using Moq;
using NotaFiscal.Application.DTOs;
using NotaFiscal.Application.Handlers;
using NotaFiscal.Application.Interfaces;
using NotaFiscal.Application.Queries;

namespace NotaFiscal.Tests.Handlers;

public class GetInvoiceByOrderIdHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnInvoice_WhenRepositoryFindsOne()
    {
        var repository = new Mock<IInvoiceRepository>();
        var orderId = Guid.NewGuid();
        repository.Setup(item => item.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvoiceDto { OrderId = orderId, Number = 123, Status = "Issued" });

        var handler = new GetInvoiceByOrderIdHandler(repository.Object);

        var result = await handler.Handle(new GetInvoiceByOrderIdQuery(orderId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(orderId, result!.OrderId);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenRepositoryDoesNotFindInvoice()
    {
        var repository = new Mock<IInvoiceRepository>();
        var handler = new GetInvoiceByOrderIdHandler(repository.Object);

        var result = await handler.Handle(new GetInvoiceByOrderIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
