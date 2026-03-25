using Order.Application.DTOs;

namespace Order.Application.Interfaces;

public interface IInventoryOrderReservationClient
{
    Task<ProductAvailabilityValidationResultDto> ReserveAsync(
        Guid orderId,
        Guid customerId,
        IReadOnlyCollection<ProductAvailabilityCheckItemDto> items,
        CancellationToken cancellationToken = default);

    Task ReleaseAsync(Guid orderId, CancellationToken cancellationToken = default);
}
