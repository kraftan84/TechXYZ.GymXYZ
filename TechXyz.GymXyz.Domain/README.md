# TechXyz.GymXyz.Domain

## Overview
Core domain models, entities, and business rules. This is the heart of the system and should remain free of external frameworks and infrastructure dependencies.

## What This Is For
- Entities, value objects, and aggregates.
- Domain services that express business policies.
- Invariants and validation rules that must always hold.

## What Does Not Belong Here
- EF Core attributes or database mappings.
- Web/Blazor code, controllers, or UI components.
- Application orchestration and cross-cutting concerns.

## Dependencies
- None

## Build
```bash
dotnet build TechXyz.GymXyz.Domain/TechXyz.GymXyz.Domain.csproj
```

## Test
```bash
dotnet test TechXyz.GymXyz.Domain/TechXyz.GymXyz.Domain.csproj
```

## Notes
- Keep this project free of infrastructure concerns.
- Favor rich domain models over anemic data structures.
