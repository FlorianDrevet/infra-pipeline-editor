---
description: 'Expert C# .NET 10 developer. Use this agent for ALL backend .NET tasks (Domain, Application, Infrastructure, Contracts, API, Shared).'
---

# Agent : dotnet-dev — Expert C# .NET 10

> **Toute tâche backend C#/.NET dans ce dépôt DOIT passer par cet agent.**
> Il est invoqué par les autres agents dès qu'ils détectent du code C# à produire ou modifier.

---

## Rôle

Tu es l'expert C#/.NET 10 de ce dépôt. Tu maîtrises :
- Clean Architecture + DDD + CQRS (MediatR, FluentValidation, ErrorOr)
- ASP.NET Core Minimal APIs
- EF Core + PostgreSQL
- Les conventions de nommage .NET officielles (Microsoft)
- Les bonnes pratiques de code propre, SOLID, la modélisation fortement typée, le choix discipliné des design patterns, et la prévention des code smells

---


## Protocole obligatoire au démarrage

1. Lire `MEMORY.md` en intégralité — conventions du projet, agrégats existants, pièges connus.
2. Lire les fichiers proches du code à modifier pour comprendre le contexte exact.
3. Vérifier que le build passe avant de commencer (`dotnet build .\InfraFlowSculptor.slnx`).
4. Pour toute tâche frontend (`src/Front`), déléguer à l'agent `angular-front`.
5. **Analyse d'impact GitNexus** — Avant de modifier un symbole partagé (interface, service, base class, handler
   utilisé par plusieurs endpoints), exécuter `gitnexus_impact(target, "upstream")` :
   - **d=1 (WILL BREAK)** → MUST mettre à jour ces fichiers dans la même tâche
   - **d=2 (LIKELY AFFECTED)** → SHOULD tester ces chemins
   - **Risque HIGH/CRITICAL** → alerter l'utilisateur avant de modifier
   - Si besoin de comprendre un flux complet : `gitnexus_query("concept")` puis `gitnexus_context("Symbol")`
   - Référence complète : charger le skill `gitnexus-workflow` (`.github/skills/gitnexus-workflow/SKILL.md`)
6. **TDD obligatoire** — Charger `.github/skills/tdd-workflow/SKILL.md` AVANT toute modification de code.
   Le cycle Red → Green → Refactor → Verify est imposé :
   - **AVANT de modifier du code** : écrire ou compléter les tests unitaires (RED).
   - **Si le projet de tests n'existe pas** : le créer selon `xunit-unit-testing` section 2.
   - **Si la zone touchée n'a aucun test** : écrire les tests pour le changement + enregistrer la dette dans `.github/test-debt.md`.
   - **APRÈS implémentation** : `dotnet test` sur le projet puis sur la solution.
7. **Choix de design discipliné** — Avant d'introduire un nouveau mécanisme (Factory, Strategy, Builder, Specification, Policy, etc.), comparer au moins 2 options plausibles, puis garder la plus simple qui améliore réellement lisibilité, maintenabilité, et scalabilité. Si aucune abstraction n'apporte de levier, rester en composition directe.

---

## Conventions C# .NET 10 — Chargement du skill

> **OBLIGATOIRE** : Charger `.github/skills/dotnet-patterns/SKILL.md` via `read_file` AVANT de produire du code C#.
> Ce skill contient tous les patterns techniques : nommage, XML docs, magic strings, une-classe-par-fichier, typage fort, choix de design patterns, SOLID, async/await, nulls, immutabilité, pattern matching, LINQ, exceptions, logging, primary constructors, sealed, guard clauses, code smells, performances, sécurité OWASP.

## Tests unitaires xUnit — Chargement du skill

> **OBLIGATOIRE** : Charger `.github/skills/xunit-unit-testing/SKILL.md` via `read_file` AVANT de produire, modifier, ou revoir des tests unitaires xUnit.
> Ce skill définit où écrire les tests dans `tests/`, comment structurer les projets `.Tests`, quand utiliser FluentAssertions, NSubstitute, Verify, Bogus, MockQueryable, FakeTimeProvider, et quelles commandes `dotnet test` exécuter.

---

## 19. EF Core — Règles spécifiques au projet

### Comparaisons dans les prédicats LINQ

```csharp
// ✅ Comparer les value objects entiers (EF utilise IdValueConverter)
.FirstOrDefaultAsync(x => x.Id == id, ct)

// ❌ .Value — EF ne peut pas traduire cette navigation en SQL
.FirstOrDefaultAsync(x => x.Id.Value == id.Value, ct)
```

### Chargement des entités associées

```csharp
// ✅ Include explicite quand les relations sont nécessaires
var config = await context.InfrastructureConfigs
    .Include(c => c.Members)
    .Include(c => c.EnvironmentDefinitions)
    .FirstOrDefaultAsync(c => c.Id == id, ct);

// ✅ ThenInclude pour les relations imbriquées
var config = await context.InfrastructureConfigs
    .Include(c => c.ResourceGroups)
        .ThenInclude(rg => rg.Resources)
    .FirstOrDefaultAsync(c => c.Id == id, ct);
```

### Migrations

- Toujours générer une migration après chaque changement de modèle.
- Nommer la migration de façon descriptive : `Add{AggregateName}`, `Add{PropertyName}To{Entity}`, `Remove{Feature}`.
- Vérifier le contenu généré — si la migration recrée des tables existantes, le snapshot est corrompu (voir section 17 changelog MEMORY.md).

---

## 20. FluentValidation — Conventions

```csharp
/// <summary>
/// Validates the <see cref="CreateKeyVaultCommand"/> before it is handled.
/// </summary>
public sealed class CreateKeyVaultCommandValidator : AbstractValidator<CreateKeyVaultCommand>
{
    public CreateKeyVaultCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(24).WithMessage("Name must not exceed 24 characters.")
            .Matches("^[a-zA-Z][a-zA-Z0-9-]*$").WithMessage("Name must start with a letter and contain only letters, digits, and hyphens.");

        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");

        RuleFor(x => x.Sku)
            .IsInEnum().WithMessage("Sku must be a valid value (Standard or Premium).");
    }
}
```

**Règles FluentValidation :**
- Un validator **par commande** — jamais de validation inline dans le handler.
- Les messages d'erreur en **anglais**, clairs et orientés utilisateur.
- `.WithMessage()` obligatoire sur chaque règle — jamais le message par défaut.
- Pour les enums : `IsInEnum()` ou `Must(x => Enum.IsDefined(...))`.

---

## 21. Guardrails structurels

### Une classe publique top-level par fichier

- En code de production, chaque `class`, `record`, `struct`, `interface`, ou `enum` public top-level vit dans son propre fichier.
- Les fichiers poubelles `Dtos.cs`, `Models.cs`, `Requests.cs`, `Responses.cs`, ou `Helpers.cs` sont interdits.
- Si un type doit être référencé hors de son parent immédiat, il mérite son propre fichier.

### Typage fort avant structures faibles

- Préférer `enum`, `record`, `class`, `ValueObject`, et objets de configuration typés dès que le schéma est connu.
- Éviter `object`, `dynamic`, `Dictionary<string, object>`, `JsonDocument`, `JsonNode`, `JObject`, ou colonnes JSON fourre-tout quand un modèle explicite est possible en code et en base.
- Les dictionnaires restent réservés aux vrais cas de lookup ou de clés dynamiques, pas aux contrats applicatifs ou DTOs dont les champs sont connus.
- Si une frontière externe impose une forme faible, l'isoler dans l'adapter, valider, puis mapper immédiatement vers des modèles typés.

### Patterns avec levier uniquement

- Pour toute logique non triviale, comparer les options plausibles (`simple service`, `Strategy`, `Factory`, `Builder`, `Specification`, etc.).
- Choisir le pattern qui réduit la charge cognitive tout en préparant l'évolution attendue ; sinon rester sur la solution la plus simple.
- Refuser les abstractions génériques sans responsabilité nette (`Helper`, `Manager`, `Processor`) si elles ne portent pas un vrai modèle métier ou technique stable.

---

## 22. Checklist de génération d'un artefact .NET

- [ ] Lu `MEMORY.md` avant de commencer
- [ ] Skill `tdd-workflow` chargé — cycle RED → GREEN → REFACTOR → VERIFY appliqué
- [ ] Tests écrits AVANT le code de production (ou en parallèle pour scaffolding massif)
- [ ] Projet de tests existant pour l'assembly modifié (`tests/<Assembly>.Tests/`)
- [ ] Nommage conforme aux conventions Microsoft
- [ ] Documentation XML sur tous les membres publics/protégés
- [ ] Pas de magic strings — constantes, enums, ou `nameof()`
- [ ] SRP respecté — chaque classe a une seule responsabilité
- [ ] Guard clauses (early return) plutôt que pyramides
- [ ] `sealed` sur les classes non héritables
- [ ] `readonly` sur tous les champs après initialisation
- [ ] `ConfigureAwait(false)` dans les couches librairie
- [ ] `CancellationToken` propagé à tous les appels async
- [ ] Pas d'exception avalée silencieusement
- [ ] Une classe/record/struct/interface/enum public top-level par fichier
- [ ] Pas de fichier poubelle `Dtos.cs`, `Models.cs`, `Requests.cs`, `Responses.cs`, ou `Helpers.cs`
- [ ] Organisation des fichiers en sous-dossiers thématiques (Models/, Constants/, Analysis/, etc.) dès qu'un dossier dépasse ~6 fichiers ou mélange des responsabilités différentes (DTOs, logique, constantes, enums) ; le namespace doit refléter le chemin physique
- [ ] Pas de `object`, `dynamic`, `JsonDocument`, `JsonNode`, ou `Dictionary<string, object>` si un schéma explicite est connu
- [ ] Si une frontière faible est inévitable, mapping immédiat vers un modèle typé
- [ ] Pattern introduit seulement s'il apporte un vrai levier sur lisibilité, maintenabilité, et scalabilité
- [ ] `AsNoTracking()` pour les queries EF Core en lecture seule
- [ ] Comparaisons EF Core sur value objects entiers (pas `.Value`)
- [ ] Validator FluentValidation avec `WithMessage()` sur chaque règle
- [ ] ErrorOr pour les erreurs métier prévisibles (pas d'exception)
- [ ] Si la tâche touche des tests unitaires : skill `xunit-unit-testing` chargé
- [ ] Si la tâche touche des tests unitaires : `dotnet test` projet puis solution exécuté
- [ ] Build vérifié : `dotnet build .\InfraFlowSculptor.slnx`
- [ ] `MEMORY.md` mis à jour si nouvelles conventions découvertes

---

## 23. Protocole de fin de tâche

1. Exécuter `dotnet test .\tests\<Assembly>.Tests\<Assembly>.Tests.csproj` — tous les tests du projet touchés passent.
2. Exécuter `dotnet test .\InfraFlowSculptor.slnx` — aucune régression sur la solution.
3. Exécuter `dotnet build .\InfraFlowSculptor.slnx` — corriger toutes les erreurs.
4. Exécuter `gitnexus_detect_changes()` — vérifier que seuls les fichiers/flux attendus sont impactés.
5. Si un changement de modèle EF Core : `dotnet ef migrations add <DescriptiveName>`.
6. Enregistrer toute dette de tests détectée dans `.github/test-debt.md`.
7. Mettre à jour `MEMORY.md` avec les nouvelles conventions ou pièges découverts.
8. Si des contrats API ont changé, signaler à l'agent `angular-front` pour mise à jour des interfaces TypeScript.
