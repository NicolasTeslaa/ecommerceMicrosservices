using ECommerce.Shared.Protos;
using Order.Application.DTOs;
using Order.Application.Interfaces;

namespace Order.Infrastructure.Clients;

public class InventoryOrderReservationGrpcClient : IInventoryOrderReservationClient
{
    private readonly InventoryOrderReservation.InventoryOrderReservationClient _client;

    public InventoryOrderReservationGrpcClient(InventoryOrderReservation.InventoryOrderReservationClient client)
    {
        _client = client;
    }

    public async Task<ProductAvailabilityValidationResultDto> ReserveAsync(
        Guid orderId,
        Guid customerId,
        IReadOnlyCollection<ProductAvailabilityCheckItemDto> items,
        CancellationToken cancellationToken = default)
    {
        var request = new ReserveOrderItemsRequest
        {
            OrderId = orderId.ToString(),
            CustomerId = customerId.ToString()
        };

        request.Items.AddRange(items.Select(item => new ReserveOrderItem
        {
            ProductId = item.ProductId.ToString(),
            ProductName = item.ProductName,
            Quantity = item.RequestedQuantity
        }));

        var reply = await _client.ReserveOrderItemsAsync(request, cancellationToken: cancellationToken);

        return new ProductAvailabilityValidationResultDto
        {
            IsValid = reply.IsSuccess,
            Reason = reply.Reason,
            Issues = reply.RejectedItems
                .Select(item => new ProductAvailabilityIssueDto
                {
                    ProductId = Guid.TryParse(item.ProductId, out var productId) ? productId : Guid.Empty,
                    ProductName = item.ProductName,
                    RequestedQuantity = item.RequestedQuantity,
                    AvailableQuantity = item.AvailableQuantity,
                    Reason = item.Reason
                })
                .ToArray()
        };
    }

    public async Task ReleaseAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        await _client.ReleaseOrderReservationAsync(
            new ReleaseOrderReservationRequest
            {
                OrderId = orderId.ToString()
            },
            cancellationToken: cancellationToken);
    }
}
