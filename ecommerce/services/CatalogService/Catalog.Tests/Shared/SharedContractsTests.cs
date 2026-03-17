using ECommerce.Shared.Contracts;

namespace Catalog.Tests.Shared;

public class PaginationRequestTests
{
    [Fact]
    public void PageNumber_ShouldDefaultToOne_WhenValueIsInvalid()
    {
        var request = new PaginationRequest
        {
            PageNumber = 0,
            PageSize = -10
        };

        Assert.Equal(PaginationRequest.DefaultPageNumber, request.PageNumber);
        Assert.Equal(PaginationRequest.DefaultPageSize, request.PageSize);
    }

    [Fact]
    public void PageSize_ShouldRespectMaximumLimit()
    {
        var request = new PaginationRequest
        {
            PageSize = PaginationRequest.MaxPageSize + 50
        };

        Assert.Equal(PaginationRequest.MaxPageSize, request.PageSize);
    }
}

public class PagedResultTests
{
    [Fact]
    public void Create_ShouldPopulatePaginationMetadata()
    {
        var result = PagedResult<int>.Create([1, 2, 3], pageNumber: 2, pageSize: 3, totalItems: 8);

        Assert.Equal(3, result.Items.Count);
        Assert.Equal(2, result.Pagination.PageNumber);
        Assert.Equal(3, result.Pagination.PageSize);
        Assert.Equal(8, result.Pagination.TotalItems);
        Assert.Equal(3, result.Pagination.TotalPages);
        Assert.True(result.Pagination.HasPreviousPage);
        Assert.True(result.Pagination.HasNextPage);
    }

    [Fact]
    public void Map_ShouldTransformItems_AndPreservePagination()
    {
        var source = PagedResult<int>.Create([1, 2], pageNumber: 1, pageSize: 2, totalItems: 5);

        var result = source.Map(number => $"item-{number}");

        Assert.Equal(["item-1", "item-2"], result.Items);
        Assert.Equal(source.Pagination.PageNumber, result.Pagination.PageNumber);
        Assert.Equal(source.Pagination.TotalItems, result.Pagination.TotalItems);
    }
}
