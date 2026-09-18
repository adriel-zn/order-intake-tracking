# Order Intake Backend

The backend is built with .NET 8 and uses a DDD-style structure with SQL Server and EF Core 8.

## Project structure

| Project | Responsibility |
| --- | --- |
| `OrderIntake.Domain` | Order rules, validation, status transitions, and repository contracts |
| `OrderIntake.Application` | Order use cases such as create, get, list, and update status |
| `OrderIntake.Infrastructure` | EF Core, SQL Server, repositories, and migrations |
| `OrderIntake.Api` | Controllers, DTOs, validation, Swagger, and dependency setup |
| `OrderIntake.Api.Tests` | Domain, application, API, and optional SQL Server integration tests |

The API depends on the Application and Infrastructure layers.

Customer details are stored as part of the order rather than in a separate customer table.

## Run locally

### Prerequisites

- .NET 8 SDK
- SQL Server
- PowerShell

From the `backend` directory:

```powershell
dotnet restore OrderIntake.sln
dotnet tool restore

$env:ASPNETCORE_ENVIRONMENT = 'Development'

dotnet ef database update `
  --project src/OrderIntake.Infrastructure `
  --startup-project src/OrderIntake.Api

dotnet run --project src/OrderIntake.Api --launch-profile http
```

Open Swagger at:

```text
http://localhost:5251/swagger
```

By default, development uses:

```text
(localdb)\MSSQLLocalDB
```

with database:

```text
OrderIntake
```

To use another SQL Server instance, configure:

```text
ConnectionStrings__OrderIntake
```

Production should provide its own connection string and database credentials.

## Migrations

Create a migration:

```powershell
dotnet ef migrations add YourChange `
  --project src/OrderIntake.Infrastructure `
  --startup-project src/OrderIntake.Api `
  --output-dir Persistence/Migrations
```

Check for model changes:

```powershell
dotnet ef migrations has-pending-model-changes `
  --project src/OrderIntake.Infrastructure `
  --startup-project src/OrderIntake.Api
```

Generate an idempotent SQL script:

```powershell
dotnet ef migrations script --idempotent `
  --project src/OrderIntake.Infrastructure `
  --startup-project src/OrderIntake.Api `
  --output migration.sql
```

Database migrations are applied explicitly rather than automatically when the API starts.

## Behavior

- Domain rules are enforced independently of the API layer.
- `externalReference` is unique and case-insensitive.
- Submitting the same reference with the same order details returns the existing order.
- Reusing the same reference with different details returns `409 Conflict`.
- SQL Server `rowversion` protects against conflicting status updates.
- Repeating the current status is treated as a successful no-op.
- Totals are calculated from the stored line items.
- Invalid price values are rejected with `400 Bad Request`.
- EF Core scoped repositories replace the previous in-memory production storage.
- `/health` checks that the API process is running, but does not check database connectivity.

## Tests

Run all tests with:

```powershell
dotnet test OrderIntake.sln
```

SQL Server integration tests are skipped by default.

To enable them:

```powershell
$env:OrderIntake_TestSqlConnection = 'Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True'

dotnet test OrderIntake.sln
```

The integration tests create their own temporary databases, apply migrations, run the tests, and delete the databases afterwards.

Tests cover:

- Order persistence
- Duplicate reference handling
- Concurrent submissions
- Status update conflicts
- API responses
- Swagger