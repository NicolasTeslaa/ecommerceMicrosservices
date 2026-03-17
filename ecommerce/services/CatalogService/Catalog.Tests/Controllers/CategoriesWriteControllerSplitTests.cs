using Catalog.API.Write.Controllers;
using Catalog.Application.Commands;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Catalog.Tests.Controllers;

public class CategoriesWriteControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CategoriesWriteController _controller;

    public CategoriesWriteControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new CategoriesWriteController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnAccepted_WithApiResponse()
    {
        var categoryId = Guid.NewGuid();
        var command = new CreateCategoryCommand
        {
            Name = "Hardware"
        };

        _mediatorMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryId);

        var result = await _controller.Create(command);

        var acceptedResult = Assert.IsType<AcceptedResult>(result.Result);
        var response = Assert.IsType<ApiResponse<Guid>>(acceptedResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Category created successfully.", response.Message);
        Assert.Equal(categoryId, response.Data);
    }

    [Fact]
    public async Task Update_ShouldSetCommandId_AndReturnOk_WithApiResponse()
    {
        var categoryId = Guid.NewGuid();
        var command = new UpdateCategoryCommand
        {
            Name = "Monitores"
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryId);

        var result = await _controller.Update(categoryId, command);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<Guid>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("Category updated successfully.", response.Message);
        Assert.Equal(categoryId, response.Data);
        Assert.Equal(categoryId, command.Id);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WithApiResponse()
    {
        var categoryId = Guid.NewGuid();

        _mediatorMock
            .Setup(x => x.Send(It.Is<DeleteCategoryCommand>(c => c.Id == categoryId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryId);

        var result = await _controller.Delete(categoryId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<Guid>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("Category deleted successfully.", response.Message);
        Assert.Equal(categoryId, response.Data);
    }
}
