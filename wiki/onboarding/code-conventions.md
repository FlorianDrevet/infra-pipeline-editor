# Code Conventions

## General Principles

| Principle | Application |
|-----------|-------------|
| **SOLID** | Single responsibility per class, dependency injection everywhere |
| **DRY** | Shared helpers for repeated patterns, but not premature abstraction |
| **KISS** | Simplest solution that satisfies requirements |
| **YAGNI** | Don't build what isn't needed yet |
| **Explicit over implicit** | No magic strings, no hidden behavior |

---

## C# / .NET Conventions

### Naming

| Element | Convention | Example |
|---------|-----------|---------|
| Class | PascalCase | `ContainerAppRepository` |
| Interface | IPascalCase | `IContainerAppRepository` |
| Method | PascalCase | `GetByIdAsync()` |
| Property | PascalCase | `ResourceGroupId` |
| Private field | _camelCase | `_repository` |
| Parameter | camelCase | `resourceId` |
| Constant | PascalCase | `MaxRetryCount` |
| Enum value | PascalCase | `ManagedIdentity` |

### File Organization

- **One public top-level type per file** — Never create "catch-all" files like `Dtos.cs`, `Models.cs`, or `Helpers.cs`
- **File name matches type name** — `ContainerApp.cs` contains `class ContainerApp`
- **Namespace matches folder path** — `InfraFlowSculptor.Domain.ProjectAggregate` for files in `Domain/ProjectAggregate/`

### Required Patterns

```csharp
// ✅ Sealed aggregates (required for DDD)
public sealed class ContainerApp : AzureResource { }

// ✅ XML documentation on public types
/// <summary>
/// Represents a Container App resource in the Azure resource model.
/// </summary>
public sealed class ContainerApp : AzureResource { }

// ✅ Value objects with private set
public sealed class Location : ValueObject
{
    public string Region { get; private set; }
}

// ✅ No magic strings — use constants
public static class AzureResourceTypes
{
    public const string ContainerApp = "ContainerApp";
}

// ✅ ErrorOr result pattern
public async Task<ErrorOr<ProjectResult>> Handle(...)
{
    var project = await _repository.GetByIdAsync(id);
    if (project is null)
        return Errors.Project.NotFound(id);
    return new ProjectResult(...);
}
```

### Forbidden Patterns

```csharp
// ❌ Magic strings
if (resource.Type == "ContainerApp") // WRONG
if (resource.Type == AzureResourceTypes.ContainerApp) // CORRECT

// ❌ Weak typing
Dictionary<string, object> settings = ...; // WRONG
ContainerAppSettings settings = ...; // CORRECT

// ❌ SaveChanges in repositories
public async Task UpdateAsync(Project project)
{
    _context.Update(project);
    await _context.SaveChangesAsync(); // WRONG — Unit of Work handles this
}

// ❌ Multiple public types per file
public class ProjectResult { }
public class ProjectListResult { } // WRONG — separate files

// ❌ EF Core LINQ value comparison
.Where(x => x.Id.Value == id.Value) // WRONG
.Where(x => x.Id == id) // CORRECT
```

---

## Angular / TypeScript Conventions

### Component Structure

- **One class per file** — Each component, service, or interface in its own file
- **Standalone components** — No `NgModule` declarations
- **Signals for state** — `signal()`, `computed()`, `toSignal()`
- **Inject function** — `inject(MyService)` instead of constructor injection
- **Separate template/styles** — `.ts`, `.html`, `.scss` files

### Naming

| Element | Convention | Example |
|---------|-----------|---------|
| Component | kebab-case selector | `app-resource-edit` |
| Service | PascalCase + Service | `ProjectService` |
| Interface | PascalCase + suffix | `ProjectResponse` |
| Enum | PascalCase | `ResourceType` |
| File | kebab-case | `project.service.ts` |

### Required Patterns

```typescript
// ✅ Signals for reactive state
private readonly projects = signal<ProjectResponse[]>([]);
public readonly projectCount = computed(() => this.projects().length);

// ✅ Strong typing
interface ProjectResponse {
  id: string;
  name: string;
  layoutPreset: LayoutPreset;
}

// ✅ Design System components
<app-ds-button (clicked)="onSave()">Save</app-ds-button>

// ✅ Extracted constants for repeated literals
export const RESOURCE_TYPES_WITH_ENVIRONMENT_SETTINGS = [...];
```

### Forbidden Patterns

```typescript
// ❌ any type
const data: any = response; // WRONG

// ❌ Direct Material component usage (use DS wrappers)
<mat-button>Click</mat-button> // WRONG
<app-ds-button>Click</app-ds-button> // CORRECT

// ❌ Hardcoded strings in templates
<span>Container App</span> // WRONG — use i18n
<span>{{ 'RESOURCES.CONTAINER_APP' | translate }}</span> // CORRECT
```

---

## Critical Pitfalls (Must Read)

These are the most common mistakes. Memorize them:

| # | Pitfall | Correct Approach |
|---|---------|-----------------|
| 1 | `x.Id.Value == id.Value` in EF LINQ | Use `x.Id == id` |
| 2 | `(object?)x` or `is not null` in Mapster | Use `x != null` |
| 3 | Calling `SaveChangesAsync()` in repositories | Unit of Work does it |
| 4 | Hardcoded Azure resource type strings | Use `AzureResourceTypes.*` |
| 5 | Multiple types in one file | One public type per file |
| 6 | `object`, `dynamic`, `Dictionary<string,object>` | Use typed models |
| 7 | Missing `.ProducesProblem(401)` on endpoints | Always include it |
| 8 | Response DTO IDs as `Guid` | Always use `string` |
| 9 | `FK Restrict` on cross-resource FKs | Use `SetNull` or `Cascade` |
| 10 | Writing production code without tests | TDD is mandatory |

---

## Documentation Standards

- **XML docs** on all public types and methods in C#
- **English** for all error messages and code comments
- **Markdown** for architecture documentation
- **Mermaid** for diagrams
