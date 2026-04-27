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

### Migration #1 — (à compléter après UserAssignedIdentity)
<!-- Format: 
- **Erreur** : description
- **Cause** : pourquoi c'est arrivé  
- **Fix** : ce qui a été corrigé
- **Règle** : la règle à appliquer pour les suivants
-->

### Migration #2 — (à compléter)

### Migration #3 — (à compléter)

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
