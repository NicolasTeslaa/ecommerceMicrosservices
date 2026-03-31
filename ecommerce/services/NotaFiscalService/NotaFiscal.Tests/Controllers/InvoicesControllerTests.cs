using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotaFiscal.API.Controllers;
using NotaFiscal.Application.DTOs;
using NotaFiscal.Application.Queries;

namespace NotaFiscal.Tests.Controllers;

public class InvoicesControllerTests
{
    [Fact]
    public async Task GetByOrderId_ShouldReturnNotFound_WhenInvoiceDoesNotExist()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.IsAny<GetInvoiceByOrderIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InvoiceDto?)null);

        var controller = new InvoicesController(mediator.Object);

        var result = await controller.GetByOrderId(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByOrderId_ShouldReturnOk_WhenInvoiceExists()
    {
        var invoice = new InvoiceDto { OrderId = Guid.NewGuid(), Number = 321, Status = "Issued" };
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.IsAny<GetInvoiceByOrderIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var controller = new InvoicesController(mediator.Object);

        var result = await controller.GetByOrderId(invoice.OrderId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<InvoiceDto>>(okResult.Value);
        Assert.Equal(invoice.OrderId, response.Data.OrderId);
    }
}
