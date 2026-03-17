namespace ECommerce.Shared.Contracts;

public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();
    public PaginationMetadata Pagination { get; init; } = new();

    public static PagedResult<T> Create(IEnumerable<T> items, int pageNumber, int pageSize, int totalItems)
    {
        var normalizedItems = items.ToArray();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResult<T>
        {
            Items = normalizedItems,
            Pagination = new PaginationMetadata
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            }
        };
    }

    public PagedResult<TResult> Map<TResult>(Func<T, TResult> map)
    {
        return new PagedResult<TResult>
        {
            Items = Items.Select(map).ToArray(),
            Pagination = Pagination
        };
    }
}
