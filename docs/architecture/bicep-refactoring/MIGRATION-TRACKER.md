# Vague 2 — Builder + IR Migration Tracker

> **Objectif :** Migrer les 18 générateurs Bicep du pattern legacy (const string template + regex)
> vers le pattern Builder + IR (représentation intermédiaire typée).
>
> **Skill de référence :** `.github/skills/bicep-v2-migration/SKILL.md`
>
> **Workflow par générateur :** Analyse → Tests TDD → Migration → Parité → Pipeline → Review → Fix → Maj skill

---

## Légende

| Icône | Statut |
|-------|--------|
| ⬜ | Non commencé |
| 🔵 | En cours |
| ✅ | Terminé |
| ⏸️ | Bloqué |

---

## Phase 0 — Infrastructure IR

> Fondation à livrer AVANT toute migration de générateur.

| # | Tâche | Statut | Notes |
|---|-------|--------|-------|
| 0.1 | Modèle IR (`BicepGeneration/Ir/` — records immuables) | ✅ | `BicepModuleSpec`, `BicepParam`, `BicepOutput`, `BicepResourceDeclaration`, `BicepExpression` hierarchy, `BicepType`, `BicepVar`, `BicepImport`, `BicepTypeDefinition`, `BicepCompanionSpec` — 9 fichiers |
| 0.2 | Builder fluent (`Ir/Builder/`) | ✅ | `BicepModuleBuilder`, `BicepObjectBuilder` |
| 0.3 | Emitter (`Ir/Emit/BicepEmitter`) | ✅ | Transforme `BicepModuleSpec` → string. Imports, params, vars, resource, outputs, types |
| 0.4 | Transformations IR (`Ir/Transformations/`) | ✅ | `IdentityTransformer`, `OutputTransformer`, `AppSettingsTransformer`, `TagsTransformer` |
| 0.5 | Adaptateur legacy (`LegacyTextModuleAdapter`) | ✅ | `CreateSkeletonModule` + `EmitContent` pour coexistence IR/legacy |
| 0.6 | Adaptation des stages 300-700+850 (dual mode) | ✅ | ModuleBuild (300), Identity (400), Output (500), AppSettings (600), Tags (700) + SpecEmission (850) |
| 0.7 | Interface générateur (`IResourceTypeBicepSpecGenerator`) | ✅ | Extends `IResourceTypeBicepGenerator`, ajoute `GenerateSpec()`. `ModuleWorkItem.Spec` ajouté |
| 0.8 | Tests infrastructure IR | ✅ | 37 tests : Emitter (21), Builder (10), Identity (7), Output (5), Tags (5), AppSettings (4) — tous verts |
| 0.9 | Review de code Phase 0 | ⬜ | `review-expert` → `review-remediator` |

---

## Phase 1 — Tier 1 : Générateurs simples (47-84 lignes, 1 template, pas de variantes)

> Objectif : stabiliser l'IR et l'emitter sur les cas les plus simples.

### 1.1 — UserAssignedIdentity (47 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | 47 LOC, 1 template, no types/companions/variants/secure params |
| Tests TDD écrits | ✅ | `tests/.../Generators/UserAssignedIdentityTypeBicepGeneratorTests.cs` — 18 tests |
| Migration vers Builder | ✅ | Implements `IResourceTypeBicepSpecGenerator`, `GenerateSpec()` via `BicepModuleBuilder` |
| Parité d'émission vérifiée | ✅ | Emitter output contains all expected sections; cosmetic: blank lines between outputs |
| Branché dans le pipeline | ✅ | No DI change needed — `ModuleBuildStage` detects `is IResourceTypeBicepSpecGenerator` |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

### 1.2 — LogAnalyticsWorkspace (77 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | 77 LOC, 1 import (SkuName), types.bicep, nested properties, 5 params (2 with defaults), 2 outputs |
| Tests TDD écrits | ✅ | `tests/.../Generators/LogAnalyticsWorkspaceTypeBicepGeneratorTests.cs` — 22 tests |
| Migration vers Builder | ✅ | Implements `IResourceTypeBicepSpecGenerator`, nested `Property()` lambdas for properties/sku/workspaceCapping |
| Parité d'émission vérifiée | ✅ | Module + types emission verified |
| Branché dans le pipeline | ✅ | No DI change needed — auto-detected by `ModuleBuildStage` |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ✅ | |

### 1.3 — AppServicePlan (80 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | types.bicep (SkuName, OsType), sku/capacity/osType params |
| Tests TDD écrits | ⬜ | |
| Migration vers Builder | ⬜ | |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

### 1.4 — ContainerRegistry (84 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | types.bicep (SkuName, PublicNetworkAccess) |
| Tests TDD écrits | ⬜ | |
| Migration vers Builder | ⬜ | |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

---

## Phase 2 — Tier 2 : Moyens avec types (89-97 lignes)

### 2.1 — ServiceBusNamespace (89 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | types.bicep (SkuName, TlsVersion), `listKeys()` output |
| Tests TDD écrits | ⬜ | |
| Migration vers Builder | ⬜ | |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

### 2.2 — AppConfiguration (91 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | types.bicep (SkuName, PublicNetworkAccess), sku extraction |
| Tests TDD écrits | ⬜ | |
| Migration vers Builder | ⬜ | |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

### 2.3 — ApplicationInsights (91 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | types.bicep (IngestionMode), dépendance `logAnalyticsWorkspaceId` param |
| Tests TDD écrits | ⬜ | |
| Migration vers Builder | ⬜ | |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

### 2.4 — EventHubNamespace (97 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | types.bicep (SkuName, TlsVersion), auto-inflate. ⚠ PAS enregistré en DI actuellement |
| Tests TDD écrits | ⬜ | |
| Migration vers Builder | ⬜ | |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Enregistrement DI corrigé | ⬜ | Ajouter le singleton manquant |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

---

## Phase 3 — Tier 3 : Moyens avec logique (95-121 lignes)

### 3.1 — KeyVault (95 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | Template dynamique `BuildModuleTemplate()`, 6 boolean properties, types.bicep (SkuName) |
| Tests TDD écrits | ⬜ | |
| Migration vers Builder | ⬜ | |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

### 3.2 — SqlServer (97 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | `@secure()` param, version normalization (V12→12.0), types.bicep |
| Tests TDD écrits | ⬜ | |
| Migration vers Builder | ⬜ | |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

### 3.3 — SqlDatabase (91 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | Parent ref `sqlServerName` (existing resource pattern), size conversion GB→bytes |
| Tests TDD écrits | ⬜ | |
| Migration vers Builder | ⬜ | |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

### 3.4 — CosmosDb (121 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | Multi-region locations array, capabilities, types.bicep (DatabaseKind, ConsistencyLevel, BackupPolicyType) |
| Tests TDD écrits | ⬜ | |
| Migration vers Builder | ⬜ | |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

### 3.5 — RedisCache (117 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | 8 params, AAD config, types.bicep (SkuName, SkuFamily, TlsVersion) |
| Tests TDD écrits | ⬜ | |
| Migration vers Builder | ⬜ | |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

### 3.6 — ContainerAppEnvironment (104 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | types.bicep (WorkloadProfileType), diagnostic settings child resource, `logAnalyticsWorkspaceId` conditional |
| Tests TDD écrits | ⬜ | |
| Migration vers Builder | ⬜ | |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

---

## Phase 4 — Tier 4 : Complexes avec variantes ACR (~350 lignes, 3 templates)

### 4.1 — WebApp (354 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | 3 variantes ACR (Code / Container+MI / Container+Admin), `@secure()` acrPassword, parent ref `appServicePlanId`, custom domains |
| Tests TDD écrits | ⬜ | 1 test par variante + edge cases |
| Migration vers Builder | ⬜ | Branches conditionnelles dans un seul Builder |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

### 4.2 — FunctionApp (384 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | Même pattern 3 variantes que WebApp + `FUNCTIONS_WORKER_RUNTIME`/`EXTENSION_VERSION` |
| Tests TDD écrits | ⬜ | |
| Migration vers Builder | ⬜ | |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

---

## Phase 5 — Tier 5 : Très complexes (400-500 lignes, variantes + companions + custom types)

### 5.1 — ContainerApp (503 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | 3 variantes ACR, 4 custom Bicep types, 12 ParameterGroupMappings, `@secure()`, health probes, custom domains |
| Tests TDD écrits | ⬜ | |
| Migration vers Builder | ⬜ | |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

### 5.2 — StorageAccount (417 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ⬜ | 3 companion modules (Blobs, Queues, Tables), JSON deserialization, CorsRuleDescription, ContainerLifecycleRule |
| Tests TDD écrits | ⬜ | |
| Migration vers Builder | ⬜ | |
| Parité d'émission vérifiée | ⬜ | |
| Branché dans le pipeline | ⬜ | |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

---

## Phase 6 — Finalisation

| # | Tâche | Statut | Notes |
|---|-------|--------|-------|
| 6.1 | Supprimer `LegacyTextModuleAdapter` (plus de générateurs legacy) | ⬜ | |
| 6.2 | Supprimer `TextManipulation/` (regex devenues inutiles) | ⬜ | |
| 6.3 | Nettoyer les tests legacy des TextManipulation | ⬜ | |
| 6.4 | Retirer le dual-mode des stages (IR only) | ⬜ | |
| 6.5 | Review finale de l'architecture IR | ⬜ | |
| 6.6 | Mise à jour mémoire projet | ⬜ | |

---

## Statistiques de progression

| Phase | Total étapes | Terminées | Progression |
|-------|-------------|-----------|-------------|
| Phase 0 — Infra IR | 9 | 0 | 0% |
| Phase 1 — Tier 1 (×4) | 32 | 0 | 0% |
| Phase 2 — Tier 2 (×4) | 32 | 0 | 0% |
| Phase 3 — Tier 3 (×6) | 48 | 0 | 0% |
| Phase 4 — Tier 4 (×2) | 16 | 0 | 0% |
| Phase 5 — Tier 5 (×2) | 16 | 0 | 0% |
| Phase 6 — Finalisation | 6 | 0 | 0% |
| **Total** | **159** | **0** | **0%** |

---

## Règles d'utilisation

1. **Un générateur à la fois** — Ne pas paralléliser les migrations pour éviter les conflits sur l'IR/emitter
2. **Tests AVANT migration** — TDD obligatoire (skill `xunit-unit-testing`)
3. **Review APRÈS migration** — `review-expert` + `review-remediator` obligatoires
4. **Skill mis à jour** — Chaque retour de review alimente `.github/skills/bicep-v2-migration/SKILL.md`
5. **Build complet entre chaque migration** — Pas de dette de compilation
6. **Tracker mis à jour** — Ce fichier est mis à jour après chaque étape terminée
