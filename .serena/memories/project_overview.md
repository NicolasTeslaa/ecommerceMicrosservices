# Project Overview
- Repository: `C:\repo\ecommerceMicrosservices`
- Main purpose: e-commerce microservices platform organized as an API gateway plus domain services.
- Current implemented code is focused on `ApiGateway` and `CatalogService`; other service folders (`CartService`, `InventoryService`, `NotificationService`, `OrderService`, `PaymentService`) currently exist as structure placeholders in the solution.
- Platform/OS context: Windows development environment.

## Tech Stack
- Language: C#
- Framework: .NET 9 (`net9.0`)
- Web stack: ASP.NET Core Web API
- API gateway: YARP (`Yarp.ReverseProxy`)
- API docs: OpenAPI / Swagger
- Application messaging: MediatR
- Persistence stack prepared: Entity Framework Core 9 + Pomelo MySQL provider

## Solution / Structure
- Solution file: `ecommerce/ecommerce-platform.slnx`
- Gateway: `ecommerce/gateway/ApiGateway`
- Services root: `ecommerce/services`
- Catalog service is split into 4 projects:
  - `Catalog.API`: HTTP entrypoint/controllers and MediatR registration
  - `Catalog.Application`: commands, queries, handlers, DTOs, repository interface
  - `Catalog.Domain`: domain entities and invariants
  - `Catalog.Infrastructure`: EF Core DbContext, DI extension, repository implementation

## Runtime Notes
- Gateway proxies `/api/catalog/*` to `https://localhost:5101/`.
- Gateway also has routes configured for cart/order/payment destinations, but matching services are not implemented in this repository yet.
- `Catalog.API` exposes product endpoints through a controller and dispatches requests via MediatR.

## Current State / Caveats
- `Catalog.Domain` no longer references `Catalog.Application`; the earlier cross-layer reference issue is gone.
- `Catalog.API` currently registers MediatR but does not reference `Catalog.Infrastructure` and does not call `AddInfrastructure(...)`, so handlers that depend on `IProductRepository` are not fully wired at runtime.
- `Catalog.Infrastructure/Persistence/ProductRepository` is still a stub and throws `NotImplementedException` in all methods.
- `Catalog.API/appsettings.json` does not currently define a `CatalogDb` connection string.
- No README, repo-wide `.editorconfig`, `global.json`, or test projects were found during the latest onboarding refresh.