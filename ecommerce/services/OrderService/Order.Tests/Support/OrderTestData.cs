using ECommerce.Shared.Contracts;
using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Application.ReadModels;
using Order.Domain.Entities;
using Order.Domain.Enums;
using OrderEntity = Order.Domain.Entities.Order;

namespace Order.Tests.Support;

internal static class OrderTestData
{
    public static CreateOrderCommand CreateCommand(
        PaymentMethod paymentMethod = PaymentMethod.Credit,
        Guid? customerId = null,
        Guid? customerAddressId = null)
    {
        var command = new CreateOrderCommand
        {
            CustomerId = customerId ?? Guid.NewGuid(),
            CustomerAddressId = customerAddressId ?? Guid.NewGuid(),
            ShippingAmount = 20m,
            PaymentMethod = paymentMethod,
            PaymentToken = paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit ? "tok_123" : null,
            PaymentCardBrand = paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit ? "Visa" : null,
            PaymentCardLast4 = paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit ? "1234" : null,
            Items =
            [
                new CreateOrderItemRequest
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Produto 1",
                    UnitPrice = 100m,
                    Quantity = 2
                }
            ]
        };

        return command;
    }

    public static OrderProcessingRequestDto CreateProcessingRequest(
        PaymentMethod paymentMethod = PaymentMethod.Credit,
        Guid? orderId = null,
        Guid? customerId = null,
        Guid? customerAddressId = null)
    {
        return new OrderProcessingRequestDto
        {
            OrderId = orderId ?? Guid.NewGuid(),
            CustomerId = customerId ?? Guid.NewGuid(),
            CustomerAddressId = customerAddressId ?? Guid.NewGuid(),
            ShippingAmount = 20m,
            PaymentMethod = paymentMethod,
            PaymentToken = paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit ? "tok_123" : null,
            PaymentCardBrand = paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit ? "Visa" : null,
            PaymentCardLast4 = paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit ? "1234" : null,
            RequestedAtUtc = DateTime.UtcNow,
            Items =
            [
                new OrderProcessingItemDto
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Produto 1",
                    UnitPrice = 100m,
                    Quantity = 2
                },
                new OrderProcessingItemDto
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Produto 2",
                    UnitPrice = 50m,
                    Quantity = 1
                }
            ]
        };
    }

    public static OrderEntity CreateOrder(
        PaymentMethod paymentMethod = PaymentMethod.Credit,
        OrderStatus status = OrderStatus.PendingPayment,
        OrderRejectionReason? rejectionReason = null,
        string? rejectionDetail = null)
    {
        if (status == OrderStatus.PaymentRejected)
        {
            return OrderEntity.CreateRejected(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                20m,
                paymentMethod,
                paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit ? "tok_123" : null,
                paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit ? "Visa" : null,
                paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit ? "1234" : null,
                [new OrderItem(Guid.NewGuid(), "Produto", 100m, 1)],
                DateTime.UtcNow,
                rejectionReason ?? OrderRejectionReason.ValidationFailed,
                rejectionDetail ?? "Pedido rejeitado.");
        }

        return new OrderEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            "Rua A, 123",
            20m,
            paymentMethod,
            paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit ? "tok_123" : null,
            paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit ? "Visa" : null,
            paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit ? "1234" : null,
            [
                new OrderItem(Guid.NewGuid(), "Produto 1", 100m, 2),
                new OrderItem(Guid.NewGuid(), "Produto 2", 50m, 1)
            ],
            DateTime.UtcNow);
    }

    public static OrderReadModel CreateReadModel(
        PaymentMethod paymentMethod = PaymentMethod.Credit,
        OrderStatus status = OrderStatus.PendingPayment,
        OrderRejectionReason? rejectionReason = null)
    {
        return new OrderReadModel
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CustomerAddressId = Guid.NewGuid(),
            CustomerEmail = "customer@example.com",
            ShippingAddress = "Rua A, 123",
            ShippingAmount = 20m,
            PaymentMethod = paymentMethod,
            PaymentCardBrand = paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit ? "Visa" : null,
            PaymentCardLast4 = paymentMethod is PaymentMethod.Credit or PaymentMethod.Debit ? "1234" : null,
            TotalAmount = 270m,
            Status = status,
            RejectionReason = rejectionReason,
            RejectionDetail = rejectionReason is null ? null : "Pedido rejeitado.",
            CreatedAtUtc = DateTime.UtcNow,
            Items =
            [
                new OrderItemReadModel
                {
                    Id = Guid.NewGuid(),
                    OrderId = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Produto 1",
                    UnitPrice = 100m,
                    Quantity = 2,
                    TotalPrice = 200m
                }
            ]
        };
    }

    public static PagedResult<OrderReadModel> CreatePagedReadModels(int count)
    {
        return PagedResult<OrderReadModel>.Create(
            Enumerable.Range(1, count).Select(_ => CreateReadModel()),
            1,
            count == 0 ? 10 : count,
            count);
    }
}
