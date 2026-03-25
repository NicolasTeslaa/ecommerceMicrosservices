using Microsoft.EntityFrameworkCore;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Domain.Enums;
using Order.Domain.Exceptions;

namespace Order.Infrastructure.Persistence;

public class OrderCancellationService : IOrderCancellationService
{
    private readonly OrderWriteDbContext _writeDbContext;
    private readonly IOrderReadModelProjector _readModelProjector;
    private readonly IInventoryOrderReservationClient _inventoryOrderReservationClient;

    public OrderCancellationService(
        OrderWriteDbContext writeDbContext,
        IOrderReadModelProjector readModelProjector,
        IInventoryOrderReservationClient inventoryOrderReservationClient)
    {
        _writeDbContext = writeDbContext;
        _readModelProjector = readModelProjector;
        _inventoryOrderReservationClient = inventoryOrderReservationClient;
    }

    public async Task<OrderActionResultDto> CancelAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken = default)
    {
        var order = await _writeDbContext.Orders
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == orderId, cancellationToken);

        if (order is null || order.CustomerId != customerId)
            throw new OrderNotFoundException(orderId);

        order.Cancel();

        await _writeDbContext.SaveChangesAsync(cancellationToken);
        await _inventoryOrderReservationClient.ReleaseAsync(orderId, cancellationToken);
        await _readModelProjector.ProjectAsync(order, cancellationToken);

        return new OrderActionResultDto
        {
            OrderId = order.Id,
            Status = OrderStatus.Cancelled.ToString(),
            Message = "Pedido cancelado com sucesso."
        };
    }
}
