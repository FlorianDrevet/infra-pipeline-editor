# Code Graph — GitNexus Knowledge Cache

> Maintenu par `@dream`. Pré-cache les informations structurelles stables pour éviter aux agents de requêter GitNexus pour les infos connues.
> Source de vérité : le knowledge graph GitNexus (repo `infra-pipeline-editor`). Si un doute, re-vérifier via `gitnexus_context()` ou `gitnexus_impact()`.

---

## Dual-graph architecture [2026-04-29]

Ce dépôt utilise **deux graphes complémentaires** :

| Graphe | Outil | Périmètre | Force | Skill |
|--------|-------|-----------|-------|-------|
| Code graph | **GitNexus** | Symboles, appels, héritages, flows | Impact analysis, blast radius, rename, detect_changes | `gitnexus-workflow` |
| Corpus graph | **Graphify** | Code AST + docs + audits + diagrammes + images | God nodes, communautés, connexions surprenantes, traçabilité doc↔code | `graphify-corpus` |

**Règle absolue :** GitNexus pour le code, Graphify pour le corpus. Ne jamais les intervertir.

---

## Index status

- **Repo indexé :** `infra-pipeline-editor`
- **Session context [2026-04-30] :** 15 162 symbols, 69 021 edges, 958 clusters, 300 execution flows (verified via `npx gitnexus analyze`).
- **Règle pratique :** pour les noms partagés entre entités métier et classes d'erreur, fournir `file_path` à `gitnexus_context()` pour obtenir le bon symbole du premier coup.

## Symboles à haut risque (beaucoup de dépendants upstream)

| Symbole | Type | Raison du risque |
|---------|------|-----------------|
| `AzureResource` | Base class (TPT) | 18 agrégats enfants héritent — tout changement cascade sur toutes les ressources |
| `IInfraConfigAccessService` | Interface | Utilisé par tous les handlers Resource pour la vérification d'accès |
| `BicepGenerationEngine` | Class (~88 lignes) | Façade mince mais point d'entrée central de la génération Bicep ; orchestre la pipeline de 9 stages pour les handlers config et projet |
| `BicepAssembler` | Class (~180 lines) | Thin orchestrator — delegates to 14 specialized classes under `Assemblers/`, `Helpers/`, `StorageAccount/`, `Models/` |
| `InfrastructureConfigReadRepository` | Class | Point central de lecture — switch cases sur tous les types de ressources |
| `AppPipelineGenerationEngine` | Class | Orchestrateur app pipeline — 5 generators (Container/Code × resource type), appelé par les handlers génération pipeline; spot-check GitNexus [2026-04-25]: risque upstream **MEDIUM**, 6 dépendants directs |
| `MonoRepoPipelineAssembler` | Class | Assembleur pipeline YAML infra — mono-repo structure, couplé aux handlers génération pipeline |
| `ResourceCommandFactory` | Class | Pivot partagé entre `ApplyImportPreview`, `ProjectSetupOrchestrator`, `ProjectCreationTools`, `IacImportTools` et leurs suites de tests ; GitNexus impact [2026-04-30] : 11 dépendants directs, risque **MEDIUM** |
| `ProjectCreationTools` | Class | Surface MCP mutante `create_project_from_draft` ; GitNexus impact [2026-04-30] : 8 dépendants directs, risque **MEDIUM** |

## Flows critiques

| Flow | Chemin simplifié |
|------|-----------------|
| Génération Bicep (config) | `BicepGenerationController` → `GenerateBicepCommandHandler` → `BicepGenerationEngine` → `BicepAssembler` (→ sub-assemblers) |
| Génération Bicep (projet) | `BicepGenerationController` → `GenerateProjectBicepCommandHandler` → `BicepGenerationEngine` → `MonoRepoBicepAssembler` |
| Génération Pipeline (infra+app) | `PipelineGenerationController` → `GeneratePipelineCommandHandler` → `MonoRepoPipelineAssembler` + `AppPipelineGenerationEngine` |
| Génération Bootstrap ADO (projet) | `ProjectController` → `GenerateProjectBootstrapPipelineCommandHandler` → `BootstrapPipelineGenerationEngine` (split-aware: `FullOwner` for infra, `ApplicationOnly` for code) |
| Création projet (wizard) | `ProjectController` → `CreateProjectWithSetupCommandHandler` → atomic Project + Layout + Envs + Repos |
| Création projet MCP | `ProjectCreationTools.CreateProjectFromDraft` → `ProjectSetupOrchestrator` → `ResourceCommandFactory` / `ResourceCreationCoordinator` → handlers de création de ressources |
| Import ARM (preview/apply) | `ImportController` ou `IacImportTools` → `PreviewIacImportQuery` / `ApplyImportPreviewCommand` → `IImportPreviewAnalyzer` / `ResourceCommandFactory` |
| CRUD Resource (pattern) | `{Resource}Controller` → MediatR → `{Action}{Resource}CommandHandler` → `I{Resource}Repository` → EF Core |

## Counts — Verified snapshots [2026-04-30]

| Métrique | Valeur | Requête |
|----------|--------|---------|
| AzureResource children | 18 | `(c)-[:EXTENDS]->(AzureResource)` |
| AggregateRoot classes | 5 (Project, InfrastructureConfig, ResourceGroup, AzureResource, User) | `(c)-[:EXTENDS]->(AggregateRoot)` |
| Total Entity/AggregateRoot | 46 | Includes all EnvironmentSettings, sub-entities, base entities |
| Controllers | 31 | `Get-ChildItem .\src -Recurse -Filter *Controller.cs` (verified in repo on 2026-04-30) |
| TypeBicepGenerators | 18 | Classes ending `BicepGenerator` |
| AzureResourceTypes.All | 18 entries | In `GenerationCore/AzureResourceTypes.cs` |
| Commands | ~110 | Files ending `Command.cs` in Application layer |
| Queries | ~51 | Files ending `Query.cs` in Application layer |
| Bicep generation tests | 842+ | `tests/InfraFlowSculptor.BicepGeneration.Tests/` |
| Pipeline generation tests | 91 | `tests/InfraFlowSculptor.PipelineGeneration.Tests/` (44 golden + 47 stage) |
| MCP tests | 104 | `tests/InfraFlowSculptor.Mcp.Tests/` |
| Checked-in test projects | 7 | `tests/**/*.csproj` (verified in repo on 2026-04-30) |
| Total solution tests | ~940 | `dotnet test .\InfraFlowSculptor.slnx` |

## Clusters fonctionnels principaux

- **Génération Bicep** — `BicepGenerationController` / handlers projet+config -> `BicepGenerationEngine` -> `BicepAssembler` / `MonoRepoBicepAssembler`
- **Génération Pipeline** — `PipelineGenerationController` / `ProjectController` -> handlers projet+config -> `MonoRepoPipelineAssembler`, `AppPipelineGenerationEngine`, `BootstrapPipelineGenerationEngine`
- **Topology & Git routing** — `Project`, `InfrastructureConfig`, `IRepositoryTargetResolver`, repositories projet/config, handlers de push mono-repo et multi-repo
- **MCP project creation & imports** — `ProjectDraftTools`, `ProjectCreationTools`, `ProjectSetupOrchestrator`, `IacImportTools`, `ResourceCommandFactory`, `ResourceCreationCoordinator`, `IImportPreviewAnalyzer`
- **CRUD ressources** — contrôleurs par ressource -> handlers CQRS -> repositories EF Core / read repositories

---

*Dernière mise à jour : 2026-04-30 — Dream consolidation*
