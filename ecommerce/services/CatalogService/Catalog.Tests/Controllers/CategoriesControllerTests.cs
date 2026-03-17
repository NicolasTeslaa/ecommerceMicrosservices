using Catalog.API.Controllers;
using Catalog.API.Responses;
using Catalog.Application.Commands;
using Catalog.Application.DTOs;
using Catalog.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Catalog.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new CategoriesController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WithApiResponse()
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

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(CategoriesController.GetById), createdAtActionResult.ActionName);

        var response = Assert.IsType<ApiResponse<Guid>>(createdAtActionResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Category created successfully.", response.Message);
        Assert.Equal(categoryId, response.Data);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WithApiResponse()
    {
        var categoryId = Guid.NewGuid();
        var category = new CategoryDto
        {
            Id = categoryId,
            Name = "Hardware"
        };

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetCategoryByIdQuery>(q => q.Id == categoryId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _controller.GetById(categoryId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CategoryDto>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("Category retrieved successfully.", response.Message);
        Assert.Equal(categoryId, response.Data!.Id);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithApiResponse()
    {
        var categories = new List<CategoryDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Hardware" },
            new() { Id = Guid.NewGuid(), Name = "Monitores" }
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetAllCategoriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<CategoryDto>>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("Categories retrieved successfully.", response.Message);
        Assert.Equal(2, response.Data!.Count());
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
