# Stack-Aware Application Pipelines - Implementation Roadmap

> Purpose: handoff document for continuing the WebApp / FunctionApp / ContainerApp application pipeline work on another machine.
> Current date: 2026-05-21.

## Goal

Make the **App Pipeline** configuration closer to real application frameworks instead of asking junior users to type raw commands.

The target UX is:

- select or auto-detect an application stack such as `.NET`, `Node.js`, `Angular`, `Java`, `Python`, `Static site`, or `Custom`;
- show framework-specific choices such as `xUnit`, `NUnit`, `Jest`, `Vitest`, `pytest`, `JUnit`, package manager, lint/test/build toggles;
- keep raw commands only as an advanced/custom escape hatch;
- generate Azure DevOps YAML from typed options, not from scattered free-text fields.

This plan supersedes the older generic option plan in [pipeline-options-plan.md](pipeline-options-plan.md) for the current stack-aware implementation wave.

## Current Status

### Done - Lot 1: Domain Foundation

Status: implemented and validated.

What it contains:

- `ApplicationStack` domain selector distinct from Azure hosting runtime stacks such as `WebAppRuntimeStack` and `FunctionAppRuntimeStack`.
- Typed `AppPipelineStackProfile` hierarchy:
  - `DotNetPipelineProfile`
  - `NodeJsPipelineProfile`
  - `AngularPipelineProfile`
  - `JavaPipelineProfile`
  - `PythonPipelineProfile`
  - `StaticSitePipelineProfile`
  - `CustomPipelineProfile`
- Strongly typed framework/package/build-tool wrappers:
  - `DotNetTestFramework`
  - `NodePackageManager`
  - `NodeTestFramework`
  - `JavaBuildTool`
  - `JavaTestFramework`
  - `PythonPackageManager`
  - `PythonTestFramework`
- `AppPipelineStepOptionsData` now carries `Stack` and optional typed `Profile`.
- `AppPipelineStepOptions.Update(...)` validates stack/profile consistency before mutating fields, preserving atomicity.
- Domain tests cover default profiles, mismatch rejection, and custom command normalization.

Reference files:

- [AppPipelineStepOptions.cs](../../src/Api/InfraFlowSculptor.Domain/Common/OwnedEntities/AppPipelineStepOptions.cs)
- [AppPipelineStepOptionsData.cs](../../src/Api/InfraFlowSculptor.Domain/Common/OwnedEntities/AppPipelineStepOptionsData.cs)
- [Stacks](../../src/Api/InfraFlowSculptor.Domain/Common/OwnedEntities/Stacks)
- [AppPipelineStepOptionsTests.cs](../../tests/InfraFlowSculptor.Domain.Tests/Common/OwnedEntities/AppPipelineStepOptionsTests.cs)
- [PipelineProfileTests.cs](../../tests/InfraFlowSculptor.Domain.Tests/Common/OwnedEntities/Stacks/PipelineProfileTests.cs)

### Done - Prerequisite: Pipeline Switch Behavior

Status: implemented and validated.

Why it was required:

The existing shared code pipeline template emitted `.NET` tests, test result publishing, and coverage publishing even when the corresponding options were disabled. This would have made future stack profiles misleading because user-visible switches would not match generated YAML.

What it contains:

- Shared code pipeline/job/step templates now declare and forward:
  - `runUnitTests`
  - `testFramework`
  - `testResultsFormat`
  - `testResultsPath`
  - `publishTestResults`
  - `publishCodeCoverage`
  - `coverageTool`
  - `coverageReportPath`
- Default `.NET` `dotnet test` runs only when `runUnitTests` is true and no custom `testCommand` is supplied.
- Coverage collection runs only when `publishCodeCoverage` is true.
- `PublishTestResults@2` runs only when `runUnitTests && publishTestResults`.
- `PublishCodeCoverageResults@2` runs only when `runUnitTests && publishCodeCoverage`.
- Node default `npm run test --if-present` is now also gated by `runUnitTests`.
- PR wrappers now forward test and coverage switches, matching the CI wrapper behavior.
- Golden files were refreshed.

Reference files:

- [AppPrPipelineBuilder.cs](../../src/Api/InfraFlowSculptor.PipelineGeneration/Generators/App/AppPrPipelineBuilder.cs)
- [AppPipelinePipelineTemplates.cs](../../src/Api/InfraFlowSculptor.PipelineGeneration/Generators/App/SharedTemplates/AppPipelinePipelineTemplates.cs)
- [AppPipelineJobTemplates.cs](../../src/Api/InfraFlowSculptor.PipelineGeneration/Generators/App/SharedTemplates/AppPipelineJobTemplates.cs)
- [AppPipelineStepTemplates.cs](../../src/Api/InfraFlowSculptor.PipelineGeneration/Generators/App/SharedTemplates/AppPipelineStepTemplates.cs)
- [AppPipelineCodeStepOptionTests.cs](../../tests/InfraFlowSculptor.PipelineGeneration.Tests/AppPipelineCodeStepOptionTests.cs)
- [AppPipeline Golden Files](../../tests/InfraFlowSculptor.PipelineGeneration.Tests/GoldenFiles/AppPipeline)

Important note:

`AppBuildStepEmitter` still contains older hardcoded behavior, but impact study found it is not the active generation path. The active path is shared templates plus thin wrappers via `extends`.

### Done - Lot 2: EF Persistence

Status: implemented and validated.

What it contains:

- `ApplicationStack` persisted on the shared owned `AppPipelineStepOptions` model for `WebApp`, `FunctionApp`, and `ContainerApp`.
- Public typed `Profile` is ignored by EF and rehydrated from explicit private snapshot fields.
- Profile persistence uses flattened typed columns, not JSON and not EF polymorphic owned types.
- Migration generated: `20260521145619_AddApplicationStackProfileToPipelineOptions`.
- Migration adds `PipelineStepOptions_Stack` with default `Unknown`, plus nullable profile snapshot columns on:
  - `WebApps`
  - `FunctionApps`
  - `ContainerApps`

Why flattened columns were chosen:

EF tried to map `AppPipelineStackProfile` as an entity and failed with a missing primary-key error. Flattened private fields keep the database explicit, typed, queryable, and aligned with the repository rule against JSON blobs when the schema is known.

Reference files:

- [AppPipelineStepOptionsConfiguration.cs](../../src/Api/InfraFlowSculptor.Infrastructure/Persistence/Configurations/AppPipelineStepOptionsConfiguration.cs)
- [20260521145619_AddApplicationStackProfileToPipelineOptions.cs](../../src/Api/InfraFlowSculptor.Infrastructure/Migrations/20260521145619_AddApplicationStackProfileToPipelineOptions.cs)
- [ProjectDbContextModelSnapshot.cs](../../src/Api/InfraFlowSculptor.Infrastructure/Migrations/ProjectDbContextModelSnapshot.cs)
- [AppPipelineStepOptionsConfigurationTests.cs](../../tests/InfraFlowSculptor.Infrastructure.Tests/Persistence/Configurations/AppPipelineStepOptionsConfigurationTests.cs)

## Validation Already Run

Validated after implementation:

- `dotnet test .\tests\InfraFlowSculptor.Domain.Tests\InfraFlowSculptor.Domain.Tests.csproj --no-restore`
  - 691 total, 0 failed.
- `dotnet test .\tests\InfraFlowSculptor.PipelineGeneration.Tests\InfraFlowSculptor.PipelineGeneration.Tests.csproj --no-restore`
  - 116 total, 0 failed.
- `dotnet test .\tests\InfraFlowSculptor.Infrastructure.Tests\InfraFlowSculptor.Infrastructure.Tests.csproj --no-restore`
  - 304 total, 0 failed, 9 skipped.
- `dotnet test .\InfraFlowSculptor.slnx --no-restore`
  - 4293 total, 0 failed, 4284 passed, 9 skipped.
- `dotnet build .\InfraFlowSculptor.slnx --no-restore`
  - success.

Known unrelated output:

- Full solution tests reported 3 pre-existing `xUnit1013` analyzer warnings in Application tests.
- Infrastructure has existing skipped tests around EF InMemory limitations for complex user-name properties.

## Remaining Lots

### Done - Lot 3 - Contracts, API, and Application Mapping

Goal: expose the stack/profile model through the existing create/update/read flows.

Status: implemented and validated.

Scope:

- Add typed contract DTOs for stack-aware pipeline options.
- Extend existing `Create*AppRequest` / `Update*AppRequest` DTOs for `WebApp`, `FunctionApp`, and `ContainerApp` so `pipelineStepOptions` can carry stack/profile data.
- Extend response DTOs so the UI can reload and display the selected stack/profile.
- Update Mapster mappings and/or request mapping logic to produce `AppPipelineStepOptionsData` with the correct typed profile.
- Add validators for stack/profile consistency at the contract boundary.
- Preserve backward compatibility: omitted stack should behave as `Unknown`, omitted profile should remain null.

Suggested contract shape:

```json
{
  "pipelineStepOptions": {
    "stack": "DotNet",
    "profile": {
      "kind": "DotNet",
      "testFramework": "XUnit",
      "collectCoverage": true,
      "customTestProjectGlob": "tests/**/*.csproj"
    },
    "runUnitTests": true,
    "publishTestResults": true,
    "publishCodeCoverage": true
  }
}
```

Implementation guidance:

- Prefer one public top-level DTO per file.
- Do not use `object`, `dynamic`, `Dictionary<string, object>`, `JsonDocument`, or raw JSON for profile payloads.
- Use a discriminated typed model if possible, or split profile DTOs and map at the boundary.
- If the serializer needs a boundary discriminator, map it immediately into `AppPipelineStackProfile` and do not propagate weak shapes into Application/Domain.

Likely files to inspect:

- `src/Api/InfraFlowSculptor.Contracts/**/Create*AppRequest.cs`
- `src/Api/InfraFlowSculptor.Contracts/**/Update*AppRequest.cs`
- `src/Api/InfraFlowSculptor.Contracts/**/Responses/*AppResponse.cs`
- `src/Api/InfraFlowSculptor.Api/**/Mapping*`
- `src/Api/InfraFlowSculptor.Application/**/Commands/**`

Expected tests:

- Contract shape tests or request mapping tests for each compute type.
- Validator tests for stack/profile mismatch.
- Roundtrip read-response tests if existing response mapping tests are available.

### Done - Lot 4 - Generation Uses Stack Profiles

Goal: generate stack-specific YAML from typed profiles instead of raw commands wherever possible.

Status: implemented and validated.

Scope:

- Extend `AppPipelineGenerationRequest` with `ApplicationStack` and typed or flattened generation profile values.
- Update `AppPipelineRequestFactory` to read `AppPipelineStepOptions.Stack/Profile` from compute aggregates and populate the generation request.
- Update `BuildCodeStep` logic to emit stack-specific install/test/lint/build behavior:
  - `.NET`: `dotnet restore`, `dotnet build`, `dotnet test`; `xUnit`, `NUnit`, and `MSTest` all use TRX publishing.
  - `Node.js`: package manager-aware install/test commands for npm/yarn/pnpm; support Jest/Vitest/Mocha defaults.
  - `Angular`: package manager-aware install, optional `ng test`, `ng lint`, production build.
  - `Java`: Maven/Gradle test/build, JUnit/TestNG result publishing, optional JaCoCo.
  - `Python`: pip/Poetry/uv install and pytest/unittest execution, optional coverage.
  - `StaticSite`: typed build command and output directory.
  - `Custom`: keep raw commands as advanced fallback.
- Keep commands deterministic and Windows-agent compatible. Generated YAML must use `powershell` steps, not Bash or `pwsh`.

Implementation guidance:

- Do not put Azure DevOps `${{ if }}` directives inside multiline script bodies.
- Prefer YAML-level conditional nodes for mutually exclusive branches.
- Keep shared templates as the canonical surface; do not revive `AppBuildStepEmitter` unless intentionally refactoring the pipeline engine.
- Update golden files after tests prove the intended generated output.

Likely files:

- [AppPipelineGenerationRequest.cs](../../src/Api/InfraFlowSculptor.PipelineGeneration/Generators/App/AppPipelineGenerationRequest.cs)
- [AppPipelineRequestFactory.cs](../../src/Api/InfraFlowSculptor.Application/Common/Generation/AppPipelineRequestFactory.cs)
- [AppPipelineStepTemplates.cs](../../src/Api/InfraFlowSculptor.PipelineGeneration/Generators/App/SharedTemplates/AppPipelineStepTemplates.cs)
- [AppPipelineCodeStepOptionTests.cs](../../tests/InfraFlowSculptor.PipelineGeneration.Tests/AppPipelineCodeStepOptionTests.cs)
- [AppPipeline Golden Files](../../tests/InfraFlowSculptor.PipelineGeneration.Tests/GoldenFiles/AppPipeline)

Expected tests:

- Unit tests for request factory stack/profile propagation.
- Pipeline generation tests per stack for the minimum happy path.
- Golden snapshots for shared templates and representative WebApp/FunctionApp/ContainerApp wrappers.
- Existing Windows shell compatibility tests must stay green.

### Done - Lot 5 - Frontend Stack-Aware Pipeline UI

Goal: replace the current generic pipeline option form with a stack-aware UI that feels like application-framework configuration.

Status: implemented and validated.

Current frontend baseline:

- The current model is generic and string-heavy in [pipeline-step-options.model.ts](../../src/Front/src/app/features/resource-edit/models/pipeline-step-options.model.ts).
- The current component is [pipeline-options.component.ts](../../src/Front/src/app/features/resource-edit/components/pipeline-options/pipeline-options.component.ts).

Scope:

- Add frontend types for `ApplicationStack` and profile DTOs matching Lot 3 contracts.
- Replace raw `testCommand`-first UX with stack selector + profile controls.
- Use existing DS components (`app-ds-*`) and `ToggleSectionCardComponent`.
- Stack-specific controls:
  - `.NET`: test framework select (`xUnit`, `NUnit`, `MSTest`), coverage toggle, optional test project glob advanced field.
  - `Node.js`: package manager select, test framework select, lint toggle, test/lint script names.
  - `Angular`: package manager, `ng test`, `ng lint`, production build toggle, optional project name.
  - `Java`: Maven/Gradle, JUnit 5/JUnit 4/TestNG, coverage toggle.
  - `Python`: pip/Poetry/uv, pytest/unittest, coverage toggle.
  - `Static site`: build command and output directory.
  - `Custom`: raw test/lint/build commands.
- Keep existing generic toggles for Sonar, dependency scan, build validation, dependency cache, and smoke tests unless they move into a later dedicated UX pass.
- Add i18n keys in both `fr.json` and `en.json`.

Implementation guidance:

- Do not create a marketing-style page; this is an operational resource editor panel.
- Keep controls dense, predictable, and junior-friendly.
- Avoid exposing impossible combinations. For example, do not show `.NET` test framework choices when stack is `Angular`.
- Raw commands should live in an advanced/custom section, not be the default path.

Likely files:

- [pipeline-step-options.model.ts](../../src/Front/src/app/features/resource-edit/models/pipeline-step-options.model.ts)
- [pipeline-options.component.ts](../../src/Front/src/app/features/resource-edit/components/pipeline-options/pipeline-options.component.ts)
- [pipeline-options.component.html](../../src/Front/src/app/features/resource-edit/components/pipeline-options/pipeline-options.component.html)
- [pipeline-options.component.scss](../../src/Front/src/app/features/resource-edit/components/pipeline-options/pipeline-options.component.scss)
- `src/Front/src/public/i18n/fr.json`
- `src/Front/src/public/i18n/en.json`

Expected validation:

- Component tests for stack switching and emitted payload shape.
- `npm run typecheck`.
- `npm run build`.

### Done - Lot 6 - Automatic Stack and Framework Detection

Goal: suggest stack/profile settings from repository content.

Status: implemented and validated.

Scope:

- Add backend detection service that reads repository files through the existing Git provider abstractions.
- Add CQRS query endpoint such as `GET /azure-resources/{resourceId}/pipeline-stack-detection`.
- Detect stack and profile from source files.
- Return suggestions, confidence, signals, and warnings rather than silently mutating the resource.
- Let the frontend show suggestions and let the user apply them manually.

Detection matrix:

- `.NET`:
  - `*.csproj`, `global.json`, `Directory.Packages.props`.
  - `PackageReference Include="xunit"`, `NUnit`, `MSTest`.
  - `coverlet.collector` for coverage.
- `Node.js`:
  - `package.json`.
  - package manager lockfiles: `package-lock.json`, `yarn.lock`, `pnpm-lock.yaml`.
  - `jest`, `vitest`, `mocha`, `eslint`.
- `Angular`:
  - `angular.json`, `@angular/core`, `ng` scripts.
- `Java`:
  - `pom.xml`, `build.gradle`, `build.gradle.kts`.
  - `junit-jupiter`, `junit`, `testng`, `jacoco`.
- `Python`:
  - `pyproject.toml`, `requirements.txt`, `pytest.ini`, `setup.cfg`.
  - `pytest`, `pytest-cov`, `coverage`, `ruff`, `black`.
- `Static site`:
  - simple HTML/static output or known static build scripts.

Implementation guidance:

- Treat detection as advisory, never authoritative.
- Do not require repository access to save manual options.
- If repository access fails, return a typed warning and keep the manual UX usable.
- Keep parsing bounded and deterministic; avoid fragile regex-heavy scans over large files.

Expected tests:

- Detector unit tests with small fake repository file sets.
- Query handler authorization/access tests.
- Frontend tests for suggestion apply/dismiss behavior.

### Done - Lot 7 - Sonar, Linting, Dependency Scan, Cache, and Smoke Tests

Goal: finish the non-P0 pipeline options after the stack-aware foundation is stable.

Status: implemented and validated. All pipeline step options (Sonar, linting, dependency scan, dependency cache, smoke tests) were already wired through shared templates in Phases 4-6. Stack-specific lint/test defaults are now resolved by `StackProfileCommandResolver` in the Application layer.

Scope:

- Wire `RunSonarAnalysis`, Sonar project key, organization, host URL, and service connection into shared templates.
- Wire `RunLinting` and stack-specific lint defaults.
- Wire `RunDependencyScan` for supported stacks.
- Wire `EnableDependencyCache` for NuGet/npm/yarn/pnpm/pip/Poetry/Gradle/Maven as appropriate.
- Wire `RunSmokeTests` after deployment, with a future decision on whether smoke tests remain global per resource or become per environment.

Implementation guidance:

- Prefer one option family per PR if the template changes become large.
- Add explicit Azure DevOps service-connection prerequisites to the bootstrap/docs only when the generated YAML truly needs them.
- Keep secret values out of YAML; use variable groups/service connections.

Expected tests:

- Template tests for every new parameter family.
- Golden file refresh.
- Bootstrap/preflight tests if service connections are introduced.

### Lot 8 - MCP and Import Workflow Integration

Goal: expose stack-aware pipeline configuration to MCP/project drafting flows once API and UI are stable.

Status: future.

Scope:

- Extend MCP project draft models so prompts can request stack/profile choices.
- Allow import/preview tools to suggest stack-aware pipeline options from IaC or repository signals.
- Keep clarification prompts mandatory when stack/profile cannot be inferred with enough confidence.

Implementation guidance:

- Use the `mcp-dotnet-server` skill before implementation.
- Do not let MCP create invalid Azure DevOps pipeline assumptions; surface clarification questions instead.

## Recommended Next Sequence

1. Start with Lot 3 so stack/profile selections can cross the API boundary.
2. Then do Lot 4 so generation consumes the stored choices.
3. Then do Lot 5 so users can edit the model properly.
4. Then add Lot 6 detection as an advisory layer.
5. Finish with Lot 7 option families and Lot 8 MCP integration.

Do not start the frontend UI before Lot 3 contracts are stable, otherwise the frontend will have to guess the payload shape.

## TDD Checklist for Next PC

For every lot that modifies code:

- Load project instructions and relevant skills first.
- Write focused RED tests before production code.
- Keep changes scoped to the lot.
- Run focused tests first.
- Run impacted project tests.
- Run the full solution build/test before closing the lot when backend contracts or shared generation behavior changed.

Recommended commands:

```powershell
dotnet test .\tests\InfraFlowSculptor.Domain.Tests\InfraFlowSculptor.Domain.Tests.csproj --no-restore
dotnet test .\tests\InfraFlowSculptor.Contracts.Tests\InfraFlowSculptor.Contracts.Tests.csproj --no-restore
dotnet test .\tests\InfraFlowSculptor.Application.Tests\InfraFlowSculptor.Application.Tests.csproj --no-restore
dotnet test .\tests\InfraFlowSculptor.Infrastructure.Tests\InfraFlowSculptor.Infrastructure.Tests.csproj --no-restore
dotnet test .\tests\InfraFlowSculptor.PipelineGeneration.Tests\InfraFlowSculptor.PipelineGeneration.Tests.csproj --no-restore
dotnet test .\InfraFlowSculptor.slnx --no-restore
dotnet build .\InfraFlowSculptor.slnx --no-restore
```

Frontend lots:

```powershell
Set-Location .\src\Front
npm run typecheck
npm run build
```

## Handoff Notes

- Keep EF profile persistence flattened unless there is a deliberate schema redesign.
- Keep `ApplicationStack.Unknown` as the backward-compatible default.
- Keep raw custom commands available, but only as advanced/custom behavior.
- Keep app pipeline generated YAML Windows-agent compatible.
- Keep shared templates as the active generation surface.
- Update [pipeline-options-plan.md](pipeline-options-plan.md) only if the generic long-term option catalog changes; use this file for the stack-aware implementation status.