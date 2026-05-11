using System.Net;
using System.Net.Http.Json;
using ECommerce.Shared.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Order.Application.DTOs;
using Order.Application.Interfaces;

namespace Order.Infrastructure.Clients;

public class CustomerProfileClient : ICustomerProfileClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerProfileClient> _logger;

    public CustomerProfileClient(HttpClient httpClient, ILogger<CustomerProfileClient>? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger ?? NullLogger<CustomerProfileClient>.Instance;
    }

    public async Task<CustomerProfileDto> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/customers/{customerId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogError("Customer '{CustomerId}' was not found by the customer profile client.", customerId);
                return new CustomerProfileDto { Id = customerId };
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Customer profile client received status code '{StatusCode}' while loading customer '{CustomerId}'.", response.StatusCode, customerId);
                return new CustomerProfileDto { Id = customerId };
            }

            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerProfileDto>>(cancellationToken: cancellationToken);
            if (payload?.Data is not null)
                return payload.Data;

            _logger.LogError("Customer profile client received an empty payload for customer '{CustomerId}'.", customerId);
            return new CustomerProfileDto { Id = customerId };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Customer profile client failed while loading customer '{CustomerId}'.", customerId);
            return new CustomerProfileDto { Id = customerId };
        }
    }
}
