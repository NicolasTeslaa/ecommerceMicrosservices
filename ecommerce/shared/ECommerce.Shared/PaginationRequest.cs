namespace ECommerce.Shared.Contracts;

public class PaginationRequest
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    private int _pageNumber = DefaultPageNumber;
    private int _pageSize = DefaultPageSize;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value <= 0 ? DefaultPageNumber : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value <= 0)
            {
                _pageSize = DefaultPageSize;
                return;
            }

            _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
