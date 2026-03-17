# Suggested Commands
All commands below are for PowerShell on Windows and should be run from `C:\repo\ecommerceMicrosservices` unless noted.

## General Windows / repo navigation
- List files: `Get-ChildItem`
- List files recursively: `Get-ChildItem -Recurse`
- Search text (preferred if available): `rg "pattern" .\ecommerce`
- Git status: `git status`
- Git diff: `git diff`

## .NET restore / build / format / test
- Restore solution: `dotnet restore .\ecommerce\ecommerce-platform.slnx`
- Build solution: `dotnet build .\ecommerce\ecommerce-platform.slnx`
- Format solution: `dotnet format .\ecommerce\ecommerce-platform.slnx`
- Test solution: `dotnet test .\ecommerce\ecommerce-platform.slnx`
  - Note: no test projects were found during the latest onboarding refresh.

## Run entrypoints
- Run API gateway: `dotnet run --project .\ecommerce\gateway\ApiGateway\ApiGateway.csproj`
- Run catalog API: `dotnet run --project .\ecommerce\services\CatalogService\Catalog.API\Catalog.API.csproj`

## Useful targeted builds
- Build gateway only: `dotnet build .\ecommerce\gateway\ApiGateway\ApiGateway.csproj`
- Build catalog API only: `dotnet build .\ecommerce\services\CatalogService\Catalog.API\Catalog.API.csproj`
- Build catalog application only: `dotnet build .\ecommerce\services\CatalogService\Catalog.Application\Catalog.Application.csproj`
- Build catalog domain only: `dotnet build .\ecommerce\services\CatalogService\Catalog.Domain\Catalog.Domain.csproj`
- Build catalog infrastructure only: `dotnet build .\ecommerce\services\CatalogService\Catalog.Infrastructure\Catalog.Infrastructure.csproj`

## Notes
- Gateway reverse proxy config lives in `ecommerce/gateway/ApiGateway/appsettings.json`.
- Catalog API config currently only includes logging and host defaults in `ecommerce/services/CatalogService/Catalog.API/appsettings.json`.
- Before relying on runtime behavior in `CatalogService`, verify DI wiring for `IProductRepository` and database configuration.