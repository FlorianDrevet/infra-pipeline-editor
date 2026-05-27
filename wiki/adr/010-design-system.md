# ADR-010: Design System Components

## Status
Accepted

## Context

Feature components were using Angular Material directly, leading to:
- Inconsistent styling across features
- Duplicated configuration (button variants, card styles)
- Difficult to change global UI patterns
- No single source of truth for UI primitives

## Decision

Create a **Design System layer** (`app-ds-*` components) in `shared/components/ds/`:
- Wraps Material components with project-specific defaults
- All feature code uses DS components, never raw Material
- DS components are generic, reusable, and documented

## Consequences

### Positive
- Single point of change for UI patterns
- Consistent visual language across all features
- New features get correct styling by default
- Easier to swap underlying library (Material → custom)
- Design tokens live in one place

### Negative
- Additional abstraction layer
- Migration effort (completed in W1-W8 waves)
- Must maintain DS alongside Material updates

### Risks
- DS becoming too opinionated (mitigated: keep generic, use inputs for variants)
- DS lagging behind Material features (mitigated: thin wrapper, pass-through most props)
