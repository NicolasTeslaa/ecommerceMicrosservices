using System.Net;
using IntegrationTests.Contracts;
using IntegrationTests.Infrastructure;

namespace IntegrationTests;

[Collection(MicroservicesCollection.Name)]
public sealed class OrderIntegrationTests
{
    private readonly MicroservicesTestEnvironment _environment;

    public OrderIntegrationTests(MicroservicesTestEnvironment environment)
    {
        _environment = environment;
    }

    [Fact]
    public async Task CreatingOrder_WithAvailableProductAndMissingAddress_ShouldBeRejectedByOrderProcessor()
    {
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var registerRequest = IntegrationTestData.CreateRegisterUserRequest(suffix);
        var (registerStatusCode, registerBody) = await _environment.GatewayApi.RegisterUserAsync(registerRequest);

        Assert.Equal(HttpStatusCode.Created, registerStatusCode);
        Assert.NotNull(registerBody?.Data);

        var authData = registerBody!.Data!;
        var product = await FindReservableProductAsync();

        var orderRequest = new CreateOrderRequest
        {
            CustomerId = authData.CustomerId,
            CustomerAddressId = Guid.NewGuid(),
            ShippingAmount = 25m,
            PaymentMethod = PaymentMethod.Pix,
            Items =
            [
                new CreateOrderItemRequest
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = 1
                }
            ]
        };

        var (createStatusCode, createBody) = await _environment.GatewayApi.CreateOrderAsync(orderRequest);

        Assert.Equal(HttpStatusCode.Accepted, createStatusCode);
        Assert.NotNull(createBody?.Data);
        Assert.True(createBody!.Success, createBody.Message);

        var acceptedOrder = createBody.Data!;

        var orderResult = await _environment.WaitForAsync(
            cancellationToken => _environment.GatewayApi.GetOrderAsync(acceptedOrder.OrderId, cancellationToken),
            result => result.StatusCode == HttpStatusCode.OK
                && result.Body?.Success == true
                && result.Body.Data?.Status == OrderStatus.PaymentRejected,
            describe: result => $"StatusCode={result.StatusCode}, Success={result.Body?.Success}, OrderStatus={result.Body?.Data?.Status}, Rejection={result.Body?.Data?.RejectionReason}");

        var order = orderResult.Body!.Data!;
        Assert.Equal(OrderStatus.PaymentRejected, order.Status);
        Assert.Equal(OrderRejectionReason.InvalidCustomerAddress, order.RejectionReason);
        Assert.Contains("address", order.RejectionDetail ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(authData.CustomerId, order.CustomerId);
        Assert.Equal(orderRequest.CustomerAddressId, order.CustomerAddressId);
        Assert.Single(order.Items);
    }

    [Fact]
    public async Task CreatingOrder_WithUnknownProduct_ShouldBeRejectedByInventoryValidation()
    {
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var registerRequest = IntegrationTestData.CreateRegisterUserRequest(suffix);
        var (registerStatusCode, registerBody) = await _environment.GatewayApi.RegisterUserAsync(registerRequest);

        Assert.Equal(HttpStatusCode.Created, registerStatusCode);
        Assert.NotNull(registerBody?.Data);

        var authData = registerBody!.Data!;
        var unknownProductId = Guid.NewGuid();

        var orderRequest = new CreateOrderRequest
        {
            CustomerId = authData.CustomerId,
            CustomerAddressId = Guid.NewGuid(),
            ShippingAmount = 10m,
            PaymentMethod = PaymentMethod.Pix,
            Items =
            [
                new CreateOrderItemRequest
                {
                    ProductId = unknownProductId,
                    ProductName = "Produto inexistente",
                    UnitPrice = 99.90m,
                    Quantity = 1
                }
            ]
        };

        var (createStatusCode, createBody) = await _environment.GatewayApi.CreateOrderAsync(orderRequest);

        Assert.Equal(HttpStatusCode.Accepted, createStatusCode);
        Assert.NotNull(createBody?.Data);

        var acceptedOrder = createBody!.Data!;

        var orderResult = await _environment.WaitForAsync(
            cancellationToken => _environment.GatewayApi.GetOrderAsync(acceptedOrder.OrderId, cancellationToken),
            result => result.StatusCode == HttpStatusCode.OK
                && result.Body?.Success == true
                && result.Body.Data?.Status == OrderStatus.PaymentRejected,
            describe: result => $"StatusCode={result.StatusCode}, Success={result.Body?.Success}, OrderStatus={result.Body?.Data?.Status}, Rejection={result.Body?.Data?.RejectionReason}");

        var order = orderResult.Body!.Data!;
        Assert.Equal(OrderStatus.PaymentRejected, order.Status);
        Assert.Equal(OrderRejectionReason.ProductUnavailable, order.RejectionReason);
        Assert.Contains("product", order.RejectionDetail ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(authData.CustomerId, order.CustomerId);
        Assert.Single(order.Items);
        Assert.Equal(unknownProductId, order.Items.Single().ProductId);
    }

    [Fact]
    public async Task CreatingOrder_WithValidAddressAndAvailableProduct_ShouldPersistPendingPaymentOrder()
    {
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var registerRequest = IntegrationTestData.CreateRegisterUserRequest(suffix);
        var (registerStatusCode, registerBody) = await _environment.GatewayApi.RegisterUserAsync(registerRequest);

        Assert.Equal(HttpStatusCode.Created, registerStatusCode);
        Assert.NotNull(registerBody?.Data);

        var authData = registerBody!.Data!;

        await _environment.WaitForAsync(
            cancellationToken => _environment.GatewayApi.GetCustomerAsync(authData.CustomerId, cancellationToken),
            result => result.StatusCode == HttpStatusCode.OK && result.Body?.Success == true);

        var addressRequest = IntegrationTestData.CreateAddressRequest();
        var (addressStatusCode, addressBody) = await _environment.GatewayApi.AddCustomerAddressAsync(authData.CustomerId, addressRequest);

        Assert.Equal(HttpStatusCode.OK, addressStatusCode);
        Assert.NotNull(addressBody?.Data);

        var customerAddress = addressBody!.Data!;
        var product = await FindReservableProductAsync();

        var orderRequest = new CreateOrderRequest
        {
            CustomerId = authData.CustomerId,
            CustomerAddressId = customerAddress.Id,
            ShippingAmount = 17.5m,
            PaymentMethod = PaymentMethod.Pix,
            Items =
            [
                new CreateOrderItemRequest
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = 1
                }
            ]
        };

        var (createStatusCode, createBody) = await _environment.GatewayApi.CreateOrderAsync(orderRequest);

        Assert.Equal(HttpStatusCode.Accepted, createStatusCode);
        Assert.NotNull(createBody?.Data);
        Assert.True(createBody!.Success, createBody.Message);

        var acceptedOrder = createBody.Data!;

        var orderResult = await _environment.WaitForAsync(
            cancellationToken => _environment.GatewayApi.GetOrderAsync(acceptedOrder.OrderId, cancellationToken),
            result => result.StatusCode == HttpStatusCode.OK
                && result.Body?.Success == true
                && result.Body.Data is not null
                && result.Body.Data.Status == OrderStatus.PendingPayment,
            describe: result => $"StatusCode={result.StatusCode}, Success={result.Body?.Success}, OrderStatus={result.Body?.Data?.Status}, Rejection={result.Body?.Data?.RejectionReason}");

        var order = orderResult.Body!.Data!;
        Assert.Equal(OrderStatus.PendingPayment, order.Status);
        Assert.Null(order.RejectionReason);
        Assert.Equal(authData.CustomerId, order.CustomerId);
        Assert.Equal(customerAddress.Id, order.CustomerAddressId);
        Assert.Equal(registerRequest.Email.ToLowerInvariant(), order.CustomerEmail);
        Assert.False(string.IsNullOrWhiteSpace(order.ShippingAddress));
        Assert.Contains(customerAddress.Street, order.ShippingAddress, StringComparison.OrdinalIgnoreCase);
        Assert.Single(order.Items);
        Assert.Equal(product.Id, order.Items.Single().ProductId);
    }

    private async Task<ProductResponse> FindReservableProductAsync()
    {
        var (productsStatusCode, productsBody) = await _environment.GatewayApi.GetProductsAsync();

        Assert.Equal(HttpStatusCode.OK, productsStatusCode);
        Assert.NotNull(productsBody?.Data);

        foreach (var product in productsBody!.Data!.Where(item => item.Active))
        {
            var (inventoryStatusCode, inventoryBody) = await _environment.GatewayApi.GetInventoryAvailabilityAsync(product.Id);

            if (inventoryStatusCode != HttpStatusCode.OK || inventoryBody?.Data is null)
                continue;

            if (inventoryBody.Data.Active && inventoryBody.Data.AvailableQuantity > 0)
                return product;
        }

        throw new InvalidOperationException("No active product with available inventory was found for the integration test.");
    }
}
