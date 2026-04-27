# Pattern 1 — Pipeline (chaîne de stages)

> **Verdict :** ✅ Recommandé en **Vague 1**. Coût faible, gain immédiat sur P2 (God Object) et P3 (magic strings). Préserve le comportement actuel byte-à-byte.

---

## 1. Idée

`BicepGenerationEngine.GenerateCore` exécute aujourd'hui ~6 phases dans une seule méthode de 200+ lignes. Le pattern **Pipeline** (cousin du Chain of Responsibility, mais avec ordre explicite et pas de short-circuit) extrait chaque phase dans une classe dédiée qui reçoit et mute un **contexte partagé** (`GenerationContext`).

C'est exactement le pattern utilisé par :
- ASP.NET Core middleware
- Roslyn `CompilationStage`
- MediatR `IPipelineBehavior`

---

## 2. Architecture cible

```
GenerationRequest
      │
      ▼
┌──────────────────────────────────────────────┐
│ BicepGenerationPipeline (orchestrateur ~80L) │
└──────────────────────────────────────────────┘
      │
      ▼
GenerationContext (mutable, traverse tous les stages)
      │
      ▼
[1] IdentityAnalysisStage          ← calcule les sets system/user identity
[2] ComputeAppSettingsStage        ← calcule computeArmTypesWithAppSettings + outputsBySourceResource
[3] ModuleBuildStage                ← appelle chaque IResourceTypeBicepGenerator
[4] IdentityInjectionStage          ← regex Inject* identity (déplacée intacte ici)
[5] OutputAndAppSettingsStage       ← regex Inject* outputs / appSettings / envVars / tags
[6] ParentReferenceResolutionStage  ← FK appServicePlanId / containerAppEnvironmentId / etc.
[7] AssemblyStage                   ← délègue à BicepAssembler
[8] OutputPruningStage              ← PruneModuleOutputs
      │
      ▼
GenerationResult
```

Chaque stage implémente la même interface :

```csharp
internal interface IBicepGenerationStage
{
    void Execute(GenerationContext context);
}
```

Le contexte est un **record mutable** qui accumule les artefacts intermédiaires :

```csharp
internal sealed class GenerationContext
{
    public required GenerationRequest Request { get; init; }

    // Calculés par IdentityAnalysisStage
    public HashSet<(string Name, string Type)> SystemIdentityResources { get; set; } = [];
    public Dictionary<(string Name, string Type), List<string>> UserIdentityResources { get; set; } = [];
    public HashSet<string> MixedIdentityArmTypes { get; set; } = [];

    // Calculés par ComputeAppSettingsStage
    public HashSet<string> ComputeArmTypesWithAppSettings { get; set; } = [];
    public Dictionary<string, List<OutputDescriptor>> OutputsBySourceResource { get; set; } = [];

    // Produit par ModuleBuildStage, muté par les stages suivants
    public List<GeneratedTypeModule> Modules { get; } = [];

    // Produit par AssemblyStage
    public GenerationResult? Result { get; set; }
}
```

---

## 3. Exemple d'implémentation

### 3.1 — Le moteur devient un orchestrateur trivial

```csharp
public sealed class BicepGenerationEngine
{
    private readonly IReadOnlyList<IBicepGenerationStage> _stages;

    public BicepGenerationEngine(IEnumerable<IBicepGenerationStage> stages)
    {
        // Ordre injecté via DI (voir DependencyInjection)
        _stages = stages.ToList();
    }

    public GenerationResult Generate(GenerationRequest request)
    {
        var context = new GenerationContext { Request = request };

        foreach (var stage in _stages)
        {
            stage.Execute(context);
        }

        return context.Result
            ?? throw new InvalidOperationException("Pipeline did not produce a result.");
    }
}
```

### 3.2 — Un stage dédié à l'analyse d'identité

```csharp
internal sealed class IdentityAnalysisStage : IBicepGenerationStage
{
    public void Execute(GenerationContext ctx)
    {
        var request = ctx.Request;

        ctx.SystemIdentityResources = request.RoleAssignments
            .Where(ra => ra.ManagedIdentityType == "SystemAssigned")
            .Select(ra => (ra.SourceResourceName, ra.SourceResourceType))
            .ToHashSet();

        ctx.UserIdentityResources = request.RoleAssignments
            .Where(ra => ra.ManagedIdentityType == "UserAssigned"
                      && ra.UserAssignedIdentityName is not null)
            .GroupBy(ra => (ra.SourceResourceName, ra.SourceResourceType))
            .ToDictionary(
                g => g.Key,
                g => g.Select(ra => BicepIdentifierHelper.ToBicepIdentifier(ra.UserAssignedIdentityName!))
                      .Distinct(StringComparer.OrdinalIgnoreCase)
                      .ToList());

        // Inclusion des UAI assignés sans rôle (ex-AssignedUserAssignedIdentityName)
        foreach (var resource in request.Resources)
        {
            if (resource.AssignedUserAssignedIdentityName is null) continue;
            // ... logique existante extraite telle quelle ...
        }

        ctx.MixedIdentityArmTypes = ComputeIdentityKindsByArmType(
            request.Resources, ctx.SystemIdentityResources, ctx.UserIdentityResources)
            .Where(kv => kv.Value.Count > 1)
            .Select(kv => kv.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, HashSet<string>> ComputeIdentityKindsByArmType(/*…*/)
        => /* méthode statique privée déplacée depuis BicepGenerationEngine */;
}
```

### 3.3 — Un stage pour l'injection d'identité (les regex restent)

```csharp
internal sealed class IdentityInjectionStage : IBicepGenerationStage
{
    public void Execute(GenerationContext ctx)
    {
        for (var i = 0; i < ctx.Modules.Count; i++)
        {
            var module = ctx.Modules[i];
            var resource = ctx.Request.Resources.Single(r => r.Name == module.LogicalResourceName);
            var key = (resource.Name, resource.Type);

            var needsSystem = ctx.SystemIdentityResources.Contains(key);
            ctx.UserIdentityResources.TryGetValue(key, out var uais);
            var needsUser = resource.Type != AzureResourceTypes.ArmTypes.UserAssignedIdentity
                            && uais is { Count: > 0 };
            var isMixed = ctx.MixedIdentityArmTypes.Contains(resource.Type);

            var content = module.ModuleBicepContent;

            if (isMixed)
            {
                var hasAnyUaiForType = ctx.UserIdentityResources.Any(kv => kv.Key.SourceResourceType == resource.Type);
                content = BicepIdentityInjector.InjectParameterized(content, hasAnyUaiForType);
            }
            else
            {
                if (needsSystem) content = BicepIdentityInjector.InjectSystemAssigned(content);
                if (needsUser)   content = BicepIdentityInjector.InjectUserAssigned(content, uais!, needsSystem);
            }

            ctx.Modules[i] = module with
            {
                ModuleBicepContent = content,
                IdentityKind = ResolveIdentityKind(needsSystem, needsUser),
                UsesParameterizedIdentity = isMixed,
                ModuleTypesBicepContent = isMixed
                    ? module.ModuleTypesBicepContent + BicepIdentityInjector.ManagedIdentityTypeBicepType
                    : module.ModuleTypesBicepContent,
            };
        }
    }
}
```

> Les méthodes regex `Inject*` sont déplacées **telles quelles** dans une classe statique `BicepIdentityInjector`. Aucune réécriture syntaxique n'est nécessaire à ce stade — c'est ce qui garantit la parité avec les golden tests.

### 3.4 — Enregistrement DI (ordre garanti)

```csharp
// InfraFlowSculptor.Application/DependencyInjection.cs
services.AddSingleton<IBicepGenerationStage, IdentityAnalysisStage>();
services.AddSingleton<IBicepGenerationStage, ComputeAppSettingsStage>();
services.AddSingleton<IBicepGenerationStage, ModuleBuildStage>();
services.AddSingleton<IBicepGenerationStage, IdentityInjectionStage>();
services.AddSingleton<IBicepGenerationStage, OutputAndAppSettingsStage>();
services.AddSingleton<IBicepGenerationStage, ParentReferenceResolutionStage>();
services.AddSingleton<IBicepGenerationStage, AssemblyStage>();
services.AddSingleton<IBicepGenerationStage, OutputPruningStage>();
services.AddSingleton<BicepGenerationEngine>();
```

> ⚠️ **Attention DI .NET** : `IEnumerable<T>` injecté respecte l'ordre d'enregistrement. Si on veut garantir l'ordre indépendamment de l'enregistrement, exposer `int Order { get; }` sur l'interface et trier dans le constructeur du moteur.

### 3.5 — Test unitaire rendu trivial

```csharp
[Fact]
public void IdentityAnalysisStage_DetectsMixedArmType_WhenSystemAndUserCoexist()
{
    var ctx = new GenerationContext
    {
        Request = new GenerationRequest
        {
            Resources = [
                new ResourceDefinition { Name = "wa-1", Type = "Microsoft.Web/sites" },
                new ResourceDefinition { Name = "wa-2", Type = "Microsoft.Web/sites" },
            ],
            RoleAssignments = [
                new() { SourceResourceName = "wa-1", SourceResourceType = "Microsoft.Web/sites",
                        ManagedIdentityType = "SystemAssigned" },
                new() { SourceResourceName = "wa-2", SourceResourceType = "Microsoft.Web/sites",
                        ManagedIdentityType = "UserAssigned", UserAssignedIdentityName = "uai-shared" },
            ],
        },
    };

    new IdentityAnalysisStage().Execute(ctx);

    ctx.MixedIdentityArmTypes.Should().Contain("Microsoft.Web/sites");
}
```

---

## 4. Avantages

| Bénéfice | Impact |
|---|---|
| **`BicepGenerationEngine` passe de 920 → ~80 lignes** | P2 résolu |
| Chaque stage est **testable unitairement** avec un `GenerationContext` minimal | Réduit la dépendance aux golden tests |
| **Ordre des transformations explicite** dans la composition DI | Pas d'ordre caché dans une méthode de 700 lignes |
| **Aucune réécriture des regex** → parité byte-à-byte garantie | Risque de régression nul si les stages reproduisent l'ordre actuel |
| **Composition par injection** → un stage peut être désactivé / mocké en test | Permet la TDD sur chaque transformation |
| Magic strings ARM peuvent migrer vers `AzureResourceTypes.ComputeArmTypes` au passage | P3 résolu |
| **Réutilisable pour `PipelineGenerationEngine`** (même structure de stages) | Bénéfice cross-cutting |

---

## 5. Inconvénients

| Risque | Sévérité | Mitigation |
|---|---|---|
| **Le contexte mutable est un anti-pattern fonctionnel** : facile d'ajouter des champs « parce qu'on en a besoin », ce qui recrée un God Object distribué | 🟠 Moyen | Discipliner le contexte par un découpage en sous-records (`IdentityState`, `AppSettingsState`) plutôt qu'un sac plat |
| **L'ordre implicite reste sensible** : un dev peut casser un stage en supposant qu'un autre s'est exécuté avant | 🟠 Moyen | Documenter les pré/post-conditions dans le XML doc de chaque stage. Ajouter des assertions `Debug.Assert(ctx.SystemIdentityResources is not null)` en début de stage |
| **Ne résout PAS le problème racine** : on garde la manipulation de texte par regex, juste répartie | 🔴 Important | C'est le but de la Vague 2 (Builder + IR). La Pipeline est une étape **intermédiaire**, pas une fin |
| **Plus de fichiers à naviguer** (8 stages + interface + contexte) | 🟢 Faible | Convention : tous les stages dans `BicepGeneration/Pipeline/Stages/` |
| **Risque de fuites d'abstraction** : si un stage doit savoir qu'un autre a déjà tourné, signe que le découpage est mauvais | 🟠 Moyen | Refuser tout stage qui lit un champ qu'il a lui-même écrit (= ajouter un état au contexte au lieu d'un effet de bord) |
| **MonoRepo path** complique la pipeline : `GenerateMonoRepo` boucle sur `GenerateCore` puis appelle `MonoRepoBicepAssembler.Assemble`. Il faut décider si la pipeline traite un seul `GenerationContext` ou un `MonoRepoContext` qui en agrège plusieurs | 🟠 Moyen | Soit garder `GenerateMonoRepo` à l'extérieur (boucle sur le moteur classique), soit ajouter un `MonoRepoAssemblyStage` final qui ne s'exécute que si `Request.IsMonoRepo` |

---

## 6. Coût de migration

| Tâche | Charge estimée |
|---|---|
| Définir `GenerationContext`, `IBicepGenerationStage` | S |
| Extraire les 8 stages | M |
| Brancher la DI + supprimer `BicepGenerationEngine` ancien | S |
| Migrer `BicepGenerationEngine.MonoRepo` | S |
| Couvrir chaque stage par 2-3 tests unitaires | M |
| **Total** | **1 sprint** (~5j ingé) |

**Risque de régression :** très faible si les méthodes `Inject*` sont déplacées par copie/colle sans réécriture, et si les golden tests passent intacts à chaque étape.

---

## 7. Quand ce pattern est suffisant à lui seul

Si l'équipe estime que :
- la manipulation par regex est **tolérable** (les golden tests rattrapent les régressions)
- on ne prévoit **pas** de backend d'émission alternatif (Terraform, ARM JSON)
- l'investissement Builder+IR (Vague 2) n'est pas prioritaire face à d'autres features

→ alors la Pipeline seule est un **point d'arrivée acceptable**. Elle apporte ~60 % de la valeur d'une refonte complète pour ~20 % du coût.
