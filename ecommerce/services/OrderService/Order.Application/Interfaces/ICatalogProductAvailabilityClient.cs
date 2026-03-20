using Order.Application.DTOs;

namespace Order.Application.Interfaces;

public interface ICatalogProductAvailabilityClient
{
    Task<ProductAvailabilityValidationResultDto> ValidateAsync(
        IReadOnlyCollection<ProductAvailabilityCheckItemDto> items,
        CancellationToken cancellationToken = default);
}
