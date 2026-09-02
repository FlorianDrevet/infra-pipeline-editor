---
description: "Use when generating or modifying C#/.NET or Angular source code. Enforces no magic strings, one public top-level type per file, strong typing over weak dictionaries/object/JSON, and explicit design-pattern selection."
name: "Code Quality Guardrails"
applyTo: "src/Api/**/*.cs, src/Mcp/**/*.cs, src/Shared/**/*.cs, tests/**/*.cs, src/Front/src/**/*.ts"
---

# Code Quality Guardrails

- No magic strings in production code. Use enums, dedicated constant classes, typed options, or `nameof()` when appropriate.
- In `src/Api/InfraFlowSculptor.BicepGeneration/Generators/*BicepGenerator.cs`, inline semantic Bicep literals are forbidden in builder/generator code. Extract module names, ARM types, import/type names, parameter names, resource symbols, property/output names, union literals, and repeated raw expressions to constants. Keep generator-specific literals as `private const` fields local to that generator file. When a literal has the same stable meaning across multiple generators and is repeated broadly (for example `name`, `location`, `properties`, `kind`, `id`, `./types.bicep`, or the boolean sentinel strings), reuse the dedicated shared constants type under `Generators/Constants/` instead of duplicating it. Keep one-off human-readable descriptions inline.
- One public top-level type per file in production code. Do not create dump files such as `Dtos.cs`, `Models.cs`, `Requests.cs`, `Responses.cs`, or `Helpers.cs` that aggregate many unrelated types.
- Organize files into thematic subfolders (Models/, Constants/, Analysis/, etc.) when a folder exceeds ~6 files or mixes different responsibilities. Namespaces must match the physical path.
- Prefer strongly typed contracts, domain models, persistence models, and view models. Avoid `object`, `dynamic`, `Dictionary<string, object>`, `JsonDocument`, `JsonNode`, `JObject`, `any`, and `Record<string, unknown>` when the schema is known.
- Use dictionaries only for genuine lookup or dynamic-key scenarios, not as a substitute for an explicit schema.
- If a weakly typed external boundary is unavoidable, isolate it in the adapter layer, validate it, and map it immediately to typed models before it reaches the rest of the codebase.
- Before introducing a structural pattern (`Strategy`, `Factory`, `Builder`, `Specification`, `Policy`, etc.), compare the plausible options and keep the simplest choice that improves readability, maintainability, and scalability.
- Prefer explicit database columns, owned types, converters, and typed read models over schemaless JSON storage when the persistence shape is known.