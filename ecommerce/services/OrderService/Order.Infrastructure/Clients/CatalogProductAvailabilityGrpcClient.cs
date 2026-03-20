using ECommerce.Shared.Protos;
using Order.Application.DTOs;
using Order.Application.Interfaces;

namespace Order.Infrastructure.Clients;

public class CatalogProductAvailabilityGrpcClient : ICatalogProductAvailabilityClient
{
    private readonly CatalogProductAvailability.CatalogProductAvailabilityClient _client;

    public CatalogProductAvailabilityGrpcClient(CatalogProductAvailability.CatalogProductAvailabilityClient client)
    {
        _client = client;
    }

    public async Task<ProductAvailabilityValidationResultDto> ValidateAsync(
        IReadOnlyCollection<ProductAvailabilityCheckItemDto> items,
        CancellationToken cancellationToken = default)
    {
        var request = new ValidateOrderItemsRequest();
        request.Items.AddRange(items.Select(item => new ValidateOrderItem
        {
            ProductId = item.ProductId.ToString(),
            ProductName = item.ProductName,
            RequestedQuantity = item.RequestedQuantity
        }));

        var reply = await _client.ValidateOrderItemsAsync(request, cancellationToken: cancellationToken);

        return new ProductAvailabilityValidationResultDto
        {
            IsValid = reply.IsValid,
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
}
