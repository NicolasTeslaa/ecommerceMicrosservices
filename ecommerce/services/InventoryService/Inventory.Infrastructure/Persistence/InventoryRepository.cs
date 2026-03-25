using Inventory.Application.DTOs;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _context;

    public InventoryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public Task<InventoryItem?> GetItemByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return _context.InventoryItems.FirstOrDefaultAsync(item => item.ProductId == productId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventoryItem>> GetItemsByProductIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
            return Array.Empty<InventoryItem>();

        return await _context.InventoryItems
            .Where(item => productIds.Contains(item.ProductId))
            .ToListAsync(cancellationToken);
    }

    public async Task<InventoryAvailabilityDto?> GetAvailabilityAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .AsNoTracking()
            .Where(item => item.ProductId == productId)
            .Select(item => new InventoryAvailabilityDto
            {
                ProductId = item.ProductId,
                AvailableQuantity = item.AvailableQuantity,
                ReservedQuantity = item.ReservedQuantity,
                Active = item.Active
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventoryAvailabilityDto>> GetAvailabilityAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
            return Array.Empty<InventoryAvailabilityDto>();

        return await _context.InventoryItems
            .AsNoTracking()
            .Where(item => productIds.Contains(item.ProductId))
            .Select(item => new InventoryAvailabilityDto
            {
                ProductId = item.ProductId,
                AvailableQuantity = item.AvailableQuantity,
                ReservedQuantity = item.ReservedQuantity,
                Active = item.Active
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventoryReservation>> GetReservationsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryReservations
            .Where(item => item.OrderId == orderId)
            .ToListAsync(cancellationToken);
    }

    public Task AddItemAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        return _context.InventoryItems.AddAsync(item, cancellationToken).AsTask();
    }

    public Task AddReservationsAsync(IEnumerable<InventoryReservation> reservations, CancellationToken cancellationToken = default)
    {
        return _context.InventoryReservations.AddRangeAsync(reservations, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
