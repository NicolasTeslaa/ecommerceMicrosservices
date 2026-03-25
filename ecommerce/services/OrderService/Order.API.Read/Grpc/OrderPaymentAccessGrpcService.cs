using ECommerce.Shared.Protos;
using Grpc.Core;
using Order.Application.Interfaces;
using Order.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Order.API.Read.Grpc;

public class OrderPaymentAccessGrpcService : OrderPaymentAccess.OrderPaymentAccessBase
{
    private readonly IOrderReadRepository _repository;
    private readonly OrderWriteDbContext _writeDbContext;

    public OrderPaymentAccessGrpcService(IOrderReadRepository repository, OrderWriteDbContext writeDbContext)
    {
        _repository = repository;
        _writeDbContext = writeDbContext;
    }

    public override async Task<ValidateOrderAccessReply> ValidateOrderAccess(ValidateOrderAccessRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.OrderId, out var orderId) || !Guid.TryParse(request.CustomerId, out var customerId))
        {
            return new ValidateOrderAccessReply
            {
                OrderExists = false,
                CustomerMatches = false
            };
        }

        var order = await _repository.GetByIdAsync(orderId, context.CancellationToken);

        if (order is null)
        {
            var writeOrder = await _writeDbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == orderId, context.CancellationToken);

            if (writeOrder is null)
            {
                return new ValidateOrderAccessReply
                {
                    OrderExists = false,
                    CustomerMatches = false
                };
            }

            return new ValidateOrderAccessReply
            {
                OrderExists = true,
                CustomerMatches = writeOrder.CustomerId == customerId
            };
        }

        return new ValidateOrderAccessReply
        {
            OrderExists = true,
            CustomerMatches = order.CustomerId == customerId
        };
    }
}
