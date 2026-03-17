namespace ECommerce.Shared.Contracts;

public class PaginationMetadata
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginationMetadata SingleItem()
    {
        return new PaginationMetadata
        {
            PageNumber = 1,
            PageSize = 1,
            TotalItems = 1,
            TotalPages = 1
        };
    }
}
