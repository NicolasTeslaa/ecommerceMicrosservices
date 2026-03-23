using ECommerce.Shared.Protos;
using Grpc.Core;
using Payment.Infrastructure.Clients;
using Payment.Tests.Support;

namespace Payment.Tests.Infrastructure;

public class OrderPaymentAccessGrpcClientTests
{
    [Fact]
    public async Task ValidateAsync_ShouldReturnMappedReply()
    {
        var expectedOrderId = Guid.NewGuid();
        var expectedCustomerId = Guid.NewGuid();
        var client = new OrderPaymentAccessGrpcClient(new FakeOrderPaymentAccessClient(
            request =>
            {
                Assert.Equal(expectedOrderId.ToString(), request.OrderId);
                Assert.Equal(expectedCustomerId.ToString(), request.CustomerId);

                return new ValidateOrderAccessReply
                {
                    OrderExists = true,
                    CustomerMatches = true
                };
            }));

        var result = await client.ValidateAsync(expectedOrderId, expectedCustomerId);

        Assert.True(result.OrderExists);
        Assert.True(result.CustomerMatches);
    }

    private sealed class FakeOrderPaymentAccessClient : OrderPaymentAccess.OrderPaymentAccessClient
    {
        private readonly Func<ValidateOrderAccessRequest, ValidateOrderAccessReply> _handler;

        public FakeOrderPaymentAccessClient(Func<ValidateOrderAccessRequest, ValidateOrderAccessReply> handler)
        {
            _handler = handler;
        }

        public override AsyncUnaryCall<ValidateOrderAccessReply> ValidateOrderAccessAsync(
            ValidateOrderAccessRequest request,
            Metadata? headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return GrpcTestHelpers.CreateAsyncUnaryCall(_handler(request));
        }
    }
}
