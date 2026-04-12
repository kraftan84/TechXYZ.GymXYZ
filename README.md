# TechXyz.GymXyz Solution

## Overview
TechXyz.GymXyz is a layered .NET 10 solution for gym management. It uses a Blazor Server front end, MediatR-based CQRS application flows, and EF Core persistence backed by MySQL.

The solution is structured to keep domain logic isolated, application use cases explicit, and UI/infrastructure concerns separated.

## Architecture
- `TechXyz.GymXyz.Domain`
  - Core entities and business model only.
  - No UI or infrastructure dependencies.
- `TechXyz.GymXyz.Application`
  - Commands, queries, handlers, validators, DTOs, and application helpers.
  - Uses MediatR and FluentValidation.
  - Handlers depend directly on `IGymDbContext`.
- `TechXyz.GymXyz.Persistence`
  - EF Core `GymDbContext`, conventions, relationship mapping, and database initialization.
  - Runtime provider is MySQL.
- `TechXyz.GymXyz.WebApp`
  - Blazor Server UI.
  - Uses Fluent UI components.
  - Calls the Application layer via `ISender`.

### Dependency direction
- `WebApp -> Application -> Domain`
- `Persistence -> Application + Domain`

Do not introduce repository or Unit of Work abstractions on top of the current EF Core pattern unless explicitly required. The implemented approach is direct `IGymDbContext` access from application handlers.

## Solution conventions

### CQRS and validation
- Use MediatR for application use cases.
- Keep the existing split-file pattern for commands:
  - `CreateXCommand.cs`
  - `CreateXCommand.Handler.cs`
  - `CreateXCommand.Validator.cs`
- Command handlers validate first with `ValidateAndThrowAsync(...)`.
- Query handlers should use `AsNoTracking()` and keep projection server-side in LINQ.
- Reuse existing helpers from `Application/Common` where possible.

### Soft delete
- Entities inherit from `EntityBase<T>` and use `IsActive`.
- Delete operations are soft delete (`IsActive = false`).
- New queries must explicitly filter inactive records unless the feature intentionally needs inactive data.

### WebApp behavior
- UI actions should go through `ISender`.
- User-facing success/error handling should use `IUserFeedbackService`.
- Shared layout providers are hosted in `MainLayout.razor`.

## Projects
- `TechXyz.GymXyz.WebApp`
  - ASP.NET Core / Blazor Server application and UI composition.
- `TechXyz.GymXyz.Application`
  - CQRS handlers, validators, queries, commands, models, and helpers.
- `TechXyz.GymXyz.Domain`
  - Core domain entities and relationships.
- `TechXyz.GymXyz.Persistence`
  - EF Core context and persistence integration.
- `TechXYZ.GymXYZ.Application.Tests`
  - Application handler/query tests using InMemory and SQLite.
- `TechXYZ.GymXYZ.Domain.Tests`
  - Domain-focused tests.
- `TechXYZ.GymXYZ.Persistence.Tests`
  - Persistence-focused tests.
- `TechXYZ.GymXYZ.WebApp.Tests`
  - WebApp/service tests.

## Prerequisites
- .NET SDK 10.x (`net10.0`)
- A MySQL instance for normal runtime execution

## Build
```bash
dotnet build TechXyz.GymXyz.sln
```

## Run
```bash
dotnet run --project TechXyz.GymXyz.WebApp
```

## Test
```bash
dotnet test TechXyz.GymXyz.sln
```

## Configuration
- Configuration is supplied through standard .NET configuration providers.
- Connection strings are configured from the WebApp host.
- `ConnectionStrings:GymXyzDb` must be set for the runtime database.
- `ResetDatabaseOnStartup` controls whether the development database is reset and reinitialized at startup.

## Testing notes
- Main test stack:
  - xUnit
  - Shouldly
  - Bogus
  - EF Core InMemory for fast handler tests
  - SQLite in-memory for relational/integration behavior
- Prefer reusing the shared test infrastructure already present in the test projects.

## Contributor notes
- Follow `IMPLEMENTATION_INSTRUCTIONS.md` for implementation conventions.
- Follow `.github/copilot-instructions.md` for repository-specific Copilot guidance.
- Be aware of existing naming inconsistencies such as `TechXYZ` vs `TechXyz` and `Coachs` vs `Coaches`; avoid broad renames unless explicitly requested.
