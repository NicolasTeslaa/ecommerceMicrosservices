using Inventory.Application.DTOs;
using Inventory.Domain.Entities;

namespace Inventory.Application.Interfaces;

public interface IInventoryRepository
{
    Task<InventoryItem?> GetItemByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryItem>> GetItemsByProductIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken = default);
    Task<InventoryAvailabilityDto?> GetAvailabilityAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryAvailabilityDto>> GetAvailabilityAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryReservation>> GetReservationsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddItemAsync(InventoryItem item, CancellationToken cancellationToken = default);
    Task AddReservationsAsync(IEnumerable<InventoryReservation> reservations, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
