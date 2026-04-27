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
| Analyse du générateur legacy | ✅ | 80 LOC, 2 imports (SkuName, OsType), 5 params (2 custom types), 2 variables (isLinux, kind), nested sku/properties, 1 output, 2 exported types |
| Tests TDD écrits | ✅ | `tests/.../Generators/AppServicePlanTypeBicepGeneratorTests.cs` — 27 tests |
| Migration vers Builder | ✅ | Implements `IResourceTypeBicepSpecGenerator`, first generator with `Var()` (conditional + raw expression) |
| Parité d'émission vérifiée | ✅ | Module + types emission verified; variables emit correctly |
| Branché dans le pipeline | ✅ | No DI change; fixed `ModuleBuildStage` to preserve `Parameters` dict from legacy `Generate()` |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ✅ | |

### 1.4 — ContainerRegistry (84 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | 84 LOC, 2 imports (SkuName, PublicNetworkAccess), 6 params (2 custom types, 2 bool with defaults), inline ternary in resource body, 2 outputs with descriptions, 2 exported types |
| Tests TDD écrits | ✅ | `tests/.../Generators/ContainerRegistryTypeBicepGeneratorTests.cs` — 27 tests |
| Migration vers Builder | ✅ | Implements `IResourceTypeBicepSpecGenerator`, inline `BicepConditionalExpression` in resource body property |
| Parité d'émission vérifiée | ✅ | Module + types emission verified |
| Branché dans le pipeline | ✅ | No DI change needed — auto-detected by `ModuleBuildStage` |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ✅ | |

---

## Phase 2 — Tier 2 : Moyens avec types (89-97 lignes)

### 2.1 — ServiceBusNamespace (89 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | 89 LOC, 2 imports (SkuName, TlsVersion), 7 params (2 custom types, 2 bool, 1 int with defaults), nested sku with inline ternary for capacity, `listKeys()` output, 2 exported types |
| Tests TDD écrits | ✅ | `tests/.../Generators/ServiceBusNamespaceTypeBicepGeneratorTests.cs` — 28 tests |
| Migration vers Builder | ✅ | Implements `IResourceTypeBicepSpecGenerator`, `BicepRawExpression` for `listKeys()` output and `sku == 'Premium'` condition |
| Parité d'émission vérifiée | ✅ | Module + types emission verified |
| Branché dans le pipeline | ✅ | No DI change needed — auto-detected by `ModuleBuildStage` |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ✅ | |

### 2.2 — AppConfiguration (91 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | 91 LOC, 2 imports (SkuName, PublicNetworkAccess), 7 params (2 custom, 2 bool, 1 int with defaults), simple sku, 4-prop properties, 2 outputs (id + endpoint), 2 exported types, Parameters["sku"] |
| Tests TDD écrits | ✅ | `tests/.../Generators/AppConfigurationTypeBicepGeneratorTests.cs` — 27 tests |
| Migration vers Builder | ✅ | Implements `IResourceTypeBicepSpecGenerator`, straightforward — simplest Tier 2 generator |
| Parité d'émission vérifiée | ✅ | Module + types emission verified |
| Branché dans le pipeline | ✅ | No DI change needed — auto-detected by `ModuleBuildStage` |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ✅ | |

### 2.3 — ApplicationInsights (91 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | 91 LOC, 1 import (IngestionMode), 8 params (1 custom, 2 bool, 2 int, 1 cross-resource string), `kind: 'web'` top-level literal, 7-prop properties with PascalCase keys, 3 outputs (id + 2 runtime `.properties.*`), 1 exported type |
| Tests TDD écrits | ✅ | `tests/.../Generators/ApplicationInsightsTypeBicepGeneratorTests.cs` — 29 tests |
| Migration vers Builder | ✅ | `IResourceTypeBicepSpecGenerator`, `BicepStringLiteral("web")` for top-level `kind` + `Application_Type` |
| Parité d'émission vérifiée | ✅ | Module + types emission verified |
| Branché dans le pipeline | ✅ | No DI change needed — auto-detected by `ModuleBuildStage` |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ✅ | |

### 2.4 — EventHubNamespace (97 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | 97 LOC, 2 imports (SkuName, TlsVersion), 9 params (2 custom, 3 bool, 2 int), nested sku (name/tier reuse sku param, capacity), 5-prop properties with conditional `maximumThroughputUnits`, 2 outputs (id + nameOutput), 2 exported types, empty Parameters dict |
| Tests TDD écrits | ✅ | `tests/.../Generators/EventHubNamespaceTypeBicepGeneratorTests.cs` — 31 tests |
| Migration vers Builder | ✅ | `IResourceTypeBicepSpecGenerator`, `BicepConditionalExpression` for auto-inflate ternary, sku name+tier reusing same param |
| Parité d'émission vérifiée | ✅ | Module + types emission verified |
| Branché dans le pipeline | ✅ | No DI change needed — auto-detected by `ModuleBuildStage` |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ✅ | |

---

## Phase 3 — Tier 3 : Moyens avec logique (95-121 lignes)

### 3.1 — KeyVault (95 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | 95 LOC, `BuildModuleTemplate()` with 6 dynamic booleans from `resource.Properties`, `subscription().tenantId` raw expression, nested sku inside properties (family='A'), 3 params, 3 outputs (id + name + vaultUri), 1 exported type, Parameters["sku"] |
| Tests TDD écrits | ✅ | `tests/.../Generators/KeyVaultTypeBicepGeneratorTests.cs` — 29 tests (incl. dynamic property defaults + overrides) |
| Migration vers Builder | ✅ | `IResourceTypeBicepSpecGenerator`, `bool.Parse(resource.Properties.GetValueOrDefault(...))` → `BicepBoolLiteral`, first generator with dynamic resource.Properties injection |
| Parité d'émission vérifiée | ✅ | Module + types emission verified, default + override scenarios |
| Branché dans le pipeline | ✅ | No DI change needed — auto-detected by `ModuleBuildStage` |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ✅ | |

### 3.2 — SqlServer (97 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | 97 LOC, static const template, 2 imports (SqlServerVersion, TlsVersion), 6 params (2 custom types, 1 `@secure()` string), `publicNetworkAccess: 'Enabled'` literal, 2 outputs (id + FQDN), 2 exported types, `NormalizeSqlServerVersion()` helper, Parameters dict + SecureParameters |
| Tests TDD écrits | ✅ | `tests/.../Generators/SqlServerTypeBicepGeneratorTests.cs` — 30 tests (incl. secure param, V12 normalization) |
| Migration vers Builder | ✅ | `IResourceTypeBicepSpecGenerator`, first generator with `secure: true` param via Builder API |
| Parité d'émission vérifiée | ✅ | Module + types emission verified, `@secure()` decorator emitted |
| Branché dans le pipeline | ✅ | No DI change needed — auto-detected by `ModuleBuildStage` |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ✅ | |

### 3.3 — SqlDatabase (91 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | Parent ref `sqlServerName` (existing resource pattern), size conversion GB→bytes. First generator needing IR extension for existing resources + parent references |
| Tests TDD écrits | ✅ | `tests/.../Generators/SqlDatabaseTypeBicepGeneratorTests.cs` — 33 tests |
| Migration vers Builder | ✅ | `ExistingResource()` + `Parent()` builder methods. 1 import, 7 params (SkuName custom type), existing resource + parent ref, nested sku + properties, 1 output, 1 exported type |
| Parité d'émission vérifiée | ✅ | `existing = {` block + `parent:` property emitted correctly |
| Branché dans le pipeline | ✅ | Auto-detected via `IResourceTypeBicepSpecGenerator` — no DI changes |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

### 3.4 — CosmosDb (121 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | 121 LOC, static const template. 11 params (3 custom types, 3 bool defaults, 2 int defaults, 1 array default `[]`). Deeply nested properties (consistencyPolicy 3-prop, backupPolicy 1-prop, locations array with nested object). 3 outputs, 3 exported types |
| Tests TDD écrits | ✅ | `tests/.../Generators/CosmosDbTypeBicepGeneratorTests.cs` — 40 tests |
| Migration vers Builder | ✅ | First generator with `BicepType.Array` param + `BicepArrayExpression([])` default. `BicepArrayExpression` with nested `BicepObjectExpression` for locations array. `BicepObjectBuilder` string/int/bool shorthand overloads used throughout |
| Parité d'émission vérifiée | ✅ | Array emission multiline with nested objects |
| Branché dans le pipeline | ✅ | Auto-detected via `IResourceTypeBicepSpecGenerator` — no DI changes |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

### 3.5 — RedisCache (117 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | 117 LOC, static const template. 10 params (3 custom types: SkuName, SkuFamily, TlsVersion; 2 bool defaults). Dynamic Parameters dict (8 values from resource.Properties). Conditional ternary for `'aad-enabled'`. 4 outputs (2 string, 2 int — first generator with `BicepType.Int` outputs). 3 exported types |
| Tests TDD écrits | ✅ | `tests/.../Generators/RedisCacheTypeBicepGeneratorTests.cs` — 43 tests |
| Migration vers Builder | ✅ | Quoted property key `'aad-enabled'` (hyphen in key) passed as-is to builder. `BicepConditionalExpression` for bool→string conversion. `BicepType.Int` outputs for sslPort/port |
| Parité d'émission vérifiée | ✅ | Conditional emits `aadEnabled ? 'true' : 'false'` correctly |
| Branché dans le pipeline | ✅ | Auto-detected via `IResourceTypeBicepSpecGenerator` — no DI changes |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | |

### 3.6 — ContainerAppEnvironment (104 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | types.bicep (WorkloadProfileType), diagnostic settings child resource, `logAnalyticsWorkspaceId` conditional |
| Tests TDD écrits | ✅ | 44 tests (interface, params, primary resource, additional resource diagnosticSettings, outputs, exported types, legacy compat, emission) |
| Migration vers Builder | ✅ | IR extended: `Condition`, `Scope` on `BicepResourceDeclaration`, `AdditionalResources` on `BicepModuleSpec`, `AdditionalResource()` on builder |
| Parité d'émission vérifiée | ✅ | Inline conditional `appLogsConfiguration` cosmetically different (single-line object) but semantically identical |
| Branché dans le pipeline | ✅ | Auto-detected via `IResourceTypeBicepSpecGenerator` in `ModuleBuildStage` |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ✅ | Migration #14 added to SKILL.md |

---

## Phase 4 — Tier 4 : Complexes avec variantes ACR (~350 lignes, 3 templates)

### 4.1 — WebApp (354 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | 3 variantes ACR (Code / Container+MI / Container+Admin), `@secure()` acrPassword, parent ref `appServicePlanId`, custom domains |
| Tests TDD écrits | ✅ | 71 tests (9 Code params, 14 MI params, 13 Admin params, 3 variants × resource/vars/outputs/types + emission + legacy compat) |
| Migration vers Builder | ✅ | Conditional branches in single `GenerateSpec()` based on `deploymentMode`/`acrAuthMode` |
| Parité d'émission vérifiée | ✅ | ForLoop hostNameBindings, ModuleFileName per variant, secure acrPassword |
| Branché dans le pipeline | ✅ | `IResourceTypeBicepSpecGenerator` detected by `ModuleBuildStage` at runtime |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ✅ | Migration #15 added |

### 4.2 — FunctionApp (384 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | Same 3-variant pattern as WebApp + `FUNCTIONS_WORKER_RUNTIME`/`EXTENSION_VERSION` in all variants, `workerRuntime` var in all variants, 3 exported types |
| Tests TDD écrits | ✅ | 74 tests (8 Code params, 13 MI params, 12 Admin params, all 3 variants × resource/vars/outputs/types + emission + legacy compat) |
| Migration vers Builder | ✅ | Same conditional branch pattern as WebApp, appSettings array in all variants (2 for Code/MI, 5 for Admin) |
| Parité d'émission vérifiée | ✅ | ForLoop hostNameBindings, ModuleFileName per variant, secure acrPassword, kind always present |
| Branché dans le pipeline | ✅ | `IResourceTypeBicepSpecGenerator` detected by `ModuleBuildStage` at runtime |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ✅ | Migration #16 added |

---

## Phase 5 — Tier 5 : Très complexes (400-500 lignes, variantes + companions + custom types)

### 5.1 — ContainerApp (503 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | 3 variants (NoAcr/MI/Admin), 6 exported types (4 object types), 14 ParameterGroupMappings preserved in legacy `Generate()` only, `@secure()`, health probes via raw `union()`, custom domains via raw for-loop var |
| Tests TDD écrits | ✅ | 54 tests covering all 3 variants (params, vars, resource body, configuration sub-object, outputs, exported types) + legacy compat + emission |
| Migration vers Builder | ✅ | Object exported types use multi-line `BicepRawExpression`. Probes block extracted as `BuildProbesUnion()` static helper. Custom domain bindings as raw for-loop in var. |
| Parité d'émission vérifiée | ✅ | Conditional ingress + nested customDomains conditional emit inline (acceptable per skill) |
| Branché dans le pipeline | ✅ | `IResourceTypeBicepSpecGenerator` detected at runtime |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ✅ | Migration #17 added |

### 5.2 — StorageAccount (417 LOC)

| Étape | Statut | Notes |
|-------|--------|-------|
| Analyse du générateur legacy | ✅ | 3 companion modules (Blobs, Queues, Tables), JSON deserialization, CorsRuleDescription, ContainerLifecycleRule |
| Tests TDD écrits | ✅ | 42 tests in `StorageAccountTypeBicepGeneratorTests.cs` |
| Migration vers Builder | ✅ | Primary module migrated to IR (8 params, 4 custom types, identity baked in, 6 outputs). Companions remain in legacy `Generate()` |
| Parité d'émission vérifiée | ✅ | 842 total tests green |
| Branché dans le pipeline | ✅ | `ModuleBuildStage` updated to preserve `CompanionModules` + `ParameterGroupMappings` from legacy `Generate()` |
| Review de code | ⬜ | |
| Corrections appliquées | ⬜ | |
| Skill mis à jour avec retours | ⬜ | Migration #18 added |

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
