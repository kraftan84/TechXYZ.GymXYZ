# Copilot instructions for TechXyz.GymXyz

## Scope
These instructions apply to the whole repository.

## Solution architecture
- The solution is a layered .NET 10 application:
  - `TechXyz.GymXyz.Domain`: entities and core business model only.
  - `TechXyz.GymXyz.Application`: CQRS use cases with MediatR commands/queries, validators, DTOs, and application helpers.
  - `TechXyz.GymXyz.Persistence`: EF Core/MySQL persistence, `GymDbContext`, conventions, initialization.
  - `TechXyz.GymXyz.WebApp`: Blazor Server UI using Fluent UI components.
- Keep dependencies one-way: `WebApp -> Application -> Domain`, and `Persistence -> Application + Domain`.
- Do not introduce UI or infrastructure dependencies into `Domain`.
- Do not add repository or Unit of Work abstractions. Application handlers should continue to depend on `IGymDbContext` directly.

## Project-wide coding conventions
- Target framework is `net10.0`.
- Nullable reference types are enabled; prefer null-safe code and explicit optionality.
- Follow the existing split-file CQRS structure:
  - `CreateXCommand.cs`
  - `CreateXCommand.Handler.cs`
  - `CreateXCommand.Validator.cs`
- Keep new files in architecture-aligned folders (`Commands`, `Queries`, `Common`, `Models`, `Components/Pages`, etc.).
- Preserve the current public API and route names unless the task explicitly requests a rename.
- Be careful with existing naming inconsistencies in the repository (`TechXYZ` vs `TechXyz`, `Coachs` vs `Coaches`). Do not spread them further unnecessarily, but do not silently rename existing public surfaces without an explicit request.

## Domain layer guidance
- Domain entities inherit from `EntityBase<T>` and therefore use `IsActive` for soft delete.
- Keep domain entities focused on state and business relationships.
- Avoid adding infrastructure-specific concerns to domain entities.
- Prefer extending existing domain methods (for example aggregate-style `AddX(...)` methods on `Gym` or `Location`) instead of duplicating relationship logic in upper layers.

## Application layer guidance
- Use MediatR handlers for all use cases.
- Validators use FluentValidation and should be registered by assembly scanning.
- Command handlers should:
  - call `ValidateAndThrowAsync(...)` first;
  - use `IGymDbContext` directly;
  - return `false` when an update/delete target is missing or inactive;
  - throw `ValidationException` for business validation failures;
  - call `SaveChangesAsync(...)` on `IGymDbContext`.
- Query handlers should:
  - use `AsNoTracking()` for read-only queries;
  - keep projection logic server-side in LINQ;
  - reuse projection helpers from `Application/Common` such as `QueryableProjectionExtensions`;
  - exclude inactive entities by default.
- For create operations attached to the root gym, reuse `GetDefaultGymAsync` / `GetRequiredDefaultGymAsync` rather than reimplementing the lookup.
- Reuse helper classes already present in `Application/Common` (for example `AddressHelper`) instead of duplicating normalization logic.

## Soft delete rules
- "Delete" means soft delete: set `IsActive = false`; do not physically remove rows.
- All list/detail queries must exclude inactive records unless a task explicitly requires administrative access to inactive data.
- Update operations must only target active records.
- Creation must only attach new records to active parent entities.
- Because the project does not currently use global EF Core query filters for `IsActive`, new queries must apply the filter explicitly.

## Persistence layer guidance
- `GymDbContext` is the central EF Core context and implements `IGymDbContext`.
- Keep EF Core configuration and relationship mapping inside `GymDbContext` unless there is already a stronger local convention for a specific concern.
- Preserve audit stamping behavior in `SaveChangesAsync` for `AuditableEntityBase` entities.
- Runtime database provider is MySQL; tests may use InMemory or SQLite.
- Service registration belongs in the existing `IServiceCollectionExtensions` files.
- Do not add a second abstraction over EF Core unless explicitly requested.

## WebApp guidance
- The WebApp should call the Application layer through `ISender`.
- User-visible actions should use `IUserFeedbackService` rather than surfacing raw exceptions.
- Prefer wrapping page actions in `UserFeedback.ExecuteAsync(...)` and converting not-found cases into user-friendly validation failures.
- Avoid unhandled exceptions during the component render lifecycle.
- Shared layout/providers belong in `MainLayout.razor`.
- Keep overlay/provider ordering compatible with the current layout (`FluentToastProvider`, `FluentDialogProvider`, etc.).
- Reuse existing patterns for breadcrumbs, drawers, and navigation.

## Testing guidance
- Test stack:
  - xUnit
  - Shouldly
  - Bogus
  - EF Core InMemory for fast unit-style handler tests
  - SQLite in-memory for relational/integration behavior
- Mirror production folder structure in tests.
- Add or update tests whenever you change handlers, validators, query filters, or user-visible behavior.
- For soft delete behavior, add tests that verify both the state change (`IsActive = false`) and exclusion from subsequent queries.
- Reuse `TestInfrastructure` and `RelationalTestInfrastructure` rather than creating ad hoc test setup.

## Review-driven watch-outs
- The root `README.md` mentions repositories, but the implemented pattern is direct `IGymDbContext` access from handlers. Follow the implementation, not the outdated wording.
- The solution assumes a default active gym exists for several create flows. If you change those flows, preserve or explicitly handle that assumption.
- Tests currently use a broad namespace (`TechXYZ.GymXYZ.Application.Tests.Members`) even outside member-specific files. Avoid mass renames unless requested; keep changes scoped.
- Existing UI text is mostly French while implementation instructions are English. Prefer matching the surrounding file/layer language and avoid mixing within the same artifact.

## Definition of done for changes
For any non-trivial change:
1. Respect layer boundaries.
2. Add or update validator/handler/query tests.
3. Apply `IsActive` filtering where relevant.
4. Update UI behavior and user feedback handling if the change is user-visible.
5. Run the impacted test project(s), or broader solution tests when the change crosses layers.
6. Avoid startup/runtime regressions in `TechXyz.GymXyz.WebApp`.



