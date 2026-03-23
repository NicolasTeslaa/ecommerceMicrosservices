using ECommerce.Shared.Protos;
using Grpc.Core;
using Order.Application.Interfaces;

namespace Order.API.Read.Grpc;

public class OrderPaymentAccessGrpcService : OrderPaymentAccess.OrderPaymentAccessBase
{
    private readonly IOrderReadRepository _repository;

    public OrderPaymentAccessGrpcService(IOrderReadRepository repository)
    {
        _repository = repository;
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
            return new ValidateOrderAccessReply
            {
                OrderExists = false,
                CustomerMatches = false
            };
        }

        return new ValidateOrderAccessReply
        {
            OrderExists = true,
            CustomerMatches = order.CustomerId == customerId
        };
    }
}
