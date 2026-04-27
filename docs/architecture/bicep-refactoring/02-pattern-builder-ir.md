# Pattern 2 — Builder + Intermediate Representation (IR)

> **Verdict :** ✅ **Recommandé en cible long terme** (Vague 2). Résout le problème racine : la manipulation de texte Bicep par regex. Coût significatif mais éliminable progressivement, générateur par générateur.

---

## 1. Idée

Le problème racine est que les générateurs produisent une **string** dès le départ. Toutes les transformations qui suivent doivent ré-analyser ce texte pour le muter — d'où les regex, la fragilité, les hash de contenu pour disambiguation.

Le pattern **Builder + IR** introduit une **représentation intermédiaire typée** entre les générateurs et l'émission Bicep finale :

```
ResourceDefinition → IResourceTypeBicepGenerator → BicepModuleSpec (IR objet)
                                                         │
                                                         ▼
                                       Stages de transformation (typés, non textuels)
                                                         │
                                                         ▼
                                                   BicepEmitter
                                                         │
                                                         ▼
                                                   string Bicep
```

C'est l'approche utilisée par :
- **Roslyn** (`SyntaxNode` → `SyntaxRewriter` → texte)
- **Bicep officiel lui-même** (`SyntaxBase` → `EmitterContext` → ARM JSON)
- **Pulumi** (resource graph typé → cloud provider)

---

## 2. Architecture cible

### 2.1 — Le modèle IR (records immuables)

```csharp
namespace InfraFlowSculptor.BicepGeneration.Ir;

/// <summary>Spécification structurée d'un module Bicep, agnostique du formatage texte.</summary>
public sealed record BicepModuleSpec
{
    public required string ModuleName { get; init; }
    public required string ModuleFolderName { get; init; }
    public required string ResourceTypeName { get; init; }

    public ImmutableList<BicepImport> Imports { get; init; } = [];
    public ImmutableList<BicepParam> Parameters { get; init; } = [];
    public ImmutableList<BicepVar> Variables { get; init; } = [];
    public required BicepResourceDeclaration Resource { get; init; }
    public ImmutableList<BicepOutput> Outputs { get; init; } = [];

    public ImmutableList<BicepTypeDefinition> ExportedTypes { get; init; } = [];
    public ImmutableList<BicepCompanionSpec> Companions { get; init; } = [];
}

public sealed record BicepParam(
    string Name,
    BicepType Type,
    string? Description = null,
    bool IsSecure = false,
    BicepExpression? DefaultValue = null);

public sealed record BicepResourceDeclaration
{
    public required string Symbol { get; init; }
    public required string ArmTypeWithApiVersion { get; init; }
    public required BicepObjectExpression Properties { get; init; }
    public BicepIdentityBlock? Identity { get; init; }     // ← typé, pas du texte !
    public BicepObjectExpression? SiteConfig { get; init; }
    public ImmutableList<BicepEnvVar> EnvVars { get; init; } = [];
    public ImmutableList<BicepAppSetting> AppSettings { get; init; } = [];
}

public sealed record BicepIdentityBlock
{
    public required IdentityKind Type { get; init; }                      // enum
    public ImmutableList<string> UserAssignedIdentityRefs { get; init; } = [];
    public bool IsParameterized { get; init; }                            // remplace le bool dans GeneratedTypeModule
}

public enum IdentityKind { None, SystemAssigned, UserAssigned, Both }

public abstract record BicepExpression;
public sealed record BicepLiteral(object Value) : BicepExpression;
public sealed record BicepReference(string Symbol) : BicepExpression;
public sealed record BicepFunctionCall(string Name, ImmutableList<BicepExpression> Args) : BicepExpression;
public sealed record BicepObjectExpression(ImmutableDictionary<string, BicepExpression> Properties) : BicepExpression;
public sealed record BicepArrayExpression(ImmutableList<BicepExpression> Items) : BicepExpression;
```

### 2.2 — Le Builder fluent (utilisé par les générateurs)

```csharp
public sealed class BicepModuleBuilder
{
    private string? _moduleName;
    private string? _folderName;
    private string? _resourceTypeName;
    private string? _resourceSymbol;
    private string? _armType;
    private readonly Dictionary<string, BicepExpression> _properties = [];
    private readonly List<BicepParam> _parameters = [];
    private readonly List<BicepOutput> _outputs = [];
    private BicepIdentityBlock? _identity;

    public BicepModuleBuilder Module(string name, string folder, string resourceTypeName)
    {
        _moduleName = name; _folderName = folder; _resourceTypeName = resourceTypeName;
        return this;
    }

    public BicepModuleBuilder Resource(string symbol, string armType)
    {
        _resourceSymbol = symbol; _armType = armType;
        return this;
    }

    public BicepModuleBuilder Param(string name, BicepType type, string? description = null,
        bool secure = false, BicepExpression? defaultValue = null)
    {
        _parameters.Add(new BicepParam(name, type, description, secure, defaultValue));
        return this;
    }

    public BicepModuleBuilder Property(string key, BicepExpression value)
    {
        _properties[key] = value;
        return this;
    }

    public BicepModuleBuilder WithSystemAssignedIdentity()
    {
        _identity = new BicepIdentityBlock { Type = IdentityKind.SystemAssigned };
        return this;
    }

    public BicepModuleBuilder Output(string name, BicepType type, BicepExpression expression, bool secure = false)
    {
        _outputs.Add(new BicepOutput(name, type, expression, secure));
        return this;
    }

    public BicepModuleSpec Build() => new()
    {
        ModuleName = _moduleName!,
        ModuleFolderName = _folderName!,
        ResourceTypeName = _resourceTypeName!,
        Parameters = [.._parameters],
        Outputs = [.._outputs],
        Resource = new BicepResourceDeclaration
        {
            Symbol = _resourceSymbol!,
            ArmTypeWithApiVersion = _armType!,
            Properties = new BicepObjectExpression(_properties.ToImmutableDictionary()),
            Identity = _identity,
        },
    };
}
```

### 2.3 — Un générateur réécrit avec le Builder

```csharp
public sealed class KeyVaultTypeBicepGenerator : IResourceTypeBicepGenerator
{
    public string ResourceType => AzureResourceTypes.ArmTypes.KeyVault;
    public string ResourceTypeName => AzureResourceTypes.KeyVault;

    public BicepModuleSpec Generate(ResourceDefinition resource)
        => new BicepModuleBuilder()
            .Module("keyVault", folder: "KeyVault", resourceTypeName: ResourceTypeName)
            .Param("location", BicepType.String)
            .Param("name", BicepType.String)
            .Param("tenantId", BicepType.String, defaultValue: new BicepFunctionCall("tenant", [new BicepReference("()")]))
            .Param("sku", BicepType.String, defaultValue: new BicepLiteral("standard"))
            .Resource("kv", "Microsoft.KeyVault/vaults@2023-07-01")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("properties", new BicepObjectExpression(ImmutableDictionary.CreateRange(new[]
            {
                KeyValuePair.Create<string, BicepExpression>("tenantId", new BicepReference("tenantId")),
                KeyValuePair.Create<string, BicepExpression>("sku", new BicepObjectExpression(/* ... */)),
                KeyValuePair.Create<string, BicepExpression>("enableRbacAuthorization", new BicepLiteral(true)),
            })))
            .Output("id", BicepType.String, new BicepReference("kv.id"))
            .Output("name", BicepType.String, new BicepReference("kv.name"))
            .Build();
}
```

> Plus de template `const string`. Plus de regex pour injecter quoi que ce soit après coup.

### 2.4 — Les transformations deviennent typées

Au lieu de `InjectSystemAssignedIdentity(string moduleBicep)` (regex sur du texte), on a :

```csharp
internal static class IdentityTransformer
{
    public static BicepModuleSpec WithSystemAssignedIdentity(this BicepModuleSpec spec)
    {
        if (spec.Resource.Identity is not null)
            return spec; // déjà présent — invariant clair, pas un regex qui peut faux-positiver

        var identity = new BicepIdentityBlock { Type = IdentityKind.SystemAssigned };
        var principalIdOutput = new BicepOutput(
            "principalId",
            BicepType.String,
            new BicepReference($"{spec.Resource.Symbol}.identity.principalId"));

        return spec with
        {
            Resource = spec.Resource with { Identity = identity },
            Outputs = spec.Outputs.Add(principalIdOutput),
        };
    }
}
```

**Comparaison avec l'actuel :**

| Aujourd'hui | Avec IR |
|---|---|
| 60 lignes de regex + `StringBuilder` + `FindClosingBrace` | 8 lignes de transformation immuable |
| Test = monter un template Bicep textuel parfait | Test = `spec.WithSystemAssignedIdentity().Resource.Identity.Type.Should().Be(SystemAssigned)` |
| Si le template change un espace, la regex casse | Le test reste vrai |

### 2.5 — L'Emitter (un seul, isolé, testable)

```csharp
public sealed class BicepEmitter
{
    public string Emit(BicepModuleSpec spec)
    {
        var sb = new StringBuilder();
        EmitImports(sb, spec.Imports);
        EmitParameters(sb, spec.Parameters);
        EmitVariables(sb, spec.Variables);
        EmitResource(sb, spec.Resource);
        EmitOutputs(sb, spec.Outputs);
        return sb.ToString();
    }

    private void EmitResource(StringBuilder sb, BicepResourceDeclaration r)
    {
        sb.Append("resource ").Append(r.Symbol).Append(" '").Append(r.ArmTypeWithApiVersion).AppendLine("' = {");
        sb.Append("  name: ").AppendLine(EmitExpression(r.Properties.Properties["name"]));
        sb.Append("  location: ").AppendLine(EmitExpression(r.Properties.Properties["location"]));

        if (r.Identity is { } identity)
            EmitIdentityBlock(sb, identity);

        EmitProperties(sb, r.Properties);
        sb.AppendLine("}");
    }

    private void EmitIdentityBlock(StringBuilder sb, BicepIdentityBlock identity)
    {
        sb.AppendLine("  identity: {");
        if (identity.IsParameterized)
            sb.AppendLine("    type: identityType");
        else
            sb.Append("    type: '").Append(SerializeKind(identity.Type)).AppendLine("'");

        if (identity.UserAssignedIdentityRefs.Count > 0)
            sb.AppendLine("    userAssignedIdentities: { '${userAssignedIdentityId}': {} }");

        sb.AppendLine("  }");
    }
}
```

L'Emitter est **le seul endroit** qui décide du formatage. Si demain on veut changer l'indentation ou ajouter des commentaires, c'est ici.

### 2.6 — Adapter pour migration progressive

Pour migrer générateur par générateur, introduire :

```csharp
/// <summary>Wrappe un texte Bicep legacy comme un IR opaque, le temps de migrer le générateur.</summary>
internal sealed record LegacyBicepModuleSpec(string RawContent, string ModuleName, string FolderName)
    : BicepModuleSpec
{
    // L'Emitter détecte le type et émet RawContent tel quel
}
```

Cela permet de migrer Key Vault → Builder cette semaine, Container App le mois prochain, sans casser la chaîne.

---

## 3. Avantages

| Bénéfice | Impact |
|---|---|
| **Élimine les regex sur Bicep** (P1 résolu) | Critique |
| **Transformations testables unitairement sans golden file** | Critique |
| **`GeneratedTypeModule` God DTO disparaît** : remplacé par un graphe typé avec invariants clairs (P4 résolu) | Élevé |
| **Plus de variantes de templates `const string`** : ACR / no ACR / admin = différentes branches du Builder, partagent la baseline (P5 résolu) | Élevé |
| **Disambiguation par feature, pas par hash** (P8 résolu) : deux specs sont identiques si leurs records sont égaux | Élevé |
| **Émetteur central** : changer le formatage Bicep = 1 fichier | Moyen |
| **Permet plusieurs backends** : émetteur ARM JSON, émetteur Terraform, émetteur de schéma JSON pour le frontend | Moyen (open door) |
| **Cohérent avec Bicep officiel** (qui suit le même pattern interne) | Crédibilité technique |

---

## 4. Inconvénients

| Risque | Sévérité | Mitigation |
|---|---|---|
| **Coût de migration élevé** : 18 générateurs à réécrire, dont 4 gros (ContainerApp, StorageAccount, FunctionApp, WebApp) | 🔴 Important | Migration progressive avec `LegacyBicepModuleSpec`. Commencer par les petits (UAI, LAW, AppInsights) pour stabiliser l'IR, finir par les gros |
| **Surface d'API IR à concevoir** : risque de sur-modéliser (chaque concept Bicep = un record) ou sous-modéliser (manque l'expressivité requise) | 🔴 Important | Démarrer minimaliste : `BicepExpression` polymorphe + `BicepObjectExpression` couvrent 80 % des besoins. Étendre quand le générateur ContainerApp force la main |
| **Risque de divergence visuelle** : l'Emitter peut produire un Bicep légèrement différent (espacement, ordre des props) → golden tests cassent | 🟠 Moyen | **Migrer les goldens en parallèle** : pour chaque générateur, regénérer le golden avec `dotnet test -p:DefineConstants=REGENERATE_GOLDENS` après revue manuelle. Ne PAS faire en même temps une migration et un changement de comportement |
| **Courbe d'apprentissage pour l'équipe** : nouveau vocabulaire (IR, Spec, Emitter, expressions polymorphes) | 🟠 Moyen | Un README dans `BicepGeneration/Ir/` + un test exemplaire par générateur |
| **L'IR doit être stable** : changer un record IR impacte tous les générateurs | 🟠 Moyen | Versionner via attributs ou ne casser l'IR qu'aux montées de version majeures |
| **Performance** : allocations records immuables vs `StringBuilder` mutable | 🟢 Faible | La génération Bicep est en dehors du chemin chaud (commande utilisateur, < 1s). Aucune mesure ne suggère un problème |
| **Plus de fichiers** (records IR + Builder + Emitter + Transformations) | 🟢 Faible | Découpage net : `Ir/`, `Ir/Builder/`, `Ir/Transformations/`, `Ir/Emit/` |
| **Le moteur reste un orchestrateur** : Builder+IR ne remplace pas la Pipeline | 🟢 Faible | C'est un complément, pas une alternative — Vague 1 + Vague 2 sont compatibles |

---

## 5. Coût de migration

| Tâche | Charge |
|---|---|
| Concevoir l'IR (records + invariants) | M |
| `BicepModuleBuilder` fluent | S |
| `BicepEmitter` (la pièce qui consomme du temps) | L |
| Migrer 6 petits générateurs (UAI, LAW, AppInsights, AppConfig, EventHub, ServiceBus) | M |
| Migrer Key Vault, SQL Server, SQL DB, ACR, Cosmos, Redis | M |
| Migrer ContainerAppEnvironment, AppServicePlan | S |
| Migrer Container App, Web App, Function App (gros) | L |
| Migrer Storage Account (companions) | M |
| Réécrire les transformations (`Inject*` → méthodes d'extension sur l'IR) | M |
| Régénérer + revoir tous les goldens | M |
| **Total** | **2-3 sprints** (10-15j ingé) |

---

## 6. Risques projet à valider avant de lancer

1. **Parité golden tests** : la régénération peut introduire des diffs cosmétiques (ordre des params, indentation). Le PR doit présenter les diffs golden ligne par ligne pour validation manuelle.
2. **Coexistence avec `MonoRepoBicepAssembler`** : il opère sur des `GeneratedTypeModule` à plusieurs configs. Il faut soit migrer aussi cet assembler vers l'IR, soit garder un pont temporaire `BicepModuleSpec → GeneratedTypeModule`.
3. **`PipelineGenerationEngine`** : si le pattern fonctionne pour Bicep, il devra être appliqué aussi au pipeline (d'où la valeur d'un IR générique qui sait émettre Bicep ET YAML).

---

## 7. Quand ce pattern est nécessaire

Si l'équipe répond OUI à au moins une de ces questions :
- « On veut pouvoir tester unitairement chaque transformation sans monter un template complet »
- « On veut éliminer les `const string` Bicep et leur duplication »
- « On envisage un jour un backend Terraform / ARM JSON »
- « Les bugs récents (BCP037, hash de contenu) montrent que la fragilité regex coûte cher »

→ alors Builder + IR est **incontournable**, pas une coquetterie.
