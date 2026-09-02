---
title: MultiRepo Push D10 Implementation Tracker
description: Living tracker for the project-level MultiRepo artifact push implementation.
ms.date: 2026-09-01
ms.topic: tracking
---

# MultiRepo Push D10 Implementation Tracker

This tracker records the implementation of project-level artifact pushes for the `MultiRepo`
layout. A project push is an orchestration of independent repository commits; it is not an
atomic transaction across repositories.

## Decision

The existing `SplitInfraCode` project push remains available under its explicit route and
contract. A separate `MultiRepo` operation accepts configuration-owned repository targets and
routes artifacts from each `InfrastructureConfig` to its own repositories.

## Status

| Lot | Scope | Status | Validation | Remaining work |
|-----|-------|--------|------------|----------------|
| 1 | Typed MultiRepo command, targets, result, validator, and handler | Done | 17 focused application tests passed | None |
| 2 | Configuration repository loading, role routing, artifact prevalidation, and independent pushes | Done | 8 focused service tests passed; solution build passed | Real Git provider validation |
| 3 | Config-level bootstrap split for `AllInOne` and `SplitInfraCode` | Done | 6 focused bootstrap handler tests passed | Real Git provider validation |
| 4 | API routes/contracts, dependency injection, frontend service, workflow, and bulk dialog | Done | API rate-limit test passed; 43 focused Angular tests passed; typecheck/build passed | Real Git provider validation |
| 5 | Full regression and real-project validation | In progress | Solution build passed; targeted backend and frontend checks passed; full runs are environment-limited | Validate against real Git repositories |

## Implemented Contract

The MultiRepo request uses `InfrastructureConfigId` and `InfraConfigRepositoryId` values. The
server verifies that every requested repository belongs to the requested configuration and derives
the artifact role from the persisted `ConfigLayoutMode` and `ContentKinds`.

For `AllInOne`, one configuration repository receives Bicep, infrastructure and application
pipelines, and bootstrap artifacts in one repository commit. For `SplitInfraCode`, the
Infrastructure repository receives Bicep, infrastructure pipelines, and the `FullOwner` bootstrap;
the ApplicationCode repository receives application pipelines and the `ApplicationOnly` bootstrap.

All structural checks and artifact reads complete before the first Git push. Git failures are
returned per repository so a later target can still be attempted. Cancellation is propagated and
is not converted into a generic Git failure.

## Automated Validation

Commands run from the repository root unless stated otherwise:

* `dotnet build .\InfraFlowSculptor.slnx --no-restore`
* Targeted Application tests for config-level Bicep, Pipeline, Bootstrap, D10 bulk push, legacy SplitInfraCode push, and bootstrap generation: **all passed**, including Blob cancellation, provider cancellation, and no-secret fallback cases.
* Targeted Infrastructure tests for Key Vault, Blob artifact reads, GitHub, and Azure DevOps cancellation: **all passed**.
* `dotnet test .\tests\InfraFlowSculptor.Api.Tests\InfraFlowSculptor.Api.Tests.csproj --filter "FullyQualifiedName~RateLimitingTests.Given_HeavyGenerationEndpoints_When_BuildingEndpointMap_Then_AllRequireExpensivePolicy"`
* `npm run typecheck` from `src\Front`
* `npm run build` from `src\Front`
* Focused Angular specs for the bulk dialog, workflow, ProjectService, board, and the renamed SplitInfraCode dialog: **70 passed**.
* The full .NET suite reported 4,346 tests: 4,281 passed, 9 skipped, and 56 failed. The reported failures are environment-gated PostgreSQL/Docker integration tests and the four API security integration tests that cannot build without a connection string; no D10 test failed.
* The full Karma suite reached 351 of 453 specs with 15 failures before ChromeHeadless disconnected after a full-page reload. The failures are outside D10; the focused D10 specs remain green.
* `python -m graphify update .`

## Remaining Validation

The code is not marked fully verified until a real `MultiRepo` project is exercised against a Git
provider. The validation must cover at least two `AllInOne` configurations, one
`SplitInfraCode` configuration, config-level PAT retrieval, Bicep/pipeline/bootstrap generation,
one independent commit per target repository, and a partial Git failure.

The known solution test debt remains separate from D10. Compare the complete test result with the
baseline documented in `docs/stabilization/NEXT.md`; do not treat those historical failures as
new D10 regressions without comparing their names and causes.