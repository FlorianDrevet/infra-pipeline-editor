# Code Graph — GitNexus Knowledge Cache

> Maintenu par `@dream`. Pré-cache les informations structurelles stables pour éviter aux agents de requêter GitNexus pour les infos connues.
> Source de vérité : le knowledge graph GitNexus (repo `infra-pipeline-editor`). Si un doute, re-vérifier via `gitnexus_context()` ou `gitnexus_impact()`.

---

## Index status

- **Repo indexé :** `infra-pipeline-editor`
- **Session context [2026-04-25] :** GitNexus expose le repo avec 14 901 nodes, 55 895 relations, 810 communities, et 300 flows (index frais au 2026-04-24T10:48:20Z).
- **Spot-check [2026-04-25] :** les requêtes GitNexus résolvent désormais `CustomDomain` et `BootstrapPipelineGenerationEngine`; l'ancienne alerte sur les symboles récents non résolus est obsolète.
- **Règle pratique [2026-04-25] :** pour les noms partagés entre entités métier et classes d'erreur, fournir `file_path` à `gitnexus_context()` pour obtenir le bon symbole du premier coup.
- **Obsolete cached stats removed:** the earlier 2026-04-03 counts (`9 569` nodes / `41 834` edges / `667` clusters) no longer represent the current index surface.

## Symboles à haut risque (beaucoup de dépendants upstream)

| Symbole | Type | Raison du risque |
|---------|------|-----------------|
| `AzureResource` | Base class (TPT) | 18 agrégats enfants héritent — tout changement cascade sur toutes les ressources |
| `IInfraConfigAccessService` | Interface | Utilisé par tous les handlers Resource pour la vérification d'accès |
| `BicepGenerationEngine` | Class (970 lignes) | Cœur de la génération Bicep — 2 handlers dépendent + tous les generators |
| `BicepAssembler` | Class (~180 lines) | Thin orchestrator — delegates to 14 specialized classes under `Assemblers/`, `Helpers/`, `StorageAccount/`, `Models/` |
| `InfrastructureConfigReadRepository` | Class | Point central de lecture — switch cases sur tous les types de ressources |
| `AppPipelineGenerationEngine` | Class | Orchestrateur app pipeline — 5 generators (Container/Code × resource type), appelé par les handlers génération pipeline; spot-check GitNexus [2026-04-25]: risque upstream **MEDIUM**, 6 dépendants directs |
| `MonoRepoPipelineAssembler` | Class | Assembleur pipeline YAML infra — mono-repo structure, couplé aux handlers génération pipeline |

## Flows critiques

| Flow | Chemin simplifié |
|------|-----------------|
| Génération Bicep (config) | `BicepGenerationController` → `GenerateBicepCommandHandler` → `BicepGenerationEngine` → `BicepAssembler` (→ sub-assemblers) |
| Génération Bicep (projet) | `BicepGenerationController` → `GenerateProjectBicepCommandHandler` → `BicepGenerationEngine` → `MonoRepoBicepAssembler` |
| Génération Pipeline (infra+app) | `PipelineGenerationController` → `GeneratePipelineCommandHandler` → `MonoRepoPipelineAssembler` + `AppPipelineGenerationEngine` |
| Génération Bootstrap ADO (projet) | `ProjectController` → `GenerateProjectBootstrapPipelineCommandHandler` → `BootstrapPipelineGenerationEngine` |
| CRUD Resource (pattern) | `{Resource}Controller` → MediatR → `{Action}{Resource}CommandHandler` → `I{Resource}Repository` → EF Core |

## Counts — Verified by Cypher [2026-04-03]

| Métrique | Valeur | Requête |
|----------|--------|---------|
| AzureResource children | 18 | `(c)-[:EXTENDS]->(AzureResource)` |
| AggregateRoot classes | 5 (Project, InfrastructureConfig, ResourceGroup, AzureResource, User) | `(c)-[:EXTENDS]->(AggregateRoot)` |
| Total Entity/AggregateRoot | 46 | Includes all EnvironmentSettings, sub-entities, base entities |
| Controllers | 29 | Files in `Controllers/` ending `.cs` (verified in repo on 2026-04-23) |
| TypeBicepGenerators | 18 | Classes ending `BicepGenerator` |
| AzureResourceTypes.All | 18 entries | In `GenerationCore/AzureResourceTypes.cs` |
| Commands | ~110 | Files ending `Command.cs` in Application layer |
| Queries | ~51 | Files ending `Query.cs` in Application layer |

## Clusters fonctionnels principaux

- **Génération Bicep** — `BicepGenerationController` / handlers projet+config -> `BicepGenerationEngine` -> `BicepAssembler` / `MonoRepoBicepAssembler`
- **Génération Pipeline** — `PipelineGenerationController` / `ProjectController` -> handlers projet+config -> `MonoRepoPipelineAssembler`, `AppPipelineGenerationEngine`, `BootstrapPipelineGenerationEngine`
- **Topology & Git routing** — `Project`, `InfrastructureConfig`, `IRepositoryTargetResolver`, repositories projet/config, handlers de push mono-repo et multi-repo
- **CRUD ressources** — contrôleurs par ressource -> handlers CQRS -> repositories EF Core / read repositories

---

*Dernière mise à jour : 2026-04-25 — Dream consolidation (dream cycle)*
