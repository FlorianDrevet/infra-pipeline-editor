# Test Debt Tracker

> **Ce fichier recense la dette de tests unitaires détectée par les agents.**
> Chaque agent qui modifie du code et constate l'absence de tests sur la zone touchée
> DOIT ajouter une entrée ici. Les agents peuvent ensuite résorber la dette par priorité.

---

## Convention

- **P1 — Critique** : Code métier non testé, risque de régression élevé (domain, handlers, validators)
- **P2 — Important** : Code infrastructure/services non testé, impact modéré (repositories, mappers, transformers)
- **P3 — Souhaitable** : Code utilitaire ou configuration non testé, impact faible (extensions, options, helpers)

---

## Debt Register

| # | Assembly | Zone / Classe | Description | Priorité | Détecté le | Résolu le | Résolu par |
|---|----------|---------------|-------------|----------|------------|-----------|------------|
| 1 | `InfraFlowSculptor.Domain` | All aggregates | No unit tests for domain aggregates, value objects, domain invariants | P1 | 2026-04-27 | | |
| 2 | `InfraFlowSculptor.Application` | All handlers/validators | No unit tests for MediatR handlers, FluentValidation validators, pipeline behaviors | P1 | 2026-04-27 | | |
| 3 | `InfraFlowSculptor.Infrastructure` | Repositories, services | No unit tests for repository implementations, auth services, Refit clients | P2 | 2026-04-27 | | |
| 4 | `InfraFlowSculptor.Api` | Controllers, DI, error mapping | No unit tests for endpoint registration, Mapster configs, error conversion | P2 | 2026-04-27 | | |
| 5 | `InfraFlowSculptor.PipelineGeneration` | Pipeline generators | No dedicated test project (only parity tests) | P2 | 2026-04-27 | 2026-04-27 | dev |
| 6 | `InfraFlowSculptor.GenerationCore` | Shared generation abstractions | No dedicated test project | P3 | 2026-04-27 | | |
| 7 | `InfraFlowSculptor.PipelineGeneration` | Bootstrap and app pipeline generators | Dedicated test project now exists, but most generator paths are still uncovered beyond the bootstrap pipeline warning regression | P2 | 2026-04-27 | 2026-04-27 | dev |
| 8 | `InfraFlowSculptor.PipelineGeneration` | 4 engines (PipelineGenerationEngine, BootstrapPipelineGenerationEngine, AppPipelineGenerationEngine, MonoRepoPipelineAssembler) | Byte-for-byte parity sentinels via golden file tests covering all 4 engines + R2 frozen-set lock on AppPipeline shared templates (20 paths) — 22 tests + 83 captured goldens. Behavioral coverage of internal stages still missing (will be added during Vagues 1.1/1.2/1.3 stage decomposition via TDD). | P2 | 2026-04-27 | 2026-04-27 | dev |
| 9 | `InfraFlowSculptor.Contracts` | Request and validation DTOs | Dedicated test project now exists, but coverage is currently limited to the Storage Account CORS validation slice; most request/response validation paths remain untested. | P2 | 2026-04-28 | | |
| 10 | `Front (Angular)` | Local Sonar quick-win helpers | No focused frontend specs currently cover the local CORS validators, generated artifact archive path sanitization, alias normalization helper, or Bicep highlight pipe. Validation is currently limited to `npm run typecheck` and `npm run build`. | P2 | 2026-04-28 | | |

---

## How to use this file

### Adding debt (any agent)
When modifying code and discovering the target zone has no tests:
1. Add a row with the next `#`, assembly, zone, description, priority, and today's date.
2. Leave `Résolu le` and `Résolu par` empty.

### Resolving debt (any agent, typically `dotnet-dev` + `xunit-unit-testing` skill)
1. Write the missing tests following the `xunit-unit-testing` skill.
2. Verify tests pass: `dotnet test .\tests\<Assembly>.Tests\<Assembly>.Tests.csproj`.
3. Fill `Résolu le` with today's date and `Résolu par` with the agent name.
4. Do NOT delete the row — resolved rows serve as audit trail.

### Querying debt
- Filter by `Priorité` column to find highest-priority gaps.
- Filter by empty `Résolu le` to find open debt.
