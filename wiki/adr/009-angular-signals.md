# ADR-009: Angular Standalone + Signals

## Status
Accepted

## Context

The frontend needed a modern, performant architecture. Angular introduced standalone components (no NgModules) and signals (reactive primitives) as the recommended path forward.

Options considered:
1. **NgModules + RxJS** — Traditional Angular pattern
2. **Standalone + RxJS** — Modern components with Observables
3. **Standalone + Signals** — Modern components with signals (zoneless)
4. **React/Vue** — Alternative frameworks

## Decision

Use **Angular standalone components with signals**, running in **zoneless mode** (no Zone.js).

## Consequences

### Positive
- No NgModule boilerplate
- Signals are simpler than RxJS for component state
- Zoneless improves performance (no unnecessary change detection)
- Tree-shakable: unused code eliminated
- Future-proof: Angular team's recommended direction
- inject() function is cleaner than constructor DI

### Negative
- Signals API still evolving
- Some third-party libraries expect Zone.js
- Team must learn signals pattern
- Less community examples (newer pattern)

### Risks
- Breaking changes in signals API (mitigated: Angular team committed to stability)
