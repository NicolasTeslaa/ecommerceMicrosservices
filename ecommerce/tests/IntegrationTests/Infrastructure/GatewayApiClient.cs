using System.Net;
using System.Net.Http.Json;
using IntegrationTests.Contracts;

namespace IntegrationTests.Infrastructure;

public sealed class GatewayApiClient
{
    private readonly HttpClient _httpClient;

    public GatewayApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(HttpStatusCode StatusCode, ApiResponse<AuthResponse>? Body)> RegisterUserAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/auth/register", request, HttpJsonExtensions.JsonOptions, cancellationToken);
        var body = await response.Content.ReadJsonAsync<ApiResponse<AuthResponse>>(cancellationToken);
        return (response.StatusCode, body);
    }

    public async Task<(HttpStatusCode StatusCode, ApiResponse<AuthResponse>? Body)> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request, HttpJsonExtensions.JsonOptions, cancellationToken);
        var body = await response.Content.ReadJsonAsync<ApiResponse<AuthResponse>>(cancellationToken);
        return (response.StatusCode, body);
    }

    public async Task<(HttpStatusCode StatusCode, ApiResponse<CustomerResponse>? Body)> GetCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"/api/customers/{customerId}", cancellationToken);
        var body = await response.Content.ReadJsonAsync<ApiResponse<CustomerResponse>>(cancellationToken);
        return (response.StatusCode, body);
    }

    public async Task<(HttpStatusCode StatusCode, ApiResponse<CustomerAddressResponse>? Body)> AddCustomerAddressAsync(
        Guid customerId,
        UpsertCustomerAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"/api/customers/{customerId}/addresses", request, HttpJsonExtensions.JsonOptions, cancellationToken);
        var body = await response.Content.ReadJsonAsync<ApiResponse<CustomerAddressResponse>>(cancellationToken);
        return (response.StatusCode, body);
    }

    public async Task<(HttpStatusCode StatusCode, ApiResponse<CustomerAddressResponse>? Body)> UpdateCustomerAddressAsync(
        Guid customerId,
        Guid addressId,
        UpsertCustomerAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"/api/customers/{customerId}/addresses/{addressId}", request, HttpJsonExtensions.JsonOptions, cancellationToken);
        var body = await response.Content.ReadJsonAsync<ApiResponse<CustomerAddressResponse>>(cancellationToken);
        return (response.StatusCode, body);
    }

    public async Task<(HttpStatusCode StatusCode, ApiResponse<IReadOnlyCollection<CustomerAddressResponse>>? Body)> GetCustomerAddressesAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"/api/customers/{customerId}/addresses", cancellationToken);
        var body = await response.Content.ReadJsonAsync<ApiResponse<IReadOnlyCollection<CustomerAddressResponse>>>(cancellationToken);
        return (response.StatusCode, body);
    }

    public async Task<(HttpStatusCode StatusCode, ApiResponse<CustomerAddressResponse>? Body)> SetDefaultCustomerAddressAsync(
        Guid customerId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/customers/{customerId}/addresses/{addressId}/default");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadJsonAsync<ApiResponse<CustomerAddressResponse>>(cancellationToken);
        return (response.StatusCode, body);
    }

    public async Task<(HttpStatusCode StatusCode, ApiResponse<object?>? Body)> RemoveCustomerAddressAsync(
        Guid customerId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"/api/customers/{customerId}/addresses/{addressId}", cancellationToken);
        var body = await response.Content.ReadJsonAsync<ApiResponse<object?>>(cancellationToken);
        return (response.StatusCode, body);
    }

    public async Task<(HttpStatusCode StatusCode, ApiResponse<IReadOnlyCollection<ProductResponse>>? Body)> GetProductsAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("/api/catalog/products?pageNumber=1&pageSize=100", cancellationToken);
        var body = await response.Content.ReadJsonAsync<ApiResponse<IReadOnlyCollection<ProductResponse>>>(cancellationToken);
        return (response.StatusCode, body);
    }

    public async Task<(HttpStatusCode StatusCode, ApiResponse<InventoryAvailabilityResponse?>? Body)> GetInventoryAvailabilityAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"/api/inventory/products/{productId}", cancellationToken);
        var body = await response.Content.ReadJsonAsync<ApiResponse<InventoryAvailabilityResponse?>>(cancellationToken);
        return (response.StatusCode, body);
    }

    public async Task<(HttpStatusCode StatusCode, ApiResponse<OrderProcessingAcceptedResponse>? Body)> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/orders", request, HttpJsonExtensions.JsonOptions, cancellationToken);
        var body = await response.Content.ReadJsonAsync<ApiResponse<OrderProcessingAcceptedResponse>>(cancellationToken);
        return (response.StatusCode, body);
    }

    public async Task<(HttpStatusCode StatusCode, ApiResponse<OrderResponse>? Body)> GetOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"/api/orders/{orderId}", cancellationToken);
        var body = await response.Content.ReadJsonAsync<ApiResponse<OrderResponse>>(cancellationToken);
        return (response.StatusCode, body);
    }
}
