using Microsoft.EntityFrameworkCore;
using Order.Application.Interfaces;
using Order.Application.ReadModels;

namespace Order.Infrastructure.Persistence;

public class OrderReadModelProjector : IOrderReadModelProjector
{
    private readonly OrderReadDbContext _dbContext;

    public OrderReadModelProjector(OrderReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ProjectAsync(Order.Domain.Entities.Order order, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Orders
            .Include(readModel => readModel.Items)
            .FirstOrDefaultAsync(readModel => readModel.Id == order.Id, cancellationToken);

        if (existing is not null)
        {
            _dbContext.OrderItems.RemoveRange(existing.Items);
            _dbContext.Orders.Remove(existing);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var readModel = new OrderReadModel
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
                .Select(item => new OrderItemReadModel
                {
                    Id = item.Id,
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice
                })
                .ToList()
        };

        await _dbContext.Orders.AddAsync(readModel, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
