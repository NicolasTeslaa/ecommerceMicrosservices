using System.Net.Http.Json;
using ECommerce.Shared.Contracts;
using Notification.Application.Interfaces;

namespace Notification.Infrastructure.Clients;

public class CustomerContactClient : ICustomerContactClient
{
    private readonly HttpClient _httpClient;

    public CustomerContactClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CustomerContact?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"api/customers/{customerId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerContactPayload>>(cancellationToken: cancellationToken);
        if (payload?.Success != true || payload.Data is null)
            return null;

        return new CustomerContact(payload.Data.Id, payload.Data.Email, payload.Data.PhoneNumber);
    }

    public sealed class CustomerContactPayload
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
