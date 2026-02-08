# TechXyz.GymXyz.Persistence

## Overview
Persistence layer providing data access and EF Core integration. This project contains the database-facing implementation of contracts defined in the Application layer.

## What This Is For
- DbContext and entity configurations.
- Repository implementations and query helpers.
- Migrations and database initialization logic.

## What Does Not Belong Here
- UI, pages, or web endpoints.
- Business rules that belong in the Domain layer.

## Dependencies
- TechXyz.GymXyz.Application
- TechXyz.GymXyz.Domain
- Microsoft.EntityFrameworkCore
- MySql.EntityFrameworkCore

## Build
```bash
dotnet build TechXyz.GymXyz.Persistence/TechXyz.GymXyz.Persistence.csproj
```

## Test
```bash
dotnet test TechXyz.GymXyz.Persistence/TechXyz.GymXyz.Persistence.csproj
```

## Configuration
- Provide database connection settings through standard .NET configuration providers.
- Migrations are typically run from the WebApp project using this library.
