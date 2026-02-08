# TechXyz.GymXyz.WebApp

## Overview
Blazor/ASP.NET Core web application for GymXyz. This is the entry point for hosting the UI and composing the application.

## What This Is For
- Blazor components, layouts, and pages.
- Application composition (DI, configuration, middleware).
- Presentation-level validation and UX behavior.

## What Does Not Belong Here
- Core business rules (belong in Domain).
- Data access implementations (belong in Persistence).

## Dependencies
- TechXyz.GymXyz.Application
- TechXyz.GymXyz.Domain
- TechXyz.GymXyz.Persistence
- Microsoft.FluentUI.AspNetCore.Components

## Build
```bash
dotnet build TechXyz.GymXyz.WebApp/TechXyz.GymXyz.WebApp.csproj
```

## Run
```bash
dotnet run --project TechXyz.GymXyz.WebApp
```

## Test
```bash
dotnet test TechXyz.GymXyz.WebApp/TechXyz.GymXyz.WebApp.csproj
```

## Configuration
- Application settings are provided through standard .NET configuration providers.
- Environment-specific settings should be isolated in `appsettings.{Environment}.json`.
