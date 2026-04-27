# Skill: bicep-v2-migration — Migrer un générateur Bicep vers Builder + IR (Vague 2)

> **Quand charger :** dès qu'une tâche consiste à migrer un `IResourceTypeBicepGenerator` existant
> du pattern legacy (const string template + regex injection) vers le pattern Builder + IR.

---

## Vue d'ensemble

Chaque migration transforme un générateur qui renvoie `GeneratedTypeModule` avec `ModuleBicepContent` (string brut)
en un générateur qui construit un `BicepModuleSpec` (IR typé). L'`BicepEmitter` produit le string final.

Le pipeline Vague 1 (9 stages) reste en place. Les stages `IdentityInjection`, `OutputInjection`,
`AppSettingsInjection`, `TagsInjection` opèrent désormais sur l'IR au lieu de faire du regex sur du texte.
Un adaptateur `LegacyTextModule` permet la coexistence avec les générateurs non encore migrés.

---

## Prérequis — Infrastructure IR (Phase 0)

Avant de migrer le premier générateur, l'infrastructure IR doit être en place :

1. **Modèle IR** (`BicepGeneration/Ir/`)
   - `BicepModuleSpec` — record immuable : imports, params, variables, resource, outputs, exported types, companions
   - `BicepParam` — (Name, Type, Description?, IsSecure, DefaultValue?)
   - `BicepOutput` — (Name, Type, Expression, IsSecure, Description?)
   - `BicepResourceDeclaration` — symbol, ARM type@version, properties, identity block, children
   - `BicepIdentityBlock` — IdentityKind enum + UAI refs + IsParameterized flag
   - `BicepExpression` hiérarchie polymorphe : `BicepLiteral`, `BicepReference`, `BicepInterpolation`, `BicepFunctionCall`, `BicepObjectExpression`, `BicepArrayExpression`, `BicepRawExpression`
   - `BicepImport` — (path, symbols?)
   - `BicepVar` — (Name, Expression)
   - `BicepTypeDefinition` — (Name, Body, IsExported)
   - `BicepCompanionSpec` — (ModuleName, FolderName, Spec)
   - `BicepType` — string | int | bool | object | array | custom(name)

2. **Builder** (`BicepGeneration/Ir/Builder/`)
   - `BicepModuleBuilder` — API fluent `.Module()`, `.Param()`, `.Resource()`, `.Property()`, `.Output()`, `.Companion()`, `.Build()`
   - `BicepResourceBuilder` — pour construire la déclaration resource isolément
   - `BicepObjectBuilder` — pour construire des `BicepObjectExpression` imbriquées

3. **Emitter** (`BicepGeneration/Ir/Emit/`)
   - `BicepEmitter` — transforme `BicepModuleSpec` → string Bicep (module content + types content)
   - Seul point qui décide du formatage (indentation, ordre des sections, espacement)
   - **CRITIQUE** : l'emitter doit produire un output **identique byte-pour-byte** au legacy pour les tests de parité

4. **Adaptateur legacy** (`BicepGeneration/Ir/`)
   - `LegacyTextModuleAdapter` — convertit un `GeneratedTypeModule` (string) en `BicepModuleSpec` opaque
   - Permet au pipeline de tourner en mode mixte (certains générateurs IR, d'autres legacy)

5. **Transformations IR** (`BicepGeneration/Ir/Transformations/`)
   - `IdentityTransformer` — `AddSystemAssigned`, `AddUserAssigned`, `AddParameterizedIdentity`
   - `OutputTransformer` — `AddOutputs`
   - `AppSettingsTransformer` — `AddAppSettingsParam`, `AddEnvVarsParam`
   - `TagsTransformer` — `AddTagsParam`
   - Chaque transformer = méthode d'extension pure sur `BicepModuleSpec` → `BicepModuleSpec`

6. **Stages adaptés** — Les stages 400-700 détectent si le WorkItem contient un IR ou un legacy text
   et appliquent le transformer IR ou le regex TextManipulation selon le cas

7. **Tests infra IR** — Tests unitaires pour :
   - `BicepEmitter` (chaque section : imports, params, resource, outputs, types)
   - `BicepModuleBuilder` (build valide, build sans required = erreur)
   - Chaque transformer (in → out sur `BicepModuleSpec`)

---

## Étapes pour migrer UN générateur

### Étape 1 — Analyser le générateur legacy

1. Lire le fichier du générateur (`*TypeBicepGenerator.cs`)
2. Identifier :
   - Le(s) template(s) `const string` (combien de variantes ?)
   - Les params déclarés dans le template
   - Les outputs déclarés
   - Les types exportés (fichier `types.bicep`)
   - Les `SecureParameters`
   - Les `ParameterTypeOverrides` et `ParameterGroupMappings`
   - Les `CompanionModules`
   - Les références parent (`ParentModuleIdReferences`, `ParentModuleNameReferences`, `ExistingResourceIdReferences`)
   - La logique conditionnelle (variantes ACR, propriétés optionnelles)
3. Noter le Bicep attendu en sortie (le golden file s'il existe, sinon un generate + capture)

### Étape 2 — Écrire les tests AVANT la migration (TDD)

**OBLIGATOIRE** — Écrire les tests du nouveau générateur AVANT de le migrer.

Créer `tests/InfraFlowSculptor.BicepGeneration.Tests/Generators/<ResourceTypeName>TypeBicepGeneratorTests.cs`

Conventions (from skill `xunit-unit-testing`) :
- Nommage : `Given_When_Then`
- `_sut` pour le SUT
- AAA (Arrange / Act / Assert)
- FluentAssertions, NSubstitute si besoin
- Pas de XML docs sur les tests

Tests à écrire pour chaque générateur :
1. **Spec structure** : le `BicepModuleSpec` produit a les bons `ModuleName`, `ModuleFolderName`, `ResourceTypeName`
2. **Params** : tous les params attendus sont présents avec le bon type, descriptions, valeurs par défaut
3. **Resource** : le symbol, l'ARM type@version, les properties de base
4. **Outputs** : les outputs attendus
5. **Types** : les types exportés
6. **Variantes** (si applicable) : une `ResourceDefinition` avec ACR MI → variante MI, avec ACR admin → variante admin, sans ACR → variante code
7. **Companions** (si applicable) : les companion modules sont correctement générés
8. **Secure params** : les params sécurisés sont marqués `IsSecure`
9. **Parent refs** : les params de référence parent sont corrects
10. **Émission** : `BicepEmitter.Emit(spec)` produit un Bicep valide et conforme au golden

### Étape 3 — Réécrire le générateur avec le Builder

1. Remplacer les `const string` templates par des appels au `BicepModuleBuilder`
2. Le return type change de `GeneratedTypeModule` à `BicepModuleSpec`
   - **Attention** : l'interface `IResourceTypeBicepGenerator` doit supporter les deux pendant la migration
   - Option : `IResourceTypeBicepGenerator.GenerateSpec(ResourceDefinition) → BicepModuleSpec?`
     Si non-null → IR path. Si null → fallback legacy `Generate()`.
   - Ou bien: nouvelle interface `IResourceTypeBicepSpecGenerator` implémentée en plus
3. Pour les variantes (ACR) : utiliser le Builder conditionnellement, pas des templates dupliqués
4. Pour les companions : construire des `BicepCompanionSpec` via le même Builder
5. Ne PAS modifier le `GeneratedTypeModule` ni les stages — c'est l'adaptateur qui fait le pont

### Étape 4 — Vérifier la parité d'émission

1. Le test de parité compare `BicepEmitter.Emit(generateur.GenerateSpec(resource))` avec le Bicep legacy
2. **Différences cosmétiques acceptables** (à documenter dans le test) :
   - Ordre des params (alphabétique vs déclaration)
   - Espacement entre sections
3. **Différences NON acceptables** :
   - Noms de params/outputs
   - Contenu des propriétés resource
   - Types Bicep
   - Logique conditionnelle

### Étape 5 — Brancher dans le pipeline

1. Enregistrer le générateur migré en DI (il remplace le legacy)
2. Le `ModuleBuildStage` détecte si le générateur produit un IR → stocke le `BicepModuleSpec` dans le `ModuleWorkItem`
3. Les stages suivants (400-700) utilisent le transformer IR si spec disponible, sinon le regex legacy
4. L'`AssemblyStage` appelle `BicepEmitter.Emit()` pour transformer le spec final en string avant de passer à l'assembler

### Étape 6 — Lancer la revue de code

1. Déléguer à `review-expert` pour relire l'implémentation
2. Recueillir les findings
3. Déléguer à `review-remediator` pour corriger les findings acceptés
4. **Mettre à jour CE SKILL** avec les leçons apprises (section "Retours d'expérience" en bas)

### Étape 7 — Build + tests complets

1. `dotnet build .\InfraFlowSculptor.slnx` — 0 erreurs
2. `dotnet test .\tests\InfraFlowSculptor.BicepGeneration.Tests\` — tous les tests green
3. `dotnet test .\InfraFlowSculptor.slnx` — pas de régression sur les autres projets de test

---

## Checklist de migration par générateur

```
[ ] Étape 1 — Analyse du générateur legacy complète
[ ] Étape 2 — Tests écrits AVANT migration (TDD)
[ ] Étape 3 — Générateur réécrit avec Builder
[ ] Étape 4 — Parité d'émission vérifiée
[ ] Étape 5 — Branché dans le pipeline
[ ] Étape 6 — Revue de code + corrections
[ ] Étape 7 — Build + tests complets green
[ ] Skill mis à jour avec les retours d'expérience
```

---

## Pièges connus

### Formatting de l'emitter
- L'emitter doit produire **exactement** le même formatage que les templates legacy pour les premiers générateurs migrés
- Les golden files de parité (s'ils existent) sont la référence absolue
- Une fois TOUS les générateurs migrés, l'emitter pourra être ajusté et les goldens regénérés

### Regex `\nparam\s+` dans TagsInjector
- En mode legacy, le regex TagsInjector nécessite un `\n` avant le premier `param`
- En mode IR, ce problème disparaît (les tags sont ajoutés comme `BicepParam` dans le spec)

### `GeneratedTypeModule` ne disparaît pas immédiatement
- Le `GeneratedTypeModule` reste le DTO de transit vers l'Assembler (`main.bicep`, `.bicepparam`)
- L'IR (`BicepModuleSpec`) produit le **module content** et le **types content** via l'emitter
- Les metadata (`ResourceGroupName`, `LogicalResourceName`, `ResourceAbbreviation`, etc.) sont copiées depuis la `ResourceDefinition` par le `ModuleBuildStage`

### Variantes ACR (WebApp, FunctionApp, ContainerApp)
- Les 3 variantes ne doivent PAS devenir 3 builders séparés
- Utiliser le même builder avec des branches conditionnelles basées sur `resource.Properties`
- Le `DeployMode` (Code / ContainerManagedIdentity / ContainerAdminCredentials) pilote les branches

### CompanionModules (StorageAccount)
- Les companions sont eux-mêmes des `BicepModuleSpec` (pas de string brut)
- Le `BicepCompanionSpec` contient un `BicepModuleSpec` complet
- L'emitter est réutilisé pour émettre les companions

### ParameterGroupMappings (ContainerApp)
- Ce mécanisme de l'assembler consomme les metadata depuis `GeneratedTypeModule`
- L'IR doit porter cette information via un champ dédié (ou le `ModuleBuildStage` le copie)

### Tests : ce qu'il faut tester par générateur
- Structure du spec (params, outputs, resource, types, companions)
- Chaque variante conditionnelle
- L'émission complète (round-trip spec → string via emitter)
- Les edge cases du générateur (propriétés optionnelles, valeurs par défaut)

---

## Retours d'expérience

> Cette section est alimentée après chaque migration. Les erreurs récurrentes trouvées en revue
> doivent être ajoutées ici pour éviter de les reproduire sur les générateurs suivants.

### Migration #1 — UserAssignedIdentity (Tier 1, 47 LOC)
- **Observation** : Simplest generator — no types, no companions, no variants, no secure params. Pure happy path.
- **DI** : No change needed. The existing `IResourceTypeBicepGenerator` registration is sufficient because `ModuleBuildStage` detects `is IResourceTypeBicepSpecGenerator` at runtime.
- **Legacy `Generate()` kept** : Both `Generate()` and `GenerateSpec()` coexist. `Generate()` is called first, then `GenerateSpec()` overrides if the generator implements the spec interface.
- **Parity** : Emitter output is semantically identical but has cosmetic differences — blank lines between outputs (emitter adds `\n` before each output). This is acceptable per the skill.
- **Test count** : 18 tests covering spec structure, params, resource, outputs, interface contracts, emission content, and legacy backward compat.
- **Règle** : For simple generators with no types/companions, the migration is mechanical: translate the `const string` template into `BicepModuleBuilder` calls. Zero surprises.

### Migration #2 — LogAnalyticsWorkspace (Tier 1, 77 LOC)
- **First generator with types.bicep** — `ExportedType()` on builder works; emitter `EmitTypes()` produces `@export()` + `@description()` + `type Name = ...` correctly.
- **Custom param type** — `BicepType.Custom("SkuName")` creates a `BicepCustomType`. The `LegacyTextModuleAdapter.CreateSkeletonModule` automatically populates `ParameterTypeOverrides` from `BicepCustomType` params.
- **Nested objects** — `Property("properties", props => props.Property("sku", sku => sku.Property(...)))` works cleanly for 2-level nesting. Emitter indentation is correct.
- **Default values** — `BicepStringLiteral("PerGB2018")` and `BicepIntLiteral(30)` / `BicepIntLiteral(-1)` emit as `= 'PerGB2018'` / `= 30` / `= -1`.
- **Règle** — For generators with types.bicep: use `ExportedType()` on the same builder, and `Import()` for the module template. The emitter handles both `EmitModule()` and `EmitTypes()` from the same spec.

### Migration #3 — AppServicePlan (Tier 1, 80 LOC)
- **First generator with variables** — `Var("isLinux", new BicepRawExpression("osType == 'Linux'"))` and `Var("kind", new BicepConditionalExpression(...))` emit correctly as `var isLinux = osType == 'Linux'` and `var kind = isLinux ? 'linux' : 'app'`.
- **Equality comparisons** — No `BicepBinaryExpression` exists in the IR; use `BicepRawExpression("osType == 'Linux'")` for equality checks.
- **BicepConditionalExpression** — Works for ternary: `new BicepConditionalExpression(condition, consequent, alternate)`.
- **Two custom types** — Both `SkuName` and `OsType` imported via single `.Import("./types.bicep", "SkuName", "OsType")`.
- **Parameters dict preservation** — Fixed `ModuleBuildStage`: `LegacyTextModuleAdapter.CreateSkeletonModule(spec)` returns `Parameters = new Dictionary<string, object>()` (empty), but AppServicePlan populates `Parameters` with user-configured values from `resource.Properties`. Fix: `skeleton with { Parameters = module.Parameters }`. This is backward-compatible with previous migrations (they had empty Parameters too).
- **Builder API** — `.Resource(symbol, armType)` takes 2 args (no lambda). Properties added via `.Property()` on the builder itself, not a callback.
- **ExportedType API** — `ExportedType(name, body, description)` — no `isExported` param; it's always `true`.
- **Test count** — 27 tests covering spec structure, imports, 5 params, 2 variables, resource body (5 top-level props + nested sku + nested properties), 1 output, 2 exported types, interface contracts, emission content, legacy backward compat.

### Migration #4 — ContainerRegistry (Tier 1, 84 LOC)
- **Inline ternary in resource body** — `BicepConditionalExpression` works directly inside a `Property()` lambda for nested properties: `.Property("zoneRedundancy", new BicepConditionalExpression(ref, lit1, lit2))`. No variable needed.
- **Bool defaults** — `BicepBoolLiteral(false)` emits as `= false`. First usage of `BicepBoolLiteral` as param default.
- **Empty Parameters dict** — Unlike AppServicePlan, this generator has no user-configured values from `resource.Properties`. The `ModuleBuildStage` fix from migration #3 handles both cases (empty and non-empty Parameters preserved).
- **Mechanical migration** — No new IR features needed. Standard pattern: imports, params, resource with nested objects, outputs, exported types.
- **Test count** — 27 tests covering spec structure, imports, 6 params (custom types + bool defaults), resource body (4 props + nested sku + 3-prop properties), inline ternary, 2 outputs with descriptions, 2 exported types, interface contracts, emission content, legacy backward compat.
- **Tier 1 complete** — All 4 Tier 1 generators (UserAssignedIdentity, LogAnalyticsWorkspace, AppServicePlan, ContainerRegistry) migrated.

### Migration #5 — ServiceBusNamespace (Tier 2, 89 LOC)
- **First Tier 2 generator** — Opens the "medium with types" tier. Patterns from Tier 1 apply directly; no new IR features needed.
- **`listKeys()` output** — Complex expressions like `listKeys('${resource.id}/AuthorizationRules/...', resource.apiVersion).primaryConnectionString` use `BicepRawExpression` verbatim. The emitter outputs them as-is.
- **`sku.tier: sku`** — Param reuse in nested objects: `.Property("tier", new BicepReference("sku"))` maps the same param to two different keys in the sku object.
- **Inline ternary in nested object** — `BicepConditionalExpression` with `BicepRawExpression("sku == 'Premium'")` as condition works for equality-based conditional inside nested property builder.
- **`BicepIntLiteral` as ternary alternate** — `new BicepIntLiteral(0)` as the `Alternate` in a conditional emits as `0` correctly.
- **Test count** — 28 tests covering 7 params, sku with 3 props (name, tier, conditional capacity), properties with 3 props, 2 outputs (including listKeys), 2 exported types, emission parity.

### Migration #6 — AppConfiguration (Tier 2, 91 LOC)
- **Simplest Tier 2 generator** — No conditional logic, no complex outputs. Straightforward param-to-property mapping.
- **Simple sku object** — Unlike ServiceBusNamespace (3 props with conditional), AppConfiguration sku has only `name: sku`. One-liner nested object.
- **`.properties.endpoint` output** — `BicepRawExpression("appConfig.properties.endpoint")` for accessing nested runtime property. No `listKeys()` or interpolation needed.
- **Test property names reminder** — `BicepRawExpression.RawBicep` (not `.Expression`), `BicepCustomType.Name` (not `.TypeName`), `Resource.ArmTypeWithApiVersion` (not `.ArmType`), `Resource.Body` is `IReadOnlyList<BicepPropertyAssignment>` (not `BicepObjectExpression`).
- **Test count** — 27 tests covering 7 params, simple sku, 4-prop properties, 2 outputs, 2 exported types, emission parity.

### Migration #7 — ApplicationInsights (Tier 2, 91 LOC)
- **Top-level `kind` literal** — `BicepStringLiteral("web")` used as a direct resource body property (not a param ref). First generator with a string literal at the resource top level.
- **PascalCase property keys** — `Application_Type`, `WorkspaceResourceId`, `SamplingPercentage`, etc. The builder preserves key casing exactly — no normalization.
- **Cross-resource param** — `logAnalyticsWorkspaceId` is a plain `BicepType.String` param with no default, representing a dependency on another resource's output.
- **3 outputs** — `id`, `instrumentationKey` (`applicationInsights.properties.InstrumentationKey`), `connectionString` (`applicationInsights.properties.ConnectionString`). All via `BicepRawExpression`.
- **Param count off-by-one** — Initially wrote test expecting 7 params but generator has 8 (forgot `ingestionMode`). Always recount from the template.
- **Test count** — 29 tests covering 8 params, `kind` literal, 7-prop `properties` with string literal + param refs, 3 outputs, 1 exported type, emission parity.

### Migration #8 — EventHubNamespace (Tier 2, 97 LOC)
- **Sku name+tier reusing same param** — Both `sku.name` and `sku.tier` reference the same `BicepReference("sku")`. Builder handles this naturally.
- **Conditional `maximumThroughputUnits`** — `BicepConditionalExpression(autoInflateEnabled, maxThroughputUnits, 0)` emits `autoInflateEnabled ? maxThroughputUnits : 0`. Second generator (after ContainerRegistry) with an inline ternary.
- **Property name vs param name mismatch** — `disableLocalAuthentication` (resource property) maps to `disableLocalAuth` (param), `isAutoInflateEnabled` maps to `autoInflateEnabled`. Builder key is the ARM property name, value is `BicepReference("paramName")`.
- **`BicepReference.Symbol`** — Confirmed property name is `.Symbol` (not `.ReferenceName`). Tests initially used wrong name; fixed during build verification.
- **Test count** — 31 tests covering 9 params (2 custom types, 3 bool, 2 int), sku reuse, conditional, 2 outputs, 2 exported types, emission parity.
- **Tier 2 complete** — All 4 Tier 2 generators migrated (ServiceBusNamespace, AppConfiguration, ApplicationInsights, EventHubNamespace).

### Migration #9 — KeyVault (Tier 3, 95 LOC)
- **First generator with dynamic `resource.Properties` injection** — 6 boolean properties read from `resource.Properties.GetValueOrDefault()` with defaults, parsed via `bool.Parse()`, emitted as `BicepBoolLiteral`. Not Bicep params — baked in at generation time.
- **`subscription().tenantId`** — `BicepRawExpression("subscription().tenantId")` for a built-in Bicep function call in a resource property.
- **Nested sku inside properties** — Unlike ServiceBus/AppConfig/EventHub where `sku` is a top-level resource body property, KeyVault puts `sku` inside `properties` per ARM schema. Two-prop object: `family: 'A'` (string literal) + `name: sku` (param ref).
- **`BicepReference.Symbol`** — Confirmed again (not `.ReferenceName`). Pattern is stable.
- **Test CreateResource helper with optional Properties** — `CreateResource(Dictionary<string, string>? properties = null)` enables testing default vs override scenarios. Two dedicated tests verify all 6 booleans with defaults and with full overrides.
- **Test count** — 29 tests covering 3 params, nested sku, tenantId raw expression, 6 dynamic booleans (default + override), 3 outputs, 1 exported type, emission parity with overrides.

### Migration #10 — SqlServer (Tier 3, 97 LOC)
- **First generator with `@secure()` param** — `Param("administratorLoginPassword", BicepType.String, secure: true)`. Builder API `secure:` named arg, IR `BicepParam.IsSecure`, emitter prepends `@secure()` decorator. `LegacyTextModuleAdapter.CreateSkeletonModule()` auto-derives `SecureParameters` from `IsSecure` params.
- **Static const template** — Unlike KeyVault (`BuildModuleTemplate` with dynamic booleans), SqlServer uses a const string template. The `GenerateSpec()` method doesn't read `resource.Properties` — those are only used in legacy `Generate()` for Parameters dict.
- **`publicNetworkAccess: 'Enabled'`** — Hardcoded `BicepStringLiteral("Enabled")` in resource body. Not a param — always emitted as literal.
- **`NormalizeSqlServerVersion()`** — Only used in legacy `Generate()` for the Parameters dict. The IR spec always has the default `'12.0'` — normalization is a deployment-time concern, not a Bicep template concern.
- **Test count** — 30 tests covering 6 params (incl. secure), 5-prop properties, `publicNetworkAccess` literal, 2 outputs, 2 exported types, legacy V12 normalization, SecureParameters, emission parity.

### Migration #11 — SqlDatabase (Tier 3, 91 LOC)
- **First generator requiring IR extension** — SqlDatabase uses `existing` resource declarations and `parent:` references, which the IR did not support. Required adding: `BicepExistingResource` record, `BicepModuleSpec.ExistingResources` collection, `BicepResourceDeclaration.ParentSymbol`, `BicepEmitter.EmitExistingResources()`, `BicepModuleBuilder.ExistingResource()` and `.Parent()` methods.
- **Existing resource pattern** — `ExistingResource("sqlServer", "Microsoft.Sql/servers@2023-08-01-preview", "sqlServerName")` adds to `ExistingResources` list. Emitter emits `resource sqlServer '...' existing = { name: sqlServerName }` block before the main resource.
- **Parent reference** — `Parent("sqlServer")` sets `BicepResourceDeclaration.ParentSymbol`. Emitter emits `parent: sqlServer` as first line inside resource body, before regular `Body` properties.
- **`BicepExistingResource` record** — `(string Symbol, string ArmTypeWithApiVersion, string NameExpression)`. `NameExpression` is raw text (e.g. param name) — not a `BicepExpression` to keep emission simple.
- **Child resource ARM type** — Uses `Microsoft.Sql/servers/databases@...` (slash-delimited child path).
- **Test count** — 33 tests covering 7 params (SkuName custom type), existing resource, parent ref, nested sku + properties, 1 output, 1 exported type, legacy GB→bytes conversion, emission parity.

### Migration #12 — (à compléter)

---

## Ordre de migration recommandé

Voir le fichier de suivi : `docs/architecture/bicep-refactoring/MIGRATION-TRACKER.md`

L'ordre suit le principe : **plus simple d'abord** pour stabiliser l'IR et l'emitter,
**plus complexe ensuite** une fois les patterns éprouvés.

### Tiers de migration

| Tier | Générateurs | Justification |
|------|------------|---------------|
| **Tier 0 — Infra IR** | *(pas un générateur)* | Builder, Emitter, Transformers, Adaptateur legacy, Tests infra |
| **Tier 1 — Simples** | UserAssignedIdentity, LogAnalyticsWorkspace, AppServicePlan, ContainerRegistry | 47-84 lignes, 1 template, pas de companions, pas de variantes |
| **Tier 2 — Moyens avec types** | ServiceBusNamespace, AppConfiguration, ApplicationInsights, EventHubNamespace | 89-97 lignes, types.bicep, dépendances parent simples |
| **Tier 3 — Moyens avec logique** | KeyVault, SqlServer, SqlDatabase, CosmosDb, RedisCache, ContainerAppEnvironment | 95-121 lignes, template dynamique ou params multiples, parent refs |
| **Tier 4 — Complexes avec variantes** | WebApp, FunctionApp | ~350 lignes, 3 variantes ACR, custom domains, parent refs |
| **Tier 5 — Très complexes** | ContainerApp, StorageAccount | 400-500 lignes, variantes + companions + types custom + group mappings |
