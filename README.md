# PharmaCore Server

PharmaCore Server is an ASP.NET Core backend for pharmacy operations. It manages medicines, inventory, purchases, sales, POS lookup, payments, returns, expenses, reports, users, and system backup/restore.

## Tech Stack

- ASP.NET Core / .NET
- Entity Framework Core
- PostgreSQL via Npgsql
- JWT bearer authentication
- Swagger/OpenAPI and Scalar API reference

## Solution Layout

```text
PharmaCore.API/              HTTP controllers, auth setup, Swagger/Scalar, API contracts
PharmaCore.Application/      Use-case services, DTOs, requests, interfaces, pagination
PharmaCore.Domain/           Domain entities, enums, shared result types
PharmaCore.Infrastructure/   EF Core DbContext, repositories, security, system services
PharmaCore.Tests/            Lightweight executable service tests
others/                     Endpoint specs, schema scripts, implementation notes
```

See [architecture.md](architecture.md) for the layer-by-layer architecture.

## Prerequisites

- .NET SDK capable of building the solution
- PostgreSQL
- `dotnet-ef` if you use EF tooling

Restore local tools:

```bash
dotnet tool restore
```

## Configuration

Copy the environment template and set real values:

```bash
cp .env.example .env
```

Important settings:

```text
ConnectionStrings__Default="Host=localhost;Port=5432;Database=pharma_core;Username=postgres;Password=..."
Jwt__SecretKey="at-least-32-characters-long"
```

Default appsettings live in [PharmaCore.API/appsettings.json](PharmaCore.API/appsettings.json). Do not commit real secrets.

## Database

Create the PostgreSQL database:

```sql
CREATE DATABASE pharma_core;
```

The SQL schema script is in [others/databaseCreate.txt](others/databaseCreate.txt). Apply it to the database before running the API if migrations are not being used in your local workflow.

## Run The API

```bash
dotnet run --project PharmaCore.API/PharmaCore.API.csproj
```

Default development URLs from `launchSettings.json`:

- HTTP: `http://localhost:5084`
- HTTPS: `https://localhost:7037`

Development API docs:

- Swagger UI: `/swagger`
- Swagger JSON: `/swagger/v1/swagger.json`
- Scalar API reference: `/scalar/v1`

The configured CORS frontend origin is currently `http://localhost:5000`.

## Authentication

Most endpoints require a JWT.

1. Create or seed a user with a valid password hash.
2. Call `POST /auth/login`.
3. Send the returned token as:

```http
Authorization: Bearer <token>
```

Logout uses token revocation:

```http
POST /auth/logout
```

## Endpoint Overview

Auth:

- `POST /auth/login`
- `POST /auth/logout`
- `GET /auth/me`

Users:

- `GET /users`
- `GET /users/deleted`
- `POST /users`
- `PUT /users/{id}`
- `DELETE /users/{id}`
- `POST /users/{id}/restore`
- `DELETE /users/{id}/hard`

Catalog:

- `GET /medicines`, `GET /medicines/{id}`, `GET /medicines/search`
- `POST /medicines`, `PUT /medicines/{id}`, `DELETE /medicines/{id}`
- `GET /medicines/deleted`, `POST /medicines/{id}/restore`, `DELETE /medicines/{id}/hard`
- `GET /categories`, `GET /categories/{id}`, `POST /categories`, `PUT /categories/{id}`, `DELETE /categories/{id}`
- `GET /categories/deleted`, `POST /categories/{id}/restore`, `DELETE /categories/{id}/hard`
- `GET /suppliers`, `GET /suppliers/{id}`, `POST /suppliers`, `PUT /suppliers/{id}`, `DELETE /suppliers/{id}`
- `GET /customers`, `GET /customers/{id}`, `POST /customers`, `PUT /customers/{id}`, `DELETE /customers/{id}`

Inventory and POS:

- `GET /inventory/stock`
- `GET /inventory/stock/{medicineId}`
- `GET /inventory/batches/{medicineId}`
- `GET /inventory/low-stock`
- `GET /inventory/expiring`
- `POST /inventory/adjust`
- `GET /pos/search`
- `GET /pos/scan/{barcode}`
- `GET /pos/quick-stock/{medicineId}`

Purchases:

- `GET /purchases`
- `GET /purchases/{id}`
- `POST /purchases`
- `PUT /purchases/{id}`
- `DELETE /purchases/{id}`
- `POST /purchases/{id}/items`
- `PUT /purchases/{id}/items/{itemId}`
- `DELETE /purchases/{id}/items/{itemId}`
- `POST /purchases/{id}/complete`
- `POST /purchases/{id}/cancel`
- `POST /purchases/{id}/pay`
- `POST /purchases/{id}/return`
- `GET /purchases/{id}/balance`
- `GET /purchases/{id}/items`
- `GET /purchases/{id}/returns`

Sales and returns:

- `GET /sales`
- `GET /sales/{id}`
- `POST /sales`
- `POST /sales/{id}/items`
- `PUT /sales/{id}/items/{itemId}`
- `DELETE /sales/{id}/items/{itemId}`
- `POST /sales/{id}/complete`
- `POST /sales/{id}/cancel`
- `GET /sales/{id}/balance`
- `POST /sales/{saleId}/return`
- `GET /sales/{saleId}/returns`
- `GET /sales-returns`
- `GET /sales-returns/{id}`
- `POST /sales-returns`
- `PUT /sales-returns/{id}`
- `DELETE /sales-returns/{id}`
- `POST /sales-returns/{id}/items`
- `PUT /sales-returns/{id}/items/{itemId}`
- `DELETE /sales-returns/{id}/items/{itemId}`

Payments, expenses, reports, system:

- `POST /payments`, `GET /payments`, `GET /payments/{id}`
- `GET /payments/sale/{saleId}`, `GET /payments/purchase/{purchaseId}`
- `GET /expenses`, `POST /expenses`, `DELETE /expenses/{id}`
- `GET /reports/sales/daily`
- `GET /reports/sales/range`
- `GET /reports/profit`
- `GET /reports/stock`
- `GET /reports/expired`
- `GET /reports/payments`
- `GET /health`
- `POST /backup`
- `POST /restore`

Detailed endpoint specs are in [others/endpoints.jsonc](others/endpoints.jsonc).

## Build And Test

Build the application layer:

```bash
dotnet build PharmaCore.Application/PharmaCore.Application.csproj --no-restore
```

Run the lightweight service tests:

```bash
dotnet run --project PharmaCore.Tests/PharmaCore.Tests.csproj
```

Build the API:

```bash
dotnet build PharmaCore.API/PharmaCore.API.csproj
```

## Development Notes

- Controllers should stay thin: bind input, call an application service, map `ServiceResult<T>`.
- Application services own workflow validation and orchestration.
- Repositories own EF Core queries and model/domain mapping.
- Domain entities should stay persistence-agnostic.
- Prefer soft delete plus restore for operational records. Use hard delete only where explicitly supported.
- When changing stock, payments, sales, purchases, or returns, add focused service tests.

## Security Notes

- Never commit `.env`, local appsettings, real connection strings, or real JWT secrets.
- Use a strong JWT key of at least 32 characters.
- Treat `/backup` and `/restore` as privileged operations.
- Passwords must be stored as hashes only.
