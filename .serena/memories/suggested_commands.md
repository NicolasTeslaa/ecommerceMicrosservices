# Suggested Commands
All commands below are for PowerShell on Windows and should be run from `C:\repo\ecommerceMicrosservices` unless noted.

## General Windows / repo navigation
- List files: `Get-ChildItem`
- List files recursively: `Get-ChildItem -Recurse`
- Search text (preferred if available): `rg "pattern" .\ecommerce`
- Git status: `git status`
- Git diff: `git diff`

## Backend .NET restore / build / test
- Restore solution: `dotnet restore .\ecommerce\ecommerce-platform.slnx`
- Build solution: `dotnet build .\ecommerce\ecommerce-platform.slnx`
- Format solution: `dotnet format .\ecommerce\ecommerce-platform.slnx`
- Test solution: `dotnet test .\ecommerce\ecommerce-platform.slnx`

## Run backend entrypoints
- Run API gateway: `dotnet run --project .\ecommerce\gateway\ApiGateway\ApiGateway.csproj`
- Run catalog read API: `dotnet run --project .\ecommerce\services\CatalogService\Catalog.API.Read\Catalog.API.Read.csproj`
- Run catalog write API: `dotnet run --project .\ecommerce\services\CatalogService\Catalog.API.Write\Catalog.API.Write.csproj`
- Run cart API: `dotnet run --project .\ecommerce\services\CartService\Cart.API\Cart.API.csproj`

## Frontend SPA
- Install dependencies: `cd .\ecommerce\spa; npm install`
- Start dev server: `cd .\ecommerce\spa; npm run dev`
- Build SPA: `cd .\ecommerce\spa; npm run build`
- Test SPA: `cd .\ecommerce\spa; npm run test`
- Lint SPA: `cd .\ecommerce\spa; npm run lint`

## Useful Notes
- Gateway reverse proxy config lives in `ecommerce/gateway/ApiGateway/appsettings.json`.
- Cart service config lives in `ecommerce/services/CartService/Cart.API/appsettings.json`.
- SPA API integration should normally target the gateway base URL (`http://localhost:5100`) via `VITE_API_BASE_URL`.
- If backend builds fail for the new Cart projects, check for the existing environment issue around NuGet/MSBuild temp file creation under `obj`.