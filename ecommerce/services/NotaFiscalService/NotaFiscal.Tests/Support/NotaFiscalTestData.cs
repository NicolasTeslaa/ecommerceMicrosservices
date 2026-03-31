using ECommerce.Shared.Messaging;
using NotaFiscal.Domain.Entities;

namespace NotaFiscal.Tests.Support;

public static class NotaFiscalTestData
{
    public static OrderConfirmedIntegrationEvent CreateOrderConfirmedEvent()
    {
        return new OrderConfirmedIntegrationEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CustomerAddressId = Guid.NewGuid(),
            CustomerEmail = "cliente@example.com",
            ShippingAmount = 20m,
            TotalAmount = 120m,
            Currency = "brl",
            Status = "Confirmed",
            ConfirmedAtUtc = DateTime.UtcNow,
            Items =
            [
                new OrderConfirmedIntegrationEventItem
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Notebook Gamer",
                    Quantity = 1,
                    UnitPrice = 100m,
                    TotalPrice = 100m
                }
            ]
        };
    }

    public static Invoice CreateInvoice(Guid? orderId = null, Guid? customerId = null)
    {
        return new Invoice(
            orderId ?? Guid.NewGuid(),
            customerId ?? Guid.NewGuid(),
            123456789,
            "1",
            "12345678901234567890123456789012345678901234",
            "<MockNFe><Status>Issued</Status></MockNFe>",
            150m,
            "BRL",
            DateTime.UtcNow);
    }
}
