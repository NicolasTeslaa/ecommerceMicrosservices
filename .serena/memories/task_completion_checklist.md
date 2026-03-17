# Task Completion Checklist
After making code changes in this repository, prefer this sequence:

1. Restore/build the affected project or the full solution:
   - `dotnet restore .\ecommerce\ecommerce-platform.slnx`
   - `dotnet build .\ecommerce\ecommerce-platform.slnx`
2. Run formatting:
   - `dotnet format .\ecommerce\ecommerce-platform.slnx`
3. Run tests if/when test projects exist:
   - `dotnet test .\ecommerce\ecommerce-platform.slnx`
4. If you changed a runnable service, start the relevant entrypoint and smoke test the endpoint flow.
   - Gateway: `dotnet run --project .\ecommerce\gateway\ApiGateway\ApiGateway.csproj`
   - Catalog API: `dotnet run --project .\ecommerce\services\CatalogService\Catalog.API\Catalog.API.csproj`
5. Check `git diff` for unintended edits before finishing.

## Current verification caveats
- `CatalogService` currently has no test projects.
- Runtime validation of `Catalog.API` should include checking whether `IProductRepository` is registered through infrastructure and whether database configuration is present.
- Repository methods in `Catalog.Infrastructure/Persistence/ProductRepository` are still not implemented, so successful startup does not imply successful data operations.