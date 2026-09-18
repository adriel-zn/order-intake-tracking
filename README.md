# Order Intake & Tracking

A small internal tool for creating customer orders and tracking their status.

- **Backend:** ASP.NET Core Web API (.NET 8)
- **Frontend:** Angular 18
- **Storage:** In-memory

See [SOLUTION.md](./SOLUTION.md) for design decisions and trade-offs.

## Prerequisites

- .NET 8 SDK
- Node.js 20+
- npm

## Run the backend

```bash
cd backend/src/OrderIntake.Api
dotnet run
```

The API runs at:

```text
http://localhost:5251
```

Main endpoints:

```text
GET    /health
GET    /api/orders
GET    /api/orders/{id}
POST   /api/orders
PATCH  /api/orders/{id}/status
```

Sample API requests are available in:

```text
backend/src/OrderIntake.Api/OrderIntake.Api.http
```

### Backend tests

```bash
cd backend
dotnet test
```

The tests cover order creation, duplicate handling, totals, validation, and status transitions.

## Run the frontend

In a separate terminal:

```bash
cd frontend
npm install
npm start
```

Open:

```text
http://localhost:4200
```

The frontend connects to the API at:

```text
http://localhost:5251/api
```

### Frontend tests

```bash
cd frontend
npm test
```

## Quick walkthrough

1. Start the backend and frontend.
2. Open `http://localhost:4200`.
3. Create a new order.
4. Submit the same order again to see duplicate handling.
5. Reuse the same reference with different details to see a conflict.
6. Select an order to view its details and update its status.

## Package restore note

The project was developed in a restricted environment without access to `nuget.org`.

Because of this:

- The API uses only built-in ASP.NET Core dependencies.
- Swagger was not added.
- The xUnit test project could not be restored or run in the sandbox.

The business logic was still verified using a dependency-free test harness, direct API requests with `curl`, and end-to-end testing with Playwright.

On a normal machine with internet access, the xUnit tests can be run with:

```bash
cd backend
dotnet test
```