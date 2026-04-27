# Pattern 3 — Visitor (sur AST/IR)

> **Verdict :** ⚠️ **Pertinent UNIQUEMENT si Builder + IR est adopté**. Sans IR, le Visitor n'a rien à visiter (pas de structure d'objet à parcourir). C'est la suite naturelle de la Vague 2, pas un point d'entrée.

---

## 1. Pourquoi tu y pensais (et pourquoi ce n'est pas suffisant seul)

L'intuition est correcte : la génération Bicep est conceptuellement une opération sur une structure arborescente (modules → params + ressources + outputs). Le Visitor permet de **séparer la structure de données des opérations** qui s'appliquent dessus, exactement comme Roslyn fait pour C#.

**Mais :** aujourd'hui, la « structure » est un `string`. Visitor un string n'a aucun sens — c'est ce que les regex tentent de faire, en pire.

→ Le Visitor n'est utile QUE sur l'IR de la [Vague 2](02-pattern-builder-ir.md).

---

## 2. Ce que Visitor apporte SUR un IR existant

Une fois l'IR en place, beaucoup d'opérations doivent parcourir le graphe (`BicepModuleSpec → BicepResourceDeclaration → BicepObjectExpression → BicepExpression`) :

| Opération | Aujourd'hui | Avec Visitor |
|---|---|---|
| Émission Bicep | `if/switch` sur le type d'expression dans l'Emitter | `BicepEmitterVisitor : BicepExpressionVisitor<StringBuilder>` |
| Émission ARM JSON | n'existe pas | `BicepArmJsonVisitor : BicepExpressionVisitor<JsonNode>` |
| Validation des références | regex `ExtractRootSymbol` | `ReferenceValidationVisitor` qui collecte les `BicepReference` non résolues |
| Pruning des outputs inutilisés | parsing texte ligne par ligne (`PruneUnusedOutputs`) | `OutputUsageVisitor` qui marque les outputs cités dans d'autres specs |
| Naming functions effectivement utilisées | regex sur le `StringBuilder` final | `UsedFunctionsVisitor` |
| Calcul des imports nécessaires | calculé à la main dans `MainBicepAssembler` | `RequiredImportsVisitor` |

Chaque opération devient un **fichier dédié**, pure, testable.

---

## 3. Implémentation

### 3.1 — Visitor de base (typage par retour)

```csharp
namespace InfraFlowSculptor.BicepGeneration.Ir.Visitors;

/// <summary>Visitor générique qui retourne un résultat de type T pour chaque nœud d'expression.</summary>
public abstract class BicepExpressionVisitor<T>
{
    public T Visit(BicepExpression node) => node switch
    {
        BicepLiteral literal       => VisitLiteral(literal),
        BicepReference reference   => VisitReference(reference),
        BicepFunctionCall call     => VisitFunctionCall(call),
        BicepObjectExpression obj  => VisitObject(obj),
        BicepArrayExpression arr   => VisitArray(arr),
        _ => throw new NotSupportedException($"Unhandled expression type: {node.GetType().Name}")
    };

    protected abstract T VisitLiteral(BicepLiteral node);
    protected abstract T VisitReference(BicepReference node);
    protected abstract T VisitFunctionCall(BicepFunctionCall node);
    protected abstract T VisitObject(BicepObjectExpression node);
    protected abstract T VisitArray(BicepArrayExpression node);
}
```

### 3.2 — Visitor d'émission Bicep

```csharp
public sealed class BicepEmitterVisitor : BicepExpressionVisitor<string>
{
    protected override string VisitLiteral(BicepLiteral node) => node.Value switch
    {
        string s   => $"'{EscapeBicepString(s)}'",
        bool b     => b ? "true" : "false",
        int or long or double => node.Value.ToString()!,
        null       => "null",
        _ => throw new NotSupportedException($"Cannot emit literal of type {node.Value.GetType()}")
    };

    protected override string VisitReference(BicepReference node) => node.Symbol;

    protected override string VisitFunctionCall(BicepFunctionCall node)
    {
        var args = string.Join(", ", node.Args.Select(Visit));
        return $"{node.Name}({args})";
    }

    protected override string VisitObject(BicepObjectExpression node)
    {
        var sb = new StringBuilder("{\n");
        foreach (var (key, value) in node.Properties)
            sb.Append("  ").Append(key).Append(": ").Append(Visit(value)).AppendLine();
        sb.Append("}");
        return sb.ToString();
    }

    protected override string VisitArray(BicepArrayExpression node)
    {
        var items = string.Join(", ", node.Items.Select(Visit));
        return $"[{items}]";
    }
}
```

### 3.3 — Visitor de collecte (read-only)

```csharp
/// <summary>Collecte tous les <see cref="BicepReference"/> dans une expression — utile pour la validation.</summary>
public sealed class ReferenceCollectorVisitor : BicepExpressionVisitor<IEnumerable<string>>
{
    protected override IEnumerable<string> VisitLiteral(BicepLiteral node) => [];
    protected override IEnumerable<string> VisitReference(BicepReference node) => [node.Symbol];
    protected override IEnumerable<string> VisitFunctionCall(BicepFunctionCall node)
        => node.Args.SelectMany(Visit);
    protected override IEnumerable<string> VisitObject(BicepObjectExpression node)
        => node.Properties.Values.SelectMany(Visit);
    protected override IEnumerable<string> VisitArray(BicepArrayExpression node)
        => node.Items.SelectMany(Visit);
}
```

### 3.4 — Visitor de réécriture (pour transformations)

```csharp
/// <summary>Visitor qui produit une nouvelle expression à partir de chaque nœud (Roslyn-style rewriter).</summary>
public abstract class BicepExpressionRewriter : BicepExpressionVisitor<BicepExpression>
{
    protected override BicepExpression VisitLiteral(BicepLiteral node) => node;
    protected override BicepExpression VisitReference(BicepReference node) => node;

    protected override BicepExpression VisitFunctionCall(BicepFunctionCall node)
    {
        var newArgs = node.Args.Select(Visit).ToImmutableList();
        return newArgs.SequenceEqual(node.Args) ? node : node with { Args = newArgs };
    }

    protected override BicepExpression VisitObject(BicepObjectExpression node)
    {
        var newProps = node.Properties.ToImmutableDictionary(kv => kv.Key, kv => Visit(kv.Value));
        return new BicepObjectExpression(newProps);
    }

    protected override BicepExpression VisitArray(BicepArrayExpression node)
        => new BicepArrayExpression(node.Items.Select(Visit).ToImmutableList());
}
```

Exemple d'usage : remplacer toutes les références à `tenant().tenantId` par un param `tenantId` :

```csharp
public sealed class TenantIdInliner : BicepExpressionRewriter
{
    protected override BicepExpression VisitFunctionCall(BicepFunctionCall node)
    {
        if (node.Name == "tenant" && node.Args.Count == 0)
            return new BicepReference("tenantId");
        return base.VisitFunctionCall(node);
    }
}
```

### 3.5 — Visitor au niveau Module (pas seulement Expression)

```csharp
public abstract class BicepModuleVisitor<T>
{
    public abstract T VisitModule(BicepModuleSpec module);

    protected virtual T VisitParam(BicepParam param) => default!;
    protected virtual T VisitOutput(BicepOutput output) => default!;
    protected virtual T VisitResource(BicepResourceDeclaration resource) => default!;
}

/// <summary>Calcule la liste des fonctions de naming nécessaires en analysant tous les modules.</summary>
public sealed class RequiredNamingFunctionsVisitor : BicepModuleVisitor<HashSet<string>>
{
    private readonly HashSet<string> _functions = new(StringComparer.OrdinalIgnoreCase);
    private readonly ReferenceCollectorVisitor _refCollector = new();

    public override HashSet<string> VisitModule(BicepModuleSpec module)
    {
        // Visiter toutes les expressions du module
        foreach (var (_, expr) in module.Resource.Properties.Properties)
            foreach (var symbol in _refCollector.Visit(expr))
                if (symbol.StartsWith("Build") && symbol.EndsWith("Name"))
                    _functions.Add(symbol);

        return _functions;
    }
}
```

---

## 4. Avantages

| Bénéfice | Impact |
|---|---|
| **Sépare structure et opérations** : ajouter un nouveau « passage » sur l'IR = nouveau Visitor, sans toucher aux records IR | Élevé |
| **Permet plusieurs émetteurs** : Bicep, ARM JSON, Terraform, JSON Schema (pour le frontend) — chacun est un Visitor | Élevé (open door) |
| **Réécriture safe** : `BicepExpressionRewriter` garantit qu'on n'oublie aucun cas de l'`enum` d'expressions | Moyen |
| **Visitors composables** : un `ValidationVisitor` peut être enchaîné après un `OptimizationVisitor` | Moyen |
| **Familier** : pattern Roslyn — la stack .NET tout entière l'utilise | Crédibilité |
| **Test-friendly** : chaque Visitor est une classe pure, testée avec un IR fixture minimal | Élevé |

---

## 5. Inconvénients

| Risque | Sévérité | Mitigation |
|---|---|---|
| **Inutile sans IR** : prérequis impératif Vague 2. Coût marginal élevé si on ne va pas jusqu'au bout | 🔴 Critique | Ne pas adopter Visitor sans avoir validé Builder + IR |
| **Boilerplate** : chaque opération demande un fichier Visitor. Pour 1-2 opérations, c'est sur-ingénierie | 🟠 Moyen | Adopter Visitor seulement à partir de **3 opérations distinctes** sur l'IR. En dessous, des méthodes d'extension `BicepModuleSpec.WithIdentity(...)` suffisent |
| **Visitor à retour `T` rigide** : si une opération a besoin de plusieurs résultats hétérogènes, T devient un tuple ou un record dédié | 🟢 Faible | Utiliser `void` Visitor avec accumulateur interne dans la classe (pattern Roslyn) |
| **Double Dispatch caché derrière `switch` C#** : pas du Visitor « pur » à la GoF, on s'appuie sur le pattern matching. Si demain on ajoute un type d'expression, le compilateur ne crashe pas — il faut un test ou un `_ => throw` | 🟠 Moyen | Utiliser `[GenerateRewriter]` source generator OU préférer une classe `sealed` pour `BicepExpression` avec abstract method `Accept<T>(IBicepVisitor<T> visitor)` (vrai Visitor GoF) |
| **Performance** : un visitor qui réécrit clone toutes les structures immuables qu'il traverse. Pour des modules petits (~50 nœuds) c'est négligeable, mais à grande échelle pas idéal | 🟢 Faible | Mesurer si jamais. Records `with` est très optimisé par le runtime |
| **Confusion Pipeline vs Visitor** : Pipeline = orchestration de stages métier ; Visitor = parcours d'arbre. Risque de mélanger les deux | 🟢 Faible | Convention : Pipeline opère au niveau `GenerationContext`, Visitor opère au niveau `BicepModuleSpec` ou `BicepExpression` |

---

## 6. Coût additionnel (en plus de Vague 2)

| Tâche | Charge |
|---|---|
| Définir `BicepExpressionVisitor<T>` + `BicepExpressionRewriter` | S |
| Refactor de l'`Emitter` en `BicepEmitterVisitor` | S |
| Premier visitor utile (`ReferenceCollectorVisitor` pour la validation) | S |
| Visitor de pruning d'outputs (remplace `BicepAssembler.PruneUnusedOutputs`) | M |
| **Total** | **3-5j ingé** au-dessus de Vague 2 |

---

## 7. Décision pratique

| Si vous adoptez | Pertinence Visitor |
|---|---|
| Pipeline seule (Vague 1) | ❌ Pas de Visitor, pas d'IR à visiter |
| Pipeline + Builder/IR (Vague 1 + 2) | ⚠️ Adopter Visitor **seulement** quand 3+ opérations sur l'IR sont nécessaires |
| Multi-backends (Bicep + ARM JSON + Terraform) | ✅ Visitor **incontournable** : un Visitor par backend |

**Ma recommandation :** ne PAS chercher à coder du Visitor avant que la Vague 2 soit en production et que **deux** opérations distinctes (Emit + Validation, par exemple) traversent déjà l'IR avec des `switch` répétés. À ce moment-là, le besoin de Visitor sera évident et la refonte triviale.

---

## 8. Synthèse pour le choix final

| Critère | Pipeline seul | Pipeline + Builder/IR | Pipeline + Builder/IR + Visitor |
|---|---|---|---|
| Coût | ⭐ Faible | ⭐⭐ Moyen | ⭐⭐⭐ Élevé |
| Fragilité regex éliminée | ❌ | ✅ | ✅ |
| God Object éclaté | ✅ | ✅ | ✅ |
| Templates `const string` éliminés | ❌ | ✅ | ✅ |
| Multi-backend ouvert | ❌ | 🟡 Possible | ✅ Naturel |
| Recommandation | Si budget serré | **Cible normale** | Si vision multi-IaC à 1-2 ans |
