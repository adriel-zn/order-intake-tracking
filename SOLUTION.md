# Solution Notes

## Overview

This project is a small order management system with:

- A .NET 8 Web API backend
- An Angular 18 frontend
- xUnit tests for the backend

A sales rep can create an order, view existing orders, and update an order’s status.

The main business rule is preventing duplicate orders when the same `externalReference` is submitted more than once.

## Data Model

An order contains:

- A server-generated `id`
- An `externalReference`
- Customer details
- Line items
- Currency
- Notes
- Status
- Created and updated timestamps
- Server-calculated subtotal and total

Customer details are stored directly on the order rather than in a separate customer table. This keeps the solution simple for the scope of the assignment.

Each line item contains a SKU, name, quantity, unit price, and calculated line total.

For now, `Total` is the same as `Subtotal` because tax, shipping, and discounts are outside the scope.

## Duplicate Orders

`externalReference` is used as the idempotency key.

When creating an order:

- A new reference creates a new order and returns `201 Created`.
- The same reference with the same order details returns the existing order with `200 OK`.
- The same reference with different order details returns `409 Conflict`.

This prevents accidental duplicate submissions while also preventing the same reference from being reused for a different order.

Notes are not included when comparing duplicate orders.

Concurrent duplicate requests are protected by a lock, so only one order is created even if several identical requests arrive at the same time.

## Order Status

Orders follow this lifecycle:

`Pending → Confirmed → Fulfilled`

An order can also be cancelled from `Pending` or `Confirmed`.

`Fulfilled` and `Cancelled` are final states.

Invalid status changes return `409 Conflict`.

Requesting the order’s current status again is treated as a successful no-op.

## Validation and Errors

The API validates:

- Required fields
- Positive quantities
- Non-negative prices
- Three-letter currency codes
- Email format

Invalid request data returns `400 Bad Request`.

Other errors use standard `ProblemDetails` responses:

- `404` for missing orders
- `409` for invalid status changes or duplicate reference conflicts

## Storage

Orders are stored in memory using an `InMemoryOrderRepository`.

The repository implements `IOrderRepository`, so it could later be replaced with a database implementation without changing the service or controller layers.

The current approach is suitable for a demo, but data is lost when the application restarts and it does not support multiple API instances.

## API

The API provides four endpoints:

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/orders` | Create an order |
| GET | `/api/orders` | List orders |
| GET | `/api/orders/{id}` | Get an order |
| PATCH | `/api/orders/{id}/status` | Update order status |

The order list is returned newest first and can optionally be filtered by status.

## Frontend

The frontend uses Angular 18 with standalone components and signals.

The main pieces are:

- `OrderService` — handles API requests
- `OrderStoreService` — manages UI state and API errors
- Order form — creates orders
- Order list — displays and filters orders
- Order detail — displays an order and allows valid status changes

The UI only shows status actions that are valid for the current order, although the backend still validates every request.

Routing was not added because all three views fit naturally on a single page.

## Testing

Backend tests cover:

- Status transitions
- Order creation and totals
- Duplicate order handling
- Concurrent duplicate submissions
- Listing and filtering
- Status updates
- Request validation

The sandbox could not access NuGet, so `dotnet test` could not restore xUnit packages.

Instead, the same scenarios were tested using a dependency-free console test harness. All 54 checks passed.

The API was also tested manually with `curl`, and the Angular user flow was tested with Playwright.

Frontend unit tests were not included because they were optional for the assignment.

## Trade-offs

For a production version, I would add:

- A real database
- Authentication and authorization
- Pagination
- A separate customer entity
- Tax, shipping, and discount support
- Swagger/OpenAPI documentation

These were left out to keep the solution focused on the assignment requirements.