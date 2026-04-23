# Authentication & Authorization

- **Provider:** Azure AD (Entra ID) JWT Bearer
- **Config section:** `"AzureAd"` in appsettings
- **Fallback policy:** Authenticated users only (`RequireAuthenticatedUser`)
- **Admin policy:** `"IsAdmin"` policy
- **Current user:** `ICurrentUser` → `CurrentUser` service

## Build & Run Commands

```powershell
# Build full solution
dotnet build .\InfraFlowSculptor.slnx

# Run full stack (Aspire)
dotnet run --project .\src\Aspire\InfraFlowSculptor.AppHost\InfraFlowSculptor.AppHost.csproj

# Frontend (from src/Front)
npm install; npm run start; npm run build; npm run typecheck
```

## Windows PowerShell Notes

- Audit PowerShell scripts under `scripts/` must read their `.sh` source files with `Get-Content -Encoding UTF8` to avoid mojibake such as `SÃ©vÃ©ritÃ©` when launched from `cmd` via `powershell.exe` 5.1.
- Keep console progress messages ASCII-only when possible; `cmd` cannot have its parent code page changed reliably from inside the script.
- Azure DevOps pipeline YAML generated for the mono-repo Bicep/pipeline flow must emit `powershell` steps, not `pwsh`. The default self-hosted Windows agents used by snapshots such as Infra Flow Sculptor may not have `pwsh.exe` on PATH, which breaks CI/PR/release validation steps before any artifact logic runs.
- Bootstrap Azure DevOps generation must emit `powershell` steps, not `script`/Bash syntax. Self-hosted Windows agents execute `script` via `cmd.exe`, which breaks constructs like `|| true`, backslash continuations, `$(...)`, and `if [ ... ]`. Bootstrap runtime auth relies on `$(System.AccessToken)` exposed to scripts, not on a PAT inside the generated YAML. `az devops configure` itself must not receive `--detect false`.
- Bootstrap Azure DevOps must decode URL-encoded Azure DevOps URL segments (`%20`, etc.) before injecting organization/project/repository names into CLI defaults. Otherwise `az devops configure` can fail with interpolation/parsing errors on project names containing spaces. The generated YAML now also declares explicit stage/job names for clearer Azure DevOps run UI.
- Bootstrap Azure DevOps pipeline definitions should keep generated pipeline names ASCII-safe (` - ` instead of Unicode dashes) and check existence with `az pipelines list` rather than `az pipelines show`. On Windows self-hosted agents, `show` turns the normal "not found yet" case into a fatal PowerShell-native error instead of an idempotent empty result.
- Bootstrap Azure DevOps pipeline creation via `$(System.AccessToken)` also depends on Azure DevOps folder security: the project Build Service identity needs the `Create build pipeline` permission on the target path (for example `\Core`). The generated bootstrap YAML now fails fast if `az pipelines create` returns a non-zero exit code, instead of logging a false `Created pipeline` success message.
- Release pipelines now resolve their CI artifact source with the same display-name format as bootstrap-created definitions (`<ConfigName> - CI`). The bootstrap generator also derives variable-group entries from full project variable usages (app settings, secure parameter mappings, app configuration keys) and ensures missing variables are added even when the Azure DevOps library already exists.
- When bootstrap must create a variable group without any plain variables, it seeds the group with `PLACEHOLDER=bootstrap`, adds real plain/secret variables afterward, and only deletes `PLACEHOLDER` if another variable now exists. Deleting the placeholder too early causes Azure DevOps to reject the library with `Variable group must have at least one variable defined.`
- App pipeline file generators must return filenames relative to the application folder only (`ci.app-pipeline.yml`, `release.app-pipeline.yml`). `AppPipelineGenerationEngine.GenerateAll()` is the single owner of the `apps/{appName}/...` prefix; otherwise isolated generation produces duplicated paths such as `apps/ifs-api/ifs-api/...`.
- Generated app delivery pipelines now follow a strict CI/release split: `ci.app-pipeline.yml` builds once and publishes immutable metadata, while `release.app-pipeline.yml` consumes the CI definition through `resources.pipelines`, deploys sequential `deployment` jobs per environment, and never rebuilds artifacts.
- Container delivery now uses immutable tags derived from build number plus short SHA, publishes an `app-metadata` artifact, and promotes images across per-environment ACRs with `az acr import` by default (`AppPipelinePromotionStrategy.AcrImport`). The generator no longer emits `latest` tags.
- When `EnableSecurityScans` is enabled on app pipeline requests, generated container CI pipelines install Trivy for vulnerability scanning and Syft for CycloneDX SBOM generation, then publish the reports as a `supply-chain` artifact.
- Code-based WebApp and FunctionApp generation now mirrors the same enterprise flow: CI publishes a single `application-package` artifact and release reuses that exact package across all environments through Azure DevOps environment approvals.

## Infrastructure Services

- **GitHub Git Provider**: uses Refit (`IGitHubTreeApi`) instead of raw `HttpClient` for GitHub API calls [2026-04-16]. Registered in `Infrastructure/DependencyInjection.cs`.
- **Azure DevOps integration boundary [2026-04-22]**: `AzureDevOpsGitProviderService` still covers Git repository operations only (test connection, list branches, push files), while project bootstrap now provisions Azure DevOps pipeline definitions and variable groups through generated YAML executed inside Azure DevOps with Azure CLI calls. The bootstrap no longer emits the `Authorize Variable Groups on All Pipelines` step. Pipeline and Library security ACL assignment remains a separate concern: it can be automated only through Azure DevOps Security ACL APIs with an identity that already has admin rights. The current bootstrap runs under `$(System.AccessToken)` and therefore cannot reliably self-grant the missing rights it needs on first run. Treat pipeline `Manage security` and library `Security` assignments for the Build Service identity as one-time manual prerequisites.
- **Azure DevOps Git push branch creation [2026-04-23]**: when `AzureDevOpsGitProviderService` pushes to a branch that does not yet exist, `refUpdates.oldObjectId` must use the base branch commit SHA for edit/delete scenarios instead of the all-zero sentinel. Otherwise Azure DevOps treats the push as an initial commit on an empty tree and rejects `edit` changes with `InvalidArgumentValueException` (`The path '...' does not exist at commit ...`).
- **App pipeline path normalization [2026-04-23]**: isolated app pipeline generation must not emit duplicated logical folders like `apps/{appName}/{resourceName}/...` when `ApplicationName` is null or matches `ResourceName`. `AppPipelineGenerationEngine` now collapses that redundant first segment, and the pipeline push handlers normalize legacy duplicated blob paths (for example `apps/Ifs-frontend/ifs-frontend/ci.app-pipeline.yml` -> `apps/Ifs-frontend/ci.app-pipeline.yml`) before pushing to Git so previously generated artifacts remain compatible.
- **Manual Azure DevOps bootstrap security procedure [2026-04-22]**:
	1. Run the bootstrap pipeline once so the Build Service identity exists in Azure DevOps if the project uses project-scoped job authorization.
	2. Determine the identity to grant: `{Project Name} Build Service ({Org})` for project-scoped tokens, or `Project Collection Build Service ({Org})` for collection-scoped tokens.
	3. In `Pipelines` -> `Pipelines` -> `...` -> `Manage security`, add that Build Service identity and allow at minimum `Create build pipeline` on the target folder path used by bootstrap (for example `\Core`, or the root if you want inheritance for all generated folders).
	4. In `Pipelines` -> `Library` -> `Security`, add the same Build Service identity.
	5. Minimum role for creating new variable groups is `Creator`; recommended role for the current bootstrap flow is `Administrator`, especially if reruns must update existing groups or manage their permissions.
	6. If self-hosted pools or service connections are used, grant the Build Service identity the required `Use`/`User` permissions on those resources as separate setup steps.
- **DNS Name Availability**: `DnsNameAvailabilityChecker` [2026-04-22] — resolves `{name}.{azureSuffix}` via DNS, no Azure auth required. Replaced previous ARM-based `AzureNameAvailabilityChecker`.
- **Diagnostic Rules**: `IDiagnosticRule.EvaluateAsync()` [2026-04-22] — async interface with `Task.WhenAll` parallelization. Rules: `AcrPullDiagnosticRule`, `KeyVaultAccessDiagnosticRule`, `NameAvailabilityDiagnosticRule`.

## Sonar Quality Rules
- **S1192** — Duplicate strings in migrations (accepted)
- **new_duplicated_lines_density** — Quality gate threshold **3%**
