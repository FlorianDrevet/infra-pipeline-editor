# ADR-008: Staged Pipeline for Generation

## Status
Accepted

## Context

Bicep generation involves multiple transformations: analyzing identity requirements, resolving cross-references, injecting outputs, adding tags, building modules, and assembling final files. A monolithic function would be unmaintainable and untestable.

## Decision

Implement a **10-stage ordered pipeline**:

| Stage | Code | Responsibility |
|-------|------|---------------|
| 100 | IdentityAnalysis | Determine managed identity needs |
| 200 | AppSettingsAnalysis | Resolve cross-config app setting references |
| 300 | ModuleBuild | Call per-type generators, produce IR |
| 400 | IdentityInjection | Add identity params/properties to specs |
| 500 | OutputInjection | Add output declarations |
| 600 | AppSettingsInjection | Resolve and inject app settings |
| 700 | TagsInjection | Add tags to all resources |
| 800 | ParentReferenceResolution | Resolve parent/child relationships |
| 850 | SpecEmission | Convert IR to text via BicepEmitter |
| 900 | Assembly | Produce final file dictionary |

## Consequences

### Positive
- Each stage is independently testable
- Stages can be added/removed without affecting others
- Explicit execution order (numbered 100-900)
- Cancellation token checked between stages
- Context object carries state between stages
- Single responsibility per stage

### Negative
- More files to navigate
- Implicit state passing via context (harder to trace data flow)
- Stage ordering bugs (stage 400 depends on stage 300)

### Risks
- Context object might grow too large (mitigated: typed properties, not dictionary)
