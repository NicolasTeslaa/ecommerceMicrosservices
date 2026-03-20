using Order.Application.DTOs;
using Order.Application.ReadModels;

namespace Order.Application.Handlers;

internal static class OrderMappings
{
    public static OrderDto ToDto(this Order.Domain.Entities.Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerAddressId = order.CustomerAddressId,
            CustomerEmail = order.CustomerEmail,
            ShippingAddress = order.ShippingAddress,
            ShippingAmount = order.ShippingAmount,
            PaymentMethod = order.PaymentMethod,
            PaymentCardBrand = order.PaymentCardBrand,
            PaymentCardLast4 = order.PaymentCardLast4,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            RejectionReason = order.RejectionReason,
            RejectionDetail = order.RejectionDetail,
            CreatedAtUtc = order.CreatedAtUtc,
            Items = order.Items
                .Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice
                })
                .ToArray()
        };
    }

    public static OrderDto ToDto(this OrderReadModel order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerAddressId = order.CustomerAddressId,
            CustomerEmail = order.CustomerEmail,
            ShippingAddress = order.ShippingAddress,
            ShippingAmount = order.ShippingAmount,
            PaymentMethod = order.PaymentMethod,
            PaymentCardBrand = order.PaymentCardBrand,
            PaymentCardLast4 = order.PaymentCardLast4,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            RejectionReason = order.RejectionReason,
            RejectionDetail = order.RejectionDetail,
            CreatedAtUtc = order.CreatedAtUtc,
            Items = order.Items
                .Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice
                })
                .ToArray()
        };
    }
}
