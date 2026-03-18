# Project Overview
- Repository: `C:\repo\ecommerceMicrosservices`
- Main purpose: e-commerce microservices platform with an API gateway, backend domain services, and a SPA frontend.
- Platform/OS context: Windows development environment.

## Tech Stack
- Backend language: C#
- Backend framework: .NET 9 (`net9.0`)
- Backend web stack: ASP.NET Core Web API
- API gateway: YARP (`Yarp.ReverseProxy`)
- Application messaging: MediatR
- Persistence stack prepared: Entity Framework Core 9 + Pomelo MySQL provider
- Frontend: React 18 + TypeScript + Vite + Zustand + React Query + Tailwind/shadcn-ui

## Solution / Structure
- Solution file: `ecommerce/ecommerce-platform.slnx`
- Gateway: `ecommerce/gateway/ApiGateway`
- Services root: `ecommerce/services`
- Shared contracts: `ecommerce/shared/ECommerce.Shared`
- Frontend SPA: `ecommerce/spa`

## Backend Services
- `CatalogService` is the most mature backend service and follows a layered CQRS-oriented structure.
  - API hosts: `Catalog.API.Read` and `Catalog.API.Write`
  - Application: commands, queries, handlers, DTOs, interfaces
  - Domain: entities, enums, exceptions
  - Infrastructure: DbContexts, repositories, projector/read model persistence
  - Tests: `Catalog.Tests`
- `CartService` now exists as a single API host plus 3 layers:
  - `Cart.API`
  - `Cart.Application`
  - `Cart.Domain`
  - `Cart.Infrastructure`
  - It models carts by `ownerType + ownerId`, supporting guest and authenticated carts.

## Runtime Notes
- Gateway proxies `/api/catalog/*` to catalog read/write hosts and `/api/cart/*` to `http://localhost:5102/`.
- Gateway local host runs at `http://localhost:5100` (and HTTPS on `https://localhost:7060`).
- `Cart.API` local HTTP host is configured for `http://localhost:5102`.
- The SPA is intended to talk to the gateway rather than directly to each microservice.

## Current State / Caveats
- `CartService` code was added to the repo and solution, but local `dotnet restore/build` for the new Cart projects is currently blocked by an environment/filesystem permission issue when NuGet/MSBuild tries to create temp files under `obj`.
- The `CatalogService` remains the source for product/category data consumed by the SPA.
- The SPA previously used mock catalog/cart data; frontend integration work should prefer the gateway-backed APIs when available.
- Other service folders (`InventoryService`, `NotificationService`, `OrderService`, `PaymentService`) still exist but are not implemented in the repo.