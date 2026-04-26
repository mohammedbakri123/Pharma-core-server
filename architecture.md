# PharmaCore Server Architecture

## Overview

PharmaCore is an ASP.NET Core backend for pharmacy inventory, sales, purchases, payments, returns, reporting, and system maintenance. The solution follows a layered architecture with clear boundaries between HTTP concerns, application use cases, domain rules, and infrastructure persistence/security.

## Projects

### PharmaCore.API

The API project exposes the HTTP surface.

- ASP.NET Core controllers define routes, request binding, authorization, response shaping, and OpenAPI metadata.
- Controllers depend on application service interfaces through dependency injection.
- JWT authentication, CORS, Swagger/Scalar, and common result mapping are configured here.

### PharmaCore.Application

The application project contains use-case orchestration.

- Services implement business workflows such as creating purchases, completing sales, processing returns, paying customer debt, and generating reports.
- Request records, DTOs, pagination contracts, and service interfaces live in this layer.
- Repository, password hashing, token, and system abstractions are defined here so application logic does not depend on infrastructure details.

### PharmaCore.Domain

The domain project contains core entities and enums.

- Entities validate their own invariants and expose behavior for valid state changes.
- Enums model stable business concepts such as payment type, stock movement type, user role, sale status, and purchase status.
- Domain classes are independent of ASP.NET Core, Entity Framework Core, and database models.

### PharmaCore.Infrastructure

The infrastructure project implements technical details.

- Entity Framework Core `ApplicationDbContext` maps the PostgreSQL schema.
- Repository implementations translate between database models and domain entities.
- Security services implement PBKDF2 password hashing, JWT creation, and in-memory token revocation.
- System services provide health check, backup, and restore workflows.

## Runtime Stack

- .NET / ASP.NET Core
- Entity Framework Core
- PostgreSQL via Npgsql
- JWT bearer authentication
- Swagger/OpenAPI and Scalar for API documentation

## Request Flow

1. A client calls an API route with JSON and, for protected routes, a JWT.
2. The controller binds route/query/body values and calls the matching application service.
3. The application service validates input, coordinates repositories, and applies workflow rules.
4. Repositories load or persist data through EF Core and map database models to domain entities.
5. The service returns a `ServiceResult<T>`.
6. The controller maps the result to the correct HTTP status and response body.

## Main Functional Areas

- **Auth:** login, logout/token revocation, current-user lookup.
- **Users:** active/deleted listing, create, update, soft delete, restore, hard delete.
- **Catalog:** medicines, categories, suppliers, and customers.
- **Inventory:** stock, batches, low-stock/expiring alerts, manual adjustments.
- **Purchases:** draft purchases, item management, completion, supplier payments, returns, balance.
- **Sales and POS:** draft sales, item management, FEFO completion, cancellation, balance, POS search/scan/stock.
- **Payments:** incoming/outgoing payments for supported reference types.
- **Returns:** sales returns and purchase returns with stock movement/payment effects.
- **Reports:** daily/range sales, profit, stock, expired items, and payments summaries.
- **System:** health check, database backup, and restore.

## Cross-Cutting Concerns

- **Authentication and authorization:** JWT bearer authentication is enforced by protected controllers.
- **Validation:** controllers handle API-level shape; application services validate business inputs.
- **Error mapping:** `ServiceResult<T>` centralizes application outcomes and controller status mapping.
- **Persistence boundaries:** application services depend on repository interfaces, not EF Core.
- **Soft delete:** core records generally use `IsDeleted`/`DeletedAt` and dedicated restore/hard-delete workflows where supported.

## Operational Notes

- Keep secrets outside source control. Use `.env`, user secrets, or environment variables for connection strings and JWT secrets.
- Keep `.env.example` as the public template only.
- Treat backup/restore endpoints as privileged operations.
- Add focused service or integration tests around stock, payment, and return workflows when changing those paths.
