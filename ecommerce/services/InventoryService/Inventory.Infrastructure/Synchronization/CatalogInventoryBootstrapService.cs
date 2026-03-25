using System.Net.Http.Json;
using ECommerce.Shared.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;

namespace Inventory.Infrastructure.Synchronization;

public class CatalogInventoryBootstrapService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CatalogInventoryBootstrapService> _logger;

    public CatalogInventoryBootstrapService(
        IServiceScopeFactory serviceScopeFactory,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<CatalogInventoryBootstrapService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();
            var client = _httpClientFactory.CreateClient("catalog-read");

            var pageSize = int.TryParse(_configuration["CatalogService:BootstrapPageSize"], out var configuredPageSize)
                ? configuredPageSize
                : 500;

            var pageNumber = 1;
            var totalPages = 1;

            while (pageNumber <= totalPages && !stoppingToken.IsCancellationRequested)
            {
                var response = await client.GetFromJsonAsync<ApiResponse<IReadOnlyCollection<CatalogProductSnapshotDto>>>(
                    $"/api/catalog/products?pageNumber={pageNumber}&pageSize={pageSize}",
                    cancellationToken: stoppingToken);

                totalPages = Math.Max(response?.Pagination?.TotalPages ?? 1, 1);
                var products = response?.Data ?? Array.Empty<CatalogProductSnapshotDto>();

                foreach (var product in products.Where(product => product.Id != Guid.Empty))
                {
                    var existingItem = await repository.GetItemByProductIdAsync(product.Id, stoppingToken);

                    if (existingItem is null)
                    {
                        await repository.AddItemAsync(
                            new InventoryItem(product.Id, product.Name, 0, product.Active),
                            stoppingToken);
                        continue;
                    }

                    existingItem.UpdateCatalogMetadata(product.Name, product.Active);
                }

                pageNumber++;
            }

            await repository.SaveChangesAsync(stoppingToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Catalog bootstrap sync for inventory could not be completed.");
        }
    }

    private sealed class CatalogProductSnapshotDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}
