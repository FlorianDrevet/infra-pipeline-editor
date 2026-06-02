---
name: dotnet-upgrade
description: "Use when: .NET version upgrade, SDK/TFM bump, NuGet package upgrade, EF Core / ASP.NET Core / Aspire migration, breaking changes detection, release notes analysis, new feature proposals."
---

# Skill: dotnet-upgrade — Migration .NET SDK, TFM, NuGet, EF Core, ASP.NET

> **Quand charger :** dès qu'une tâche concerne une montée de version .NET
> (SDK, TargetFramework, packages NuGet, EF Core, ASP.NET Core, ou composants Aspire).

---

## 1. Inventaire de l'état actuel

### Fichiers à inspecter

| Fichier | Information |
|---------|-------------|
| `global.json` | Version SDK .NET |
| `Directory.Build.props` | TargetFramework commun |
| `Directory.Packages.props` | Versions centralisées NuGet (Central Package Management) |
| `*.csproj` individuels | Overrides de TFM, PackageReference locales |
| `NuGet.config` | Sources NuGet, feeds privés |

### Commandes de diagnostic

```powershell
# Version SDK installée
dotnet --version

# Packages outdated (solution entière)
dotnet list .\InfraFlowSculptor.slnx package --outdated

# Packages vulnérables
dotnet list .\InfraFlowSculptor.slnx package --vulnerable

# Packages deprecated
dotnet list .\InfraFlowSculptor.slnx package --deprecated
```

---

## 2. Sources de release notes et breaking changes

### URLs de référence par composant

| Composant | URL pattern |
|-----------|-------------|
| .NET SDK | `https://learn.microsoft.com/dotnet/core/whats-new/dotnet-{major}` |
| ASP.NET Core migration | `https://learn.microsoft.com/aspnet/core/migration/{from}0-to-{to}0` |
| EF Core breaking changes | `https://learn.microsoft.com/ef/core/what-is-new/ef-core-{major}.0/breaking-changes` |
| EF Core what's new | `https://learn.microsoft.com/ef/core/what-is-new/ef-core-{major}.0/whatsnew` |
| .NET breaking changes (runtime) | `https://learn.microsoft.com/dotnet/core/compatibility/{major}.0` |
| Aspire release notes | `https://learn.microsoft.com/dotnet/aspire/whats-new/` |
| NuGet package (GitHub releases) | Dépend du package — chercher sur GitHub |

### Ce qu'il faut extraire

Pour chaque source, identifier et classer :

1. **Breaking changes** — API renommée, supprimée, comportement modifié
2. **Deprecated APIs** — marquées `[Obsolete]`, suppression planifiée en N+1
3. **Nouvelles APIs** — qui remplacent un pattern existant dans le projet
4. **Améliorations de performance** — applicables à notre code
5. **Nouvelles fonctionnalités** — qui apportent de la valeur au projet

---

## 3. Procédure de migration SDK + TFM

### Étape 1 — Modifier `global.json`

```json
{
  "sdk": {
    "version": "{NEW_VERSION}",
    "rollForward": "latestMinor"
  }
}
```

### Étape 2 — Modifier le TFM dans `Directory.Build.props`

```xml
<TargetFramework>net{MAJOR}.0</TargetFramework>
```

### Étape 3 — Restaurer et compiler

```powershell
dotnet restore .\InfraFlowSculptor.slnx
dotnet build .\InfraFlowSculptor.slnx
```

### Étape 4 — Corriger les erreurs de compilation (breaking changes)

Traiter chaque erreur comme un breaking change à résoudre avec la doc officielle.

---

## 4. Procédure de migration NuGet packages

### Stratégie Central Package Management

Ce projet utilise `Directory.Packages.props` pour centraliser les versions.
Toute mise à jour de version NuGet se fait **uniquement** dans ce fichier.

### Groupes de mise à jour (ordre de priorité)

1. **Packages Microsoft.Extensions.*** — en premier (dépendances transversales)
2. **ASP.NET Core packages** — après les extensions
3. **EF Core packages** — ensemble (Microsoft.EntityFrameworkCore.*)
4. **MediatR, FluentValidation, Mapster** — middleware applicatif
5. **Packages utilitaires** — ErrorOr, Refit, etc.
6. **Packages de test** — xUnit, NSubstitute, FluentAssertions, Verify
7. **Aspire packages** — dernier (dépend de tout le reste)

### Règles de mise à jour

- **Major version** : lire les breaking changes AVANT de mettre à jour
- **Minor version** : mettre à jour, build, test
- **Patch version** : mettre à jour en lot, build, test
- **Preview packages** : ne pas migrer sauf demande explicite de l'utilisateur
- **Ne jamais sauter 2 majors** : si un package est en retard de 2+, faire la migration en 2 étapes

### Validation après chaque groupe

```powershell
dotnet restore .\InfraFlowSculptor.slnx
dotnet build .\InfraFlowSculptor.slnx
dotnet test .\InfraFlowSculptor.slnx
```

---

## 5. Patterns de breaking changes courants (.NET)

### 5.1 APIs supprimées ou renommées

```csharp
// AVANT (deprecated)
services.AddIdentity<TUser, TRole>();

// APRÈS (migration)
services.AddIdentityCore<TUser>();
services.AddIdentityApiEndpoints<TUser>();
```

**Stratégie** : rechercher dans le codebase avec `grep_search`, remplacer selon la doc officielle.

### 5.2 Changements de comportement par défaut

Exemples typiques :
- Sérialiseur JSON qui change de casse par défaut
- Middleware ordering qui change
- Validation qui devient plus stricte
- Default DI lifetime qui change

**Stratégie** : identifier via les tests qui échouent, corriger avec configuration explicite.

### 5.3 Packages renommés ou fusionnés

```xml
<!-- AVANT -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />

<!-- APRÈS (si fusionné dans le framework) -->
<!-- Supprimé — inclus dans le framework -->
```

**Stratégie** : `dotnet list package --outdated` signale les packages sans nouvelle version.

### 5.4 Changements EF Core

Patterns fréquents :
- Conventions de nommage qui changent
- `HasConversion<>()` API qui évolue
- `OnModelCreating` hooks ajoutés/modifiés
- Migrations qui nécessitent une régénération

**Stratégie** : après upgrade, vérifier que le modèle EF Core n'a pas de diff inattendu :
```powershell
dotnet ef migrations has-pending-model-changes --project src\Api\InfraFlowSculptor.Infrastructure
```

---

## 6. Propositions de nouvelles fonctionnalités

Après correction de tous les breaking changes, proposer les nouveautés applicables.

### Format de proposition

```markdown
## Proposition : [Nom de la feature]

**Source :** [lien release notes]
**Applicable à :** [fichiers/modules du projet]
**Bénéfice :** performance / sécurité / maintenabilité / DX
**Effort estimé :** faible / moyen / élevé
**Risque :** aucun / faible / moyen

### Avant
[code actuel]

### Après
[code proposé]

### Impact
[ce qui change concrètement]
```

### Catégories de propositions par release .NET

| Catégorie | Exemples |
|-----------|----------|
| Performance | `FrozenDictionary`, `SearchValues`, LINQ optimisations |
| Sécurité | Nouveaux middleware auth, rate limiting |
| Simplicité | Primary constructors, collection expressions |
| Observabilité | OpenTelemetry intégré, métriques native |
| Aspire | Nouveaux composants, dashboard features |
| EF Core | Bulk operations, compiled queries améliorées |

---

## 7. Pièges connus spécifiques au projet

### Central Package Management

- Ne **jamais** ajouter un `<Version>` dans un `.csproj` individuel si le package est dans `Directory.Packages.props`
- Si un package nécessite une version différente par projet, utiliser `<PackageVersion Update="..." VersionOverride="..." />`

### Aspire packages

- Les versions Aspire doivent être alignées avec le SDK Aspire (`Aspire.AppHost.Sdk` dans le `.csproj` AppHost)
- Vérifier la matrice de compatibilité Aspire ↔ .NET

### EF Core + PostgreSQL

- `Npgsql.EntityFrameworkCore.PostgreSQL` a sa propre cadence de release
- Toujours vérifier la compatibilité Npgsql ↔ EF Core version

### Refit

- Refit a des breaking changes fréquents sur les interfaces générées
- Tester les clients HTTP après upgrade

---

## 8. Checklist de validation post-migration

```
[ ] global.json mis à jour
[ ] Directory.Build.props TFM mis à jour
[ ] Directory.Packages.props versions mises à jour
[ ] dotnet restore — succès
[ ] dotnet build — 0 erreurs
[ ] dotnet test — tous les tests passent
[ ] Aucun nouveau warning [Obsolete] non traité
[ ] Aucune vulnérabilité connue (dotnet list package --vulnerable)
[ ] EF Core model coherent (pas de pending model changes non planifiées)
[ ] Aspire AppHost démarre correctement
[ ] Mémoire projet mise à jour (.claude/memory/)
```
