using Expedition.Application.DTOs;
using Expedition.Application.Interfaces;
using Expedition.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Expedition.Infrastructure.Persistence;

public class ExpeditionRepository : IExpeditionRepository
{
    private readonly ExpeditionDbContext _context;

    public ExpeditionRepository(ExpeditionDbContext context)
    {
        _context = context;
    }

    public async Task<ExpeditionDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.ExpeditionOrders
            .AsNoTracking()
            .Where(item => item.OrderId == orderId)
            .Select(item => new ExpeditionDto
            {
                Id = item.Id,
                OrderId = item.OrderId,
                InvoiceId = item.InvoiceId,
                CustomerId = item.CustomerId,
                InvoiceNumber = item.InvoiceNumber,
                InvoiceSeries = item.InvoiceSeries,
                InvoiceAccessKey = item.InvoiceAccessKey,
                Status = item.Status.ToString(),
                FailureReason = item.FailureReason.ToString(),
                FailureDetails = item.FailureDetails,
                InvoiceIssuedAtUtc = item.InvoiceIssuedAtUtc,
                CreatedAtUtc = item.CreatedAtUtc,
                UpdatedAtUtc = item.UpdatedAtUtc,
                PickedUpAtUtc = item.PickedUpAtUtc,
                InTransitAtUtc = item.InTransitAtUtc,
                DeliveredAtUtc = item.DeliveredAtUtc,
                FailedAtUtc = item.FailedAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<ExpeditionOrder?> GetEntityByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return _context.ExpeditionOrders.FirstOrDefaultAsync(item => item.OrderId == orderId, cancellationToken);
    }

    public Task AddAsync(ExpeditionOrder expeditionOrder, CancellationToken cancellationToken = default)
    {
        return _context.ExpeditionOrders.AddAsync(expeditionOrder, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
