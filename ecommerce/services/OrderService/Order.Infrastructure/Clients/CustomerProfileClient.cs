using System.Net;
using System.Net.Http.Json;
using ECommerce.Shared.Contracts;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Domain.Exceptions;

namespace Order.Infrastructure.Clients;

public class CustomerProfileClient : ICustomerProfileClient
{
    private readonly HttpClient _httpClient;

    public CustomerProfileClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CustomerProfileDto> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/customers/{customerId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new Order.Domain.Exceptions.CustomerAddressNotFoundException(customerId, Guid.Empty);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerProfileDto>>(cancellationToken: cancellationToken);
        return payload?.Data ?? throw new PersistenceException($"Customer '{customerId}' response was empty.");
    }
}
