# Integration Tests

This project validates communication across microservices through the public API gateway.

## Current coverage

- `Auth`: a registered user can authenticate through `AuthService`.
- `Auth -> Customer`: registering a user through `AuthService` must eventually create the customer record in `CustomerService`.
- `Customer`: a registered customer can create, update, list, set default, and remove addresses through the public API.
- `Order -> Inventory -> Customer`: an order with a reservable product and a missing address must be rejected during asynchronous processing.
- `Order -> Inventory`: an order with an unknown product must be rejected during inventory validation.
- `Order`: an order with a valid customer address and a reservable product must be persisted as `PendingPayment`.

## Default target

By default, the tests call:

- `http://localhost:5100`

This matches the API gateway exposed by `docker-compose.yml`.

## Configuration

You can override the defaults with environment variables:

- `IntegrationTests__GatewayBaseUrl`
- `IntegrationTests__RequestTimeoutSeconds`
- `IntegrationTests__ConsistencyTimeoutSeconds`
- `IntegrationTests__PollIntervalMilliseconds`

## Expected environment

Before running the suite, start the platform dependencies and APIs so the gateway, Kafka and the participating services are available.
