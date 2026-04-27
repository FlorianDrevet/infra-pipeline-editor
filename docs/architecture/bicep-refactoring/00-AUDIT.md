# Audit — Mécanisme de génération Bicep

> Date : 2026-04-27 — Périmètre : `src/Api/InfraFlowSculptor.BicepGeneration/*`
> Objectif : identifier les pain points structurels et proposer un design pattern de refactoring.

---

## 1. Cartographie actuelle

| Composant | Lignes | Rôle |
|---|---:|---|
| `BicepGenerationEngine` | **920** | Orchestre tout : calcul d'identités, injection regex, routage FK, normalisation, délégation à l'assembler |
| `BicepAssembler` | 202 | Orchestrateur final (types/functions/main/params/modules) |
| `MainBicepAssembler` | **525** | Génération procédurale du `main.bicep` |
| `MonoRepoBicepAssembler` | 189 | Re-déduplication cross-config |
| `ParameterFileAssembler` | 215 | Génération des `.bicepparam` |
| `Generators/ContainerApp*` | **439** | Template + 3 variantes (no ACR / ACR MI / ACR Admin) |
| `Generators/StorageAccount*` | 346 | Template + companions (blob/queue/table) |
| `Generators/FunctionApp*` | 312 | idem ContainerApp |
| `Generators/WebApp*` | 280 | idem ContainerApp |
| 14 autres générateurs | 60–100 | Strategy par ressource ARM |
| `Helpers/*` (6 fichiers) | 50–130 | Fonctions pures (formatting, naming, identifier) |

**Pattern actuel :** Strategy (`IResourceTypeBicepGenerator`) + Orchestrator (`BicepGenerationEngine`) + Assembler procédural.

---

## 2. Pain points (par ordre de gravité)

### 🔴 P1 — Manipulation de texte Bicep par regex
Les générateurs produisent un `string ModuleBicepContent`. Le moteur réécrit ensuite ce texte via 7 méthodes `Inject*` à coups de `Regex` :

```csharp
// BicepGenerationEngine.cs
moduleBicepContent = InjectSystemAssignedIdentity(moduleBicepContent);
moduleBicepContent = InjectUserAssignedIdentity(moduleBicepContent, uaiIdentifiers!, needsSystem);
moduleBicepContent = InjectOutputDeclarations(moduleBicepContent, outputs);
moduleBicepContent = InjectAppSettingsParam(moduleBicepContent, resource.Type);
moduleBicepContent = InjectTagsParam(moduleBicepContent);
```

Conséquences directes :
- **Impossible à tester unitairement** sans monter un template Bicep textuel complet
- **Fragile à toute évolution syntaxique** (ajout d'un commentaire, d'un attribut décoratif)
- Les templates `const string` doivent respecter une mise en forme précise pour que les regex `^  identity:`, `^  properties:`, `siteConfig\s*:`, `\bresources\s*:\s*\{` matchent
- Tout l'effort de tests passe par les **golden files** (`tests/InfraFlowSculptor.GenerationParity.Tests/`) → aucune barrière intermédiaire avant la comparaison byte-à-byte

### 🔴 P2 — `BicepGenerationEngine` est un God Object
920 lignes mélangent 6 responsabilités distinctes :

1. **Analyse du graphe** des besoins d'identité (méthodes `ComputeIdentityKindsByArmType`, calcul des sets `sourceResourcesNeedingSystemIdentity` / `sourceResourcesNeedingUserIdentity`)
2. **Détection des FK cross-resource** (`appServicePlanId`, `containerAppEnvironmentId`, `logAnalyticsWorkspaceId`, `sqlServerId`) avec fallbacks chaînés
3. **Détection compute-types-with-app-settings**
4. **Injection regex** des blocs identity / outputs / appSettings / tags / envVars
5. **Délégation** à `BicepAssembler` / `MonoRepoBicepAssembler`
6. **Pruning** des outputs non utilisés

Aucune de ces 6 responsabilités n'a sa propre frontière testable.

### 🟠 P3 — Magic strings ARM hardcodés
Malgré la règle projet (« Magic strings : Never hardcode Azure resource type identifiers — use `AzureResourceTypes.*` »), `BicepGenerationEngine` contient :

```csharp
private static readonly HashSet<string> ComputeResourceTypes = new(StringComparer.OrdinalIgnoreCase)
{
    "Microsoft.Web/sites",
    "Microsoft.Web/sites/functionapp",
    "Microsoft.App/containerApps",
};

if (resource.Type != "Microsoft.ManagedIdentity/userAssignedIdentities")
if (resource.Type is "Microsoft.Insights/components" or "Microsoft.App/managedEnvironments")
```

### 🟠 P4 — `GeneratedTypeModule` = God DTO
Le DTO porte ~20 propriétés optionnelles : `ModuleName`, `ModuleFileName`, `ModuleBicepContent`, `ModuleTypesBicepContent`, `Parameters`, `ParameterTypeOverrides`, `ParameterGroupMappings`, `SecureParameters`, `CompanionModules`, `IdentityKind`, `UsesParameterizedIdentity`, `ParentModuleIdReferences`, `ParentModuleNameReferences`, `ExistingResourceIdReferences`, etc.

Chaque ressource n'utilise qu'une fraction de ces champs. Pas d'invariants : un même DTO peut décrire un Key Vault sans identité, un Container App avec ACR + identité paramétrée + 4 FK, et personne ne sait quels champs sont cohérents ensemble.

### 🟠 P5 — Variantes de templates par `const string` qui se dupliquent
`ContainerAppTypeBicepGenerator` contient **3 templates** :
- `ContainerAppModuleTemplate`
- `ContainerAppWithAcrManagedIdentityModuleTemplate`
- `ContainerAppWithAcrAdminCredentialsModuleTemplate`

Idem `WebApp` et `FunctionApp`. Toute évolution de la baseline (probes, scaling, ingress) doit être propagée à la main dans 3 chaînes de caractères. C'est la cause directe de la note `[2026-04-23] Compute Module Variant Files` dans la mémoire (BCP037 quand le file name est partagé entre variantes incompatibles).

### 🟡 P6 — `MainBicepAssembler` procédural
525 lignes avec un `StringBuilder` géant. Aucune abstraction « section » (imports, params, modules, outputs). Toute nouvelle section = patch ligne par ligne dans la méthode `Generate`.

### 🟡 P7 — Couplage `BicepGenerationEngine` ↔ `BicepAssembler`
`BicepGenerationEngine.GenerateCore` se termine par un appel direct à `BicepAssembler.Assemble(...)` (méthode statique). Pas d'interface, pas de mock possible, pas de remplacement par un format alternatif (JSON ARM, Terraform).

### 🟡 P8 — Disambiguation de modules par hash de contenu
`BicepAssembler.NormalizePrimaryModuleFileNames` doit hasher le contenu pour suffixer les noms de fichier (`.{8 hex}.module.bicep`) quand plusieurs ressources produisent du Bicep différent pour le même nom logique. Symptôme : le contenu n'est pas dérivé d'une description structurée — il dépend des injections regex et du `ResourceDefinition` source. Avec une vraie représentation intermédiaire, la disambiguation se ferait sur les *features actives*, pas sur un hash opaque.

---

## 3. Critères de choix pour le refactoring

| Critère | Pondération |
|---|---|
| Réduit la manipulation de texte Bicep | **Critique** |
| Rend les transformations testables unitairement | **Critique** |
| Casse `BicepGenerationEngine` en composants ≤ 200 lignes | Élevée |
| Préserve / facilite la parité golden tests | Élevée |
| Coût de migration progressif (pas de big bang) | Élevée |
| Réutilisable pour `PipelineGenerationEngine` | Moyenne |
| Familier pour un dev .NET sans bagage compilateur | Moyenne |

---

## 4. Patterns évalués (3 retenus)

| Pattern | Fichier | Verdict |
|---|---|---|
| **Builder + Intermediate Representation (IR)** | [`02-pattern-builder-ir.md`](02-pattern-builder-ir.md) | ✅ **Recommandé en cible long terme** — résout P1, P4, P5, P8 |
| **Pipeline (chaîne de stages)** | [`01-pattern-pipeline.md`](01-pattern-pipeline.md) | ✅ **Recommandé en première étape** — résout P2, P3 sans toucher aux templates |
| **Visitor (sur AST)** | [`03-pattern-visitor.md`](03-pattern-visitor.md) | ⚠️ **Pertinent uniquement si Builder+IR adopté** — sinon overkill |

### Patterns écartés (et pourquoi)

| Pattern | Pourquoi écarté |
|---|---|
| **Chain of Responsibility pur** | Trop souple : un stage peut « passer » la main, ce qui rend l'ordre implicite. Pour la génération Bicep, l'ordre des transformations est **strict** (identity AVANT outputs AVANT app settings) — une Pipeline explicite est meilleure |
| **Mediator** | Le problème n'est pas la coordination entre composants indépendants, c'est l'absence de modèle. Un Mediator par-dessus `BicepGenerationEngine` ne ferait que masquer le God Object |
| **Decorator sur `IResourceTypeBicepGenerator`** | Permettrait de composer ACR / Identity sans variantes hardcodées, mais sans IR le décorateur doit toujours réécrire du texte Bicep — déplace le problème |
| **Interpreter** | Pertinent si on devait *parser* du Bicep entrant. Ici on émet, donc inadapté |
| **Template Method** | Déjà implicitement présent (chaque générateur suit la même forme `Generate(ResourceDefinition) → GeneratedTypeModule`). Ne résout aucun pain point |

---

## 5. Stratégie de migration recommandée

> **Approche en 2 vagues** plutôt qu'un big bang. La Pipeline est livrable indépendamment et apporte déjà 60 % de la valeur.

### Vague 1 — Pipeline (1 sprint)
Extraire les 6 responsabilités de `BicepGenerationEngine` en stages explicites. Garder `string ModuleBicepContent` et les regex `Inject*` à l'intérieur des stages correspondants. Résultat :
- `BicepGenerationEngine` ≤ 80 lignes (orchestration de la pipeline uniquement)
- 6 stages testables indépendamment avec des fixtures `GenerationContext`
- Magic strings ARM extraits dans `AzureResourceTypes.ComputeArmTypes`

**Aucun changement de comportement** → golden tests passent à l'identique.

### Vague 2 — Builder + IR (2-3 sprints)
Introduire un `BicepModuleAst` (record graph immuable). Migrer générateur par générateur, en commençant par les plus petits (`UserAssignedIdentity`, `LogAnalyticsWorkspace`). Le `BicepEmitter` produit le `string` final. Les méthodes `Inject*` deviennent des transformations sur l'AST (`AddIdentity`, `AddOutput`, `AddParam`).

**Chemin de coexistence** : tant qu'un générateur produit encore du `string` brut, un adaptateur `LegacyTextModule : IBicepModuleAst` permet la cohabitation.

### Vague 3 (optionnelle) — Visitor
Une fois l'AST stabilisé, ajouter un Visitor pour les opérations transverses (formatting, validation, génération JSON ARM alternative, génération Terraform). Pertinent si plusieurs *backends* d'émission émergent.

---

## 6. Décision attendue

Lis les 3 fiches patterns. La question n'est pas « lequel choisir » mais :

1. **« Aller jusqu'à l'AST ? »** Si oui : Vague 1 + Vague 2.
2. **« Rester sur du texte mais structurer l'orchestration ? »** Si oui : Vague 1 seule.

Le Visitor n'est jamais un point d'entrée — c'est une optimisation de l'AST.
