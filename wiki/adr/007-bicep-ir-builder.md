# ADR-007: IR + Builder for Bicep Generation

## Status
Accepted

## Context

The original Bicep generation used string templates with regex-based substitution. This caused:
- String manipulation errors (unclosed brackets, wrong indentation)
- Difficulty testing (string comparison fragile)
- No structural validation before emission
- Hard to compose transformations (identity injection, tags, outputs)

## Decision

Introduce an **Intermediate Representation (IR)** layer:
- `BicepModuleSpec` — typed model of a Bicep module
- `BicepModuleBuilder` — fluent API to construct specs
- `BicepEmitter` — converts IR to text
- Per-type generators implement `IResourceTypeBicepSpecGenerator`

## Consequences

### Positive
- Type safety: structural errors caught at compile time
- Testable: assert on IR properties, not strings
- Composable: pipeline stages modify the IR (add outputs, inject identity)
- Emitter separation: text formatting is isolated
- Future-proof: can emit ARM JSON or Terraform from same IR

### Negative
- More abstraction layers
- Migration effort from legacy string generators
- IR model must stay in sync with Bicep language features

### Risks
- Over-engineering for simple resources (mitigated: builder makes simple cases trivial)
- IR may not cover all Bicep edge cases (mitigated: raw expression escape hatch)
