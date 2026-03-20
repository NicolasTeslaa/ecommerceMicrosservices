using Catalog.Application.Interfaces;
using ECommerce.Shared.Protos;
using Grpc.Core;

namespace Catalog.API.Read.Grpc;

public class CatalogProductAvailabilityGrpcService : CatalogProductAvailability.CatalogProductAvailabilityBase
{
    private readonly IProductReadRepository _repository;

    public CatalogProductAvailabilityGrpcService(IProductReadRepository repository)
    {
        _repository = repository;
    }

    public override async Task<ValidateOrderItemsReply> ValidateOrderItems(ValidateOrderItemsRequest request, ServerCallContext context)
    {
        var rejectedItems = new List<RejectedOrderItem>();
        var validItems = new List<(Guid ProductId, string ProductName, int RequestedQuantity)>();

        foreach (var item in request.Items)
        {
            if (!Guid.TryParse(item.ProductId, out var productId))
            {
                rejectedItems.Add(new RejectedOrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = string.IsNullOrWhiteSpace(item.ProductName) ? "Produto invalido" : item.ProductName,
                    RequestedQuantity = item.RequestedQuantity,
                    AvailableQuantity = 0,
                    Reason = "Invalid product identifier."
                });
                continue;
            }

            if (item.RequestedQuantity <= 0)
            {
                rejectedItems.Add(new RejectedOrderItem
                {
                    ProductId = productId.ToString(),
                    ProductName = string.IsNullOrWhiteSpace(item.ProductName) ? "Produto invalido" : item.ProductName,
                    RequestedQuantity = item.RequestedQuantity,
                    AvailableQuantity = 0,
                    Reason = "Requested quantity must be greater than zero."
                });
                continue;
            }

            validItems.Add((productId, item.ProductName, item.RequestedQuantity));
        }

        var items = validItems
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                ProductName = group.Select(item => item.ProductName).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? "Produto",
                RequestedQuantity = group.Sum(item => item.RequestedQuantity)
            })
            .ToArray();

        if (items.Length == 0 && rejectedItems.Count == 0)
        {
            rejectedItems.Add(new RejectedOrderItem
            {
                ProductId = string.Empty,
                ProductName = "Pedido invalido",
                RequestedQuantity = 0,
                AvailableQuantity = 0,
                Reason = "No valid products were provided for stock validation."
            });
        }

        var requestedIds = items
            .Select(item => item.ProductId)
            .Distinct()
            .ToArray();

        var products = await _repository.GetByIdsAsync(requestedIds, context.CancellationToken);
        var productById = products.ToDictionary(product => product.Id);

        foreach (var item in items)
        {
            if (!productById.TryGetValue(item.ProductId, out var product) || !product.Active)
            {
                rejectedItems.Add(new RejectedOrderItem
                {
                    ProductId = item.ProductId.ToString(),
                    ProductName = string.IsNullOrWhiteSpace(item.ProductName) ? "Produto indisponivel" : item.ProductName,
                    RequestedQuantity = item.RequestedQuantity,
                    AvailableQuantity = 0,
                    Reason = "Product does not exist or is inactive."
                });
                continue;
            }

            if (product.StockQuantity < item.RequestedQuantity)
            {
                rejectedItems.Add(new RejectedOrderItem
                {
                    ProductId = item.ProductId.ToString(),
                    ProductName = product.Name,
                    RequestedQuantity = item.RequestedQuantity,
                    AvailableQuantity = product.StockQuantity,
                    Reason = "Insufficient stock."
                });
            }
        }

        if (rejectedItems.Count == 0)
        {
            return new ValidateOrderItemsReply
            {
                IsValid = true,
                Reason = "All items are available."
            };
        }

        var summaryReason = rejectedItems.Count == 1
            ? rejectedItems[0].Reason
            : "One or more products are unavailable for this order.";

        var reply = new ValidateOrderItemsReply
        {
            IsValid = false,
            Reason = summaryReason
        };
        reply.RejectedItems.AddRange(rejectedItems);
        return reply;
    }
}
