using Catalog.API.Read.Controllers;
using Catalog.Application.DTOs;
using Catalog.Application.Queries;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Catalog.Tests.Controllers;

public class CategoriesReadControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CategoriesReadController _controller;

    public CategoriesReadControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new CategoriesReadController(_mediatorMock.Object);
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
        Assert.NotNull(response.Pagination);
        Assert.Equal(1, response.Pagination!.TotalItems);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithApiResponse()
    {
        var query = new GetAllCategoriesQuery
        {
            PageNumber = 1,
            PageSize = 1
        };

        var categories = new List<CategoryDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Hardware" },
            new() { Id = Guid.NewGuid(), Name = "Monitores" }
        };

        var pagedResult = PagedResult<CategoryDto>.Create(categories.Take(1), query.PageNumber, query.PageSize, categories.Count);

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetAllCategoriesQuery>(q => q.PageNumber == query.PageNumber && q.PageSize == query.PageSize), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result = await _controller.GetAll(query);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IReadOnlyCollection<CategoryDto>>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("Categories retrieved successfully.", response.Message);
        Assert.Single(response.Data!);
        Assert.NotNull(response.Pagination);
        Assert.Equal(query.PageNumber, response.Pagination!.PageNumber);
        Assert.Equal(query.PageSize, response.Pagination.PageSize);
        Assert.Equal(categories.Count, response.Pagination.TotalItems);
    }
}
