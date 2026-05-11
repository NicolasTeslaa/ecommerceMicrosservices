using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Domain.Enums;

namespace Order.Infrastructure.Persistence;

public class OrderCancellationService : IOrderCancellationService
{
    private readonly OrderWriteDbContext _writeDbContext;
    private readonly IOrderReadModelProjector _readModelProjector;
    private readonly IInventoryOrderReservationClient _inventoryOrderReservationClient;
    private readonly ILogger<OrderCancellationService> _logger;

    public OrderCancellationService(
        OrderWriteDbContext writeDbContext,
        IOrderReadModelProjector readModelProjector,
        IInventoryOrderReservationClient inventoryOrderReservationClient,
        ILogger<OrderCancellationService>? logger = null)
    {
        _writeDbContext = writeDbContext;
        _readModelProjector = readModelProjector;
        _inventoryOrderReservationClient = inventoryOrderReservationClient;
        _logger = logger ?? NullLogger<OrderCancellationService>.Instance;
    }

    public async Task<OrderActionResultDto> CancelAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken = default)
    {
        var order = await _writeDbContext.Orders
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == orderId, cancellationToken);

        if (order is null || order.CustomerId != customerId)
        {
            _logger.LogError("Failed to cancel order '{OrderId}' for customer '{CustomerId}' because the order was not found.", orderId, customerId);
            return new OrderActionResultDto
            {
                OrderId = orderId,
                Status = string.Empty,
                Message = "Pedido nao encontrado para cancelamento."
            };
        }

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
