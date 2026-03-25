using Inventory.Application.DTOs;

namespace Inventory.Application.Interfaces;

public interface IInventoryEventPublisher
{
    Task PublishReservationRejectedAsync(
        Guid orderId,
        Guid customerId,
        string reason,
        IReadOnlyCollection<InventoryReservationIssueDto> issues,
        CancellationToken cancellationToken = default);
}
