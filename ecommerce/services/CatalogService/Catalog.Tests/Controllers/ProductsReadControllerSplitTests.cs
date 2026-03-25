using Catalog.API.Read.Controllers;
using Catalog.Application.DTOs;
using Catalog.Application.Queries;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Catalog.Tests.Controllers;

public class ProductsReadControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ProductsReadController _controller;

    public ProductsReadControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ProductsReadController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WithApiResponse()
    {
        var productId = Guid.NewGuid();
        var productDto = new ProductDto
        {
            Id = productId,
            Name = "Mouse",
            Description = "Mouse sem fio",
            Price = 120m,
            Active = true,
            CategoryId = Guid.NewGuid()
        };

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetProductByIdQuery>(q => q.Id == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productDto);

        var result = await _controller.GetById(productId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ProductDto>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("Product retrieved successfully.", response.Message);
        Assert.NotNull(response.Data);
        Assert.Equal(productId, response.Data!.Id);
        Assert.Equal("Mouse", response.Data.Name);
        Assert.NotNull(response.Pagination);
        Assert.Equal(1, response.Pagination!.TotalItems);

        _mediatorMock.Verify(
            x => x.Send(It.Is<GetProductByIdQuery>(q => q.Id == productId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithApiResponse()
    {
        var query = new GetAllProductsQuery
        {
            PageNumber = 2,
            PageSize = 1
        };

        var products = new List<ProductDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Produto 1",
                Description = "Desc 1",
                Price = 100m,
                Active = true,
                CategoryId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Produto 2",
                Description = "Desc 2",
                Price = 200m,
                Active = true,
                CategoryId = Guid.NewGuid()
            }
        };

        var pagedResult = PagedResult<ProductDto>.Create(products.Take(1), query.PageNumber, query.PageSize, products.Count);

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetAllProductsQuery>(q => q.PageNumber == query.PageNumber && q.PageSize == query.PageSize), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result = await _controller.GetAll(query);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IReadOnlyCollection<ProductDto>>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("Products retrieved successfully.", response.Message);
        Assert.NotNull(response.Data);
        Assert.Single(response.Data!);
        Assert.NotNull(response.Pagination);
        Assert.Equal(query.PageNumber, response.Pagination!.PageNumber);
        Assert.Equal(query.PageSize, response.Pagination.PageSize);
        Assert.Equal(products.Count, response.Pagination.TotalItems);

        _mediatorMock.Verify(
            x => x.Send(It.Is<GetAllProductsQuery>(q => q.PageNumber == query.PageNumber && q.PageSize == query.PageSize), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
