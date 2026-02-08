# TechXyz.GymXyz.Application

## Overview
Application layer responsible for orchestration, use cases, and cross-cutting behaviors. This project coordinates domain logic and defines application-level contracts without depending on UI or infrastructure details.

## What This Is For
- Use case implementations (commands, queries, workflows).
- Application services that coordinate domain entities and policies.
- Interfaces for persistence or external services that will be implemented elsewhere.

## What Does Not Belong Here
- UI components or web-specific code.
- Database/EF Core implementations.
- Infrastructure concerns (files, email providers, external APIs).

## Dependencies
- TechXyz.GymXyz.Domain
- MediatR
- Microsoft.Extensions.DependencyInjection.Abstractions

## Build
```bash
dotnet build TechXyz.GymXyz.Application/TechXyz.GymXyz.Application.csproj
```

## Test
```bash
dotnet test TechXyz.GymXyz.Application/TechXyz.GymXyz.Application.csproj
```

## Notes
- Designed as a class library; it is consumed by other projects.
- Keep application-level validation and orchestration here to protect the domain from infrastructure concerns.
