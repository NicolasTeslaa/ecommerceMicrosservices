using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Inventory.API.Controllers;
using Inventory.Application.DTOs;
using Inventory.Application.Queries;
using Inventory.Tests.Support;

namespace Inventory.Tests.Controllers;

public class InventoryControllerTests
{
    [Fact]
    public async Task GetByProductId_ShouldReturnOkResponse()
    {
        var mediator = new Mock<IMediator>();
        var productId = Guid.NewGuid();
        mediator.Setup(item => item.Send(It.IsAny<GetInventoryAvailabilityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryTestData.CreateAvailabilityDto(productId));
        var controller = new InventoryController(mediator.Object);

        var result = await controller.GetByProductId(productId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<InventoryAvailabilityDto?>>(ok.Value);
        Assert.Equal(productId, response.Data!.ProductId);
    }

    [Fact]
    public async Task GetBatch_ShouldReturnOkResponse()
    {
        var mediator = new Mock<IMediator>();
        var productIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        mediator.Setup(item => item.Send(It.IsAny<GetInventoryAvailabilityBatchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productIds.Select(productId => InventoryTestData.CreateAvailabilityDto(productId)).ToArray());
        var controller = new InventoryController(mediator.Object);

        var result = await controller.GetBatch(new GetInventoryAvailabilityBatchRequest { ProductIds = productIds });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IReadOnlyCollection<InventoryAvailabilityDto>>>(ok.Value);
        Assert.Equal(2, response.Data.Count);
    }
}
