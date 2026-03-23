using ECommerce.Shared.Protos;
using Payment.Application.Interfaces;

namespace Payment.Infrastructure.Clients;

public class OrderPaymentAccessGrpcClient : IOrderPaymentAccessClient
{
    private readonly OrderPaymentAccess.OrderPaymentAccessClient _client;

    public OrderPaymentAccessGrpcClient(OrderPaymentAccess.OrderPaymentAccessClient client)
    {
        _client = client;
    }

    public async Task<(bool OrderExists, bool CustomerMatches)> ValidateAsync(
        Guid orderId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var reply = await _client.ValidateOrderAccessAsync(
            new ValidateOrderAccessRequest
            {
                OrderId = orderId.ToString(),
                CustomerId = customerId.ToString()
            },
            cancellationToken: cancellationToken);

        return (reply.OrderExists, reply.CustomerMatches);
    }
}
