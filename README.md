# TechXyz.GymXyz Solution

## Overview
This solution contains the GymXyz web application along with supporting domain, application, and persistence projects. The structure keeps business rules isolated from infrastructure so the system is easier to test and evolve.

## Architecture
- Domain is the source of truth for business concepts and rules.
- Application orchestrates use cases and coordinates domain logic.
- Persistence implements data access details (EF Core, database mapping).
- WebApp hosts the UI and application composition.

## Projects
- TechXyz.GymXyz.WebApp: Blazor/ASP.NET Core web app. Hosts UI, pages, and presentation logic.
- TechXyz.GymXyz.Application: Use cases, orchestration, and cross-cutting behaviors. Coordinates domain logic without UI concerns.
- TechXyz.GymXyz.Domain: Core domain models, entities, and business rules. Free of infrastructure and framework dependencies.
- TechXyz.GymXyz.Persistence: Data access and EF Core integration. Responsible for database contexts, mappings, and repositories.

## Prerequisites
- .NET SDK 10.x (targets net10.0).

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
- Configuration is supplied via standard .NET providers (appsettings, environment variables, user secrets).
- Connection strings and hosting settings should be defined in the WebApp configuration.

## Notes
- Keep domain logic in the Domain project, and call it from Application services.
- Infrastructure changes should be isolated to Persistence and WebApp where possible.
