# TechXYZ.GymXYZ Implementation Instructions

## Purpose
This file defines the implementation conventions for this solution.  
Use it as the default checklist when adding features, refactoring, or fixing bugs.

## Solution Architecture
- `TechXyz.GymXyz.Domain`
  - Entities and core domain model only.
  - No infrastructure/UI dependencies.
- `TechXyz.GymXyz.Application`
  - Use cases via `Commands` and `Queries` (MediatR handlers).
  - Input validation via FluentValidation validators (`*.Validator.cs`).
  - Application helpers/extensions in `Common`.
- `TechXyz.GymXyz.Persistence`
  - EF Core `GymDbContext`, mappings/conventions, data initialization.
  - Implements `IGymDbContext`.
- `TechXyz.GymXyz.WebApp`
  - Blazor UI, app composition, user feedback services, routing/layout.
  - Should call Application layer through `ISender`.

## Current Data Access Rules
- No UnitOfWork/Repository abstraction layer.
- Handlers depend on `IGymDbContext`.
- Save through `SaveChangesAsync` on `IGymDbContext`.
- Use `AsNoTracking()` for read-only queries.

## Soft Delete Convention
- All entities inherit `EntityBase<T>` and use `IsActive`.
- "Delete" operations are soft delete (`IsActive = false`), not physical removal.
- All list/detail queries must exclude inactive records by default.
- Update operations must target active records only.
- Creation must attach new records to active parents only.

## Command/Query Patterns
- Command handlers:
  - Validate request first (`ValidateAndThrowAsync`).
  - Return `false` when target does not exist or is inactive.
  - Throw `ValidationException` for business validation failures.
- Query handlers:
  - Keep projection logic server-side in LINQ.
  - Reuse projection extensions from `Application/Common` when possible.

## WebApp Conventions
- Pages use `IUserFeedbackService` for user-visible errors/success.
- Avoid unhandled exceptions during component render lifecycle.
- Shared layout/providers live in `MainLayout.razor`.
- Keep toast/dialog layers above app shell overlays and below visual conflicts.

## Startup & Environment
- `ResetDatabaseOnStartup` controls development reset behavior.
- Do not silently delete/recreate DB unless explicitly enabled.

## Testing Conventions
- Testing libraries:
  - Assertions: `Shouldly`
  - Data generation: `Bogus`
- Folder organization should mirror production structure.
- Use:
  - InMemory tests for fast handler logic checks.
  - Relational provider tests (SQLite) for FK/translation/integration behavior.

## Change Checklist (Definition of Done)
For each feature/fix:
1. Respect layer boundaries.
2. Add/update validator and handler tests.
3. Add query filtering for `IsActive` when relevant.
4. Update UI behavior/tests if user-visible behavior changes.
5. Run impacted test projects (or solution tests when broad changes).
6. Ensure no startup/runtime regression in WebApp.

## Naming & Structure
- Keep existing naming style (`CreateXCommand`, `UpdateXCommand`, `GetXQuery`, etc.).
- Keep tests close to current style and naming (`...HandlerTests`).
- Place new files in architecture-aligned folders.
