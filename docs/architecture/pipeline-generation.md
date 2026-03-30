# Génération Pipeline — Pipelines Azure DevOps

## Vue d'ensemble

Le moteur de génération de pipelines produit les fichiers YAML pour **Azure DevOps** à partir de la configuration d'infrastructure. Il vit dans un projet dédié :

```
src/Api/InfraFlowSculptor.PipelineGeneration/
```

Comme le moteur Bicep, il est **indépendant du domaine** et travaille avec des modèles de génération partagés (`GenerationCore`).

---

## Architecture

```
        GenerationRequest (modèle partagé avec Bicep)
                │
                ▼
    ┌───────────────────────────────────┐
    │   PipelineGenerationEngine        │   ← Moteur principal
    │   Pour chaque config :            │
    │     → ci.pipeline.yml             │
    │     → release.pipeline.yml        │
    │     → variables/*.yml             │
    │   Shared templates :              │
    │     → pipelines/ci.pipeline.yml   │
    │     → jobs/deploy.job.yml         │
    │     → steps/deploy-template.step  │
    └──────────────┬────────────────────┘
                   │
                   ▼
    ┌───────────────────────────────────┐
    │   MonoRepoPipelineAssembler       │   ← Assembleur mono-repo
    │   Organise en .azuredevops/       │
    │   + dossier par config            │
    └───────────────────────────────────┘
```

---

## `PipelineGenerationEngine` — Le moteur

```
Fichier : src/Api/InfraFlowSculptor.PipelineGeneration/PipelineGenerationEngine.cs
```

Le moteur expose deux méthodes :

### `Generate()` — Fichiers par configuration

Génère les fichiers spécifiques à une `InfrastructureConfig` :

```csharp
public PipelineGenerationResult Generate(GenerationRequest request, string configName)
```

**Fichiers produits :**

| Fichier | Contenu |
|---------|---------|
| `ci.pipeline.yml` | Pipeline CI (build, validate, what-if) |
| `release.pipeline.yml` | Pipeline de release (déploiement par environnement) |
| `variables/vars.yml` | Variables partagées de la config |
| `variables/{env}.yml` | Variables spécifiques par environnement (dev, staging, prod…) |

### `GenerateSharedTemplates()` — Templates communs

Génère les templates réutilisables partagés entre toutes les configs :

```csharp
public static IReadOnlyDictionary<string, string> GenerateSharedTemplates(
    IReadOnlyList<string> configNames)
```

**Fichiers produits :**

| Fichier | Contenu |
|---------|---------|
| `pipelines/ci.pipeline.yml` | Template CI partagé orchestrant toutes les configs |
| `jobs/deploy.job.yml` | Job de déploiement réutilisable |
| `steps/deploy-template.step.yml` | Steps de déploiement Bicep (validate, what-if, deploy) |

---

## `MonoRepoPipelineAssembler` — Assembleur mono-repo

```
Fichier : src/Api/InfraFlowSculptor.PipelineGeneration/MonoRepoPipelineAssembler.cs
```

En mode mono-repo, l'assembleur organise la sortie sous un dossier `.azuredevops/` :

```csharp
public static class MonoRepoPipelineAssembler
{
    public static MonoRepoPipelineResult Assemble(
        IReadOnlyDictionary<string, PipelineGenerationResult> perConfigResults)
}
```

### Structure de sortie mono-repo

```
.azuredevops/
├── pipelines/
│   └── ci.pipeline.yml           ← Template CI partagé
├── jobs/
│   └── deploy.job.yml            ← Job de déploiement réutilisable
├── steps/
│   └── deploy-template.step.yml  ← Steps Bicep (validate/what-if/deploy)
├── ConfigA/
│   ├── ci.pipeline.yml           ← CI spécifique à ConfigA
│   ├── release.pipeline.yml      ← Release spécifique à ConfigA
│   └── variables/
│       ├── vars.yml              ← Variables partagées de ConfigA
│       ├── dev.yml               ← Variables dev
│       └── prod.yml              ← Variables prod
└── ConfigB/
    ├── ci.pipeline.yml
    ├── release.pipeline.yml
    └── variables/
        └── ...
```

---

## Modèles de données

### `PipelineGenerationResult`

Résultat de la génération pour une seule configuration :

| Propriété | Type | Description |
|-----------|------|-------------|
| `TemplateFiles` | `IReadOnlyDictionary<string, string>` | Fichiers générés (path → contenu YAML) |
| `Files` | Proxy | Alias de `TemplateFiles` (implémente `IGenerationResult`) |

### `MonoRepoPipelineResult`

Résultat de l'assemblage mono-repo :

| Propriété | Type | Description |
|-----------|------|-------------|
| `CommonFiles` | `IReadOnlyDictionary<string, string>` | Templates partagés (`.azuredevops/pipelines/`, `jobs/`, `steps/`) |
| `ConfigFiles` | `IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>` | Fichiers par config (clé = nom config, valeur = fichiers) |

---

## Relation avec le moteur Bicep

Les deux moteurs partagent le même `GenerationRequest` (via `GenerationCore`) et sont invoqués par les mêmes handlers dans l'Application layer. La différence :

| Aspect | Bicep | Pipeline |
|--------|-------|----------|
| Produit | Fichiers `.bicep` et `.bicepparam` | Fichiers YAML Azure DevOps |
| Stratégies par type | `IResourceTypeBicepGenerator` (16 implémentations) | Pas de stratégie par type (structure uniforme) |
| Assembleur mono-repo | `MonoRepoBicepAssembler` | `MonoRepoPipelineAssembler` |
| Dossier de sortie | `infra/` | `.azuredevops/` |

---

## Pages connexes

- [Génération Bicep](bicep-generation.md) — Le moteur compagnon pour les fichiers Bicep
- [Architecture du projet](overview.md) — Vue d'ensemble des couches
- [Push Bicep to Git](../features/push-bicep-to-git.md) — Feature de push des fichiers générés vers un dépôt Git
