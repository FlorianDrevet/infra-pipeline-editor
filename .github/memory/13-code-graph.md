# Code Graph — GitNexus Knowledge Cache

> Maintenu par `@dream`. Pré-cache les informations structurelles stables pour éviter aux agents de requêter GitNexus pour les infos connues.
> Source de vérité : le knowledge graph GitNexus (repo `infra-pipeline-editor`). Si un doute, re-vérifier via `gitnexus_context()` ou `gitnexus_impact()`.

---

## Index status

- **Repo indexé :** `infra-pipeline-editor`
- **Dernière indexation connue :** 2026-04-03
- **Stats :** 9 569 nœuds, 41 834 edges, 667 clusters, 300 flows

## Symboles à haut risque (beaucoup de dépendants upstream)

| Symbole | Type | Raison du risque |
|---------|------|-----------------|
| `AzureResource` | Base class (TPT) | 18 agrégats enfants héritent — tout changement cascade sur toutes les ressources |
| `IInfraConfigAccessService` | Interface | Utilisé par tous les handlers Resource pour la vérification d'accès |
| `BicepGenerationEngine` | Class (970 lignes) | Cœur de la génération Bicep — 2 handlers dépendent + tous les generators |
| `BicepAssembler` | Class (~180 lines) | Thin orchestrator — delegates to 14 specialized classes under `Assemblers/`, `Helpers/`, `StorageAccount/`, `Models/` |
| `InfrastructureConfigReadRepository` | Class | Point central de lecture — switch cases sur tous les types de ressources |
| `AppPipelineGenerationEngine` | Class | Orchestrateur app pipeline — 5 generators (Container/Code × resource type), appelé par les 2 pipeline handlers |
| `MonoRepoPipelineAssembler` | Class | Assembleur pipeline YAML infra — mono-repo structure, couplé aux handlers génération pipeline |

## Flows critiques

| Flow | Chemin simplifié |
|------|-----------------|
| Génération Bicep (config) | `BicepGenerationController` → `GenerateBicepCommandHandler` → `BicepGenerationEngine` → `BicepAssembler` (→ sub-assemblers) |
| Génération Bicep (projet) | `BicepGenerationController` → `GenerateProjectBicepCommandHandler` → `BicepGenerationEngine` → `MonoRepoBicepAssembler` |
| Génération Pipeline (infra+app) | `PipelineGenerationController` → `GeneratePipelineCommandHandler` → `MonoRepoPipelineAssembler` + `AppPipelineGenerationEngine` |
| CRUD Resource (pattern) | `{Resource}Controller` → MediatR → `{Action}{Resource}CommandHandler` → `I{Resource}Repository` → EF Core |

## Counts — Verified by Cypher [2026-04-03]

| Métrique | Valeur | Requête |
|----------|--------|---------|
| AzureResource children | 18 | `(c)-[:EXTENDS]->(AzureResource)` |
| AggregateRoot classes | 5 (Project, InfrastructureConfig, ResourceGroup, AzureResource, User) | `(c)-[:EXTENDS]->(AggregateRoot)` |
| Total Entity/AggregateRoot | 46 | Includes all EnvironmentSettings, sub-entities, base entities |
| Controllers | 27 | Files in `Controllers/` ending `.cs` |
| TypeBicepGenerators | 18 | Classes ending `BicepGenerator` |
| AzureResourceTypes.All | 18 entries | In `GenerationCore/AzureResourceTypes.cs` |
| Commands | ~110 | Files ending `Command.cs` in Application layer |
| Queries | ~51 | Files ending `Query.cs` in Application layer |

## Clusters fonctionnels principaux

> À compléter par `@dream` lors de la prochaine consolidation via `READ gitnexus://repo/infra-pipeline-editor/clusters`.

---

*Dernière mise à jour : 2026-04-04 — Dream consolidation*
