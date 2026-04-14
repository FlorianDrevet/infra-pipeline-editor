#!/bin/bash
# =============================================================================
# InfraFlowSculptor — Create GitHub Issues from Audit (April 2026)
# Run: chmod +x scripts/create-audit-issues.sh && ./scripts/create-audit-issues.sh
# Requires: gh CLI authenticated (gh auth login)
# Pre-requisite: Run create-audit-labels.sh first
# =============================================================================

set -euo pipefail

REPO="FlorianDrevet/infra-pipeline-editor"
AUDIT_LABEL="audit: 2026-04"

echo "📋 Creating audit issues for $REPO..."
echo "================================================"

# ---------------------------------------------------------------------------
# SEC-001 — Absence totale de tests unitaires et d'intégration
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[SEC-001] Mettre en place l'infrastructure de tests (unit + intégration)" \
  --label "severity: critical,$AUDIT_LABEL,area: testing,phase: 0-foundation,type: tech-debt" \
  --body '## Contexte (Audit SEC-001)

**Sévérité :** 🔴 CRITIQUE
**Impact :** Toute la solution — 0% de couverture de tests

### Constat

Aucun projet de test n'\''existe dans le repository. Il n'\''y a :
- Zéro test unitaire
- Zéro test d'\''intégration  
- Zéro test de non-régression

Chaque modification de code est un risque de régression non détecté. Les refactorisations futures du plan d'\''audit sont dangereuses sans filet de sécurité.

### Travail à réaliser

#### 1. Créer les projets de test
```
tests/
├── InfraFlowSculptor.Domain.UnitTests/
├── InfraFlowSculptor.Application.UnitTests/
├── InfraFlowSculptor.Infrastructure.IntegrationTests/
└── InfraFlowSculptor.Api.IntegrationTests/
```

#### 2. Configurer les dépendances
- **xUnit** (framework de test)
- **FluentAssertions** (assertions lisibles)
- **NSubstitute** (mocks)
- **Bogus** (génération de données de test)
- **Testcontainers** (PostgreSQL pour les tests d'\''intégration)
- **Microsoft.AspNetCore.Mvc.Testing** (WebApplicationFactory pour les tests API)

#### 3. Écrire les tests prioritaires

**Priorité 1 — Value Objects (Domain.UnitTests)**
- `Name` : construction, validation chaîne vide, equality
- `Location` : tous les enum members, ToString
- `SingleValueObject<T>` : immutabilité, equality
- Tous les `EnumValueObject`-derived

**Priorité 2 — Handlers critiques (Application.UnitTests)**
- Handlers de génération Bicep/Pipeline
- Handlers d'\''autorisation (`IInfraConfigAccessService`)
- `ValidationBehavior` pipeline

**Priorité 3 — Repositories (Infrastructure.IntegrationTests)**
- CRUD basique avec PostgreSQL Testcontainers
- Vérification des cascades delete
- Vérification des contraintes uniques

### Critères d'\''acceptation

- [ ] 4 projets de test créés et référencés dans la solution `.slnx`
- [ ] `dotnet test` passe en CI (même avec 0 tests initiaux)
- [ ] Au moins 20 tests unitaires sur les value objects du domaine
- [ ] Au moins 10 tests unitaires sur les handlers critiques
- [ ] Couverture mesurable via `dotnet test --collect:"XPlat Code Coverage"`
- [ ] README mis à jour avec les commandes de test

### Estimation
**Effort :** 10 jours (2 sprints)
**Risque si non traité :** Toutes les autres issues d'\''audit ne peuvent pas être validées sans tests

### Références
- [xUnit.net Getting Started](https://xunit.net/docs/getting-started/netcore/cmdline)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [WebApplicationFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
'

echo "✅ SEC-001 created"

# ---------------------------------------------------------------------------
# SEC-002 — Headers de sécurité HTTP manquants
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[SEC-002] Ajouter les headers de sécurité HTTP manquants" \
  --label "severity: critical,$AUDIT_LABEL,area: security,phase: 1-security,type: bug" \
  --body '## Contexte (Audit SEC-002)

**Sévérité :** 🔴 CRITIQUE
**Fichier :** `src/Api/InfraFlowSculptor.Api/Program.cs`

### Constat

Aucun header de sécurité HTTP n'\''est configuré dans le pipeline middleware. Les headers suivants sont absents :

| Header | Objectif | Risque sans lui |
|--------|----------|-----------------|
| `X-Frame-Options: DENY` | Anti-clickjacking | Embedding dans une iframe malveillante |
| `X-Content-Type-Options: nosniff` | Anti-MIME sniffing | Exécution de contenu malveillant |
| `Strict-Transport-Security` | Force HTTPS | Downgrade attacks (MitM) |
| `X-XSS-Protection: 1; mode=block` | Anti-XSS legacy | XSS sur navigateurs anciens |
| `Content-Security-Policy` | Contrôle sources | XSS, injection de scripts |
| `Referrer-Policy: strict-origin-when-cross-origin` | Contrôle referer | Fuite d'\''URLs internes |

### Travail à réaliser

1. Créer un middleware `SecurityHeadersMiddleware` dans `src/Api/InfraFlowSculptor.Api/Middleware/`
2. Ajouter tous les headers listés ci-dessus
3. Enregistrer le middleware dans `Program.cs` **avant** `UseCors()`

```csharp
// Fichier: src/Api/InfraFlowSculptor.Api/Middleware/SecurityHeadersMiddleware.cs
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Content-Security-Policy", "default-src '\''self'\''; frame-ancestors '\''none'\''");

        if (context.Request.IsHttps)
        {
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }

        await _next(context);
    }
}
```

### Critères d'\''acceptation

- [ ] Middleware créé et enregistré dans `Program.cs`
- [ ] Les 6 headers sont présents sur toutes les réponses HTTP
- [ ] Test avec `curl -I https://localhost:port/` confirme la présence des headers
- [ ] Aucune régression sur les endpoints existants (Scalar UI fonctionne toujours)

### Estimation
**Effort :** 0.5 jour
'

echo "✅ SEC-002 created"

# ---------------------------------------------------------------------------
# SEC-003 — Rate limiting incomplet
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[SEC-003] Étendre le rate limiting à tous les endpoints API" \
  --label "severity: critical,$AUDIT_LABEL,area: security,phase: 1-security,type: bug" \
  --body '## Contexte (Audit SEC-003)

**Sévérité :** 🔴 CRITIQUE
**Fichier :** `src/Api/InfraFlowSculptor.Api/RateLimiting/RateLimiting.cs`

### Constat

Le rate limiting est configuré **uniquement** pour l'\''endpoint "Login" (3 requêtes / 10 secondes). Les 150+ endpoints CRUD et les endpoints de génération Bicep/Pipeline n'\''ont **aucun** rate limiting.

### Travail à réaliser

1. **Ajouter 3 politiques de rate limiting** dans `RateLimiting.cs` :

| Politique | Limite | Fenêtre | Usage |
|-----------|--------|---------|-------|
| `Standard` | 100 requêtes | 1 minute | Endpoints CRUD standards |
| `Expensive` | 10 requêtes | 1 minute | Endpoints de génération Bicep/Pipeline |
| `Login` | 3 requêtes | 10 secondes | (déjà existant) |

2. **Appliquer les politiques** dans chaque contrôleur :
   - `.RequireRateLimiting("Standard")` sur tous les groupes CRUD
   - `.RequireRateLimiting("Expensive")` sur les endpoints `generate-bicep`, `generate-pipeline`, `download-bicep`, `download-pipeline`

3. **Ajouter un global fallback** pour les endpoints non couverts :
```csharp
options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 200,
            Window = TimeSpan.FromMinutes(1)
        }));
```

### Critères d'\''acceptation

- [ ] 3 politiques de rate limiting configurées
- [ ] Tous les groupes d'\''endpoints ont un rate limiting explicite
- [ ] Global fallback configuré
- [ ] Réponse `429 Too Many Requests` retournée quand la limite est dépassée
- [ ] Header `Retry-After` présent sur les réponses 429

### Estimation
**Effort :** 1 jour
'

echo "✅ SEC-003 created"

# ---------------------------------------------------------------------------
# SEC-004 — Path traversal vulnerability
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[SEC-004] Corriger la vulnérabilité de path traversal sur les endpoints fichiers" \
  --label "severity: critical,$AUDIT_LABEL,area: security,phase: 1-security,type: bug" \
  --body '## Contexte (Audit SEC-004)

**Sévérité :** 🔴 CRITIQUE
**Fichier :** `src/Api/InfraFlowSculptor.Api/Controllers/ProjectController.cs`

### Constat

Les endpoints suivants acceptent un paramètre `{*filePath}` (catch-all) **sans aucune validation** :
- `GET /{projectId}/generate-bicep/files/{*filePath}`
- `GET /{projectId}/generate-pipeline/files/{*filePath}`

Un attaquant peut envoyer `../../../etc/passwd` ou `..\..\web.config` pour accéder à des fichiers non autorisés.

### Travail à réaliser

1. **Créer un helper de validation** dans `src/Api/InfraFlowSculptor.Api/Common/` :

```csharp
public static class FilePathValidator
{
    public static bool IsValid(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return false;
        if (filePath.Contains("..")) return false;
        if (filePath.StartsWith('/') || filePath.StartsWith('\\')) return false;
        if (Path.IsPathRooted(filePath)) return false;
        
        // Seuls les caractères autorisés
        var invalidChars = Path.GetInvalidPathChars();
        return !filePath.Any(c => invalidChars.Contains(c));
    }
}
```

2. **Appliquer la validation** dans les endpoints concernés :
```csharp
if (!FilePathValidator.IsValid(filePath))
    return Results.BadRequest("Invalid file path");
```

3. **Ajouter des tests** pour les cas d'\''attaque :
   - `../../etc/passwd`
   - `..\..\..\windows\system32`
   - `%2e%2e%2f` (URL-encoded)
   - Chemins absolus (`/etc/passwd`, `C:\`)

### Critères d'\''acceptation

- [ ] Validation ajoutée sur les 2 endpoints fichiers (Bicep + Pipeline)
- [ ] Retour `400 Bad Request` pour les paths malveillants
- [ ] Tests unitaires couvrant au moins 5 cas d'\''attaque
- [ ] Les paths légitimes (`modules/main.bicep`, `pipelines/deploy.yml`) fonctionnent toujours

### Estimation
**Effort :** 0.5 jour
'

echo "✅ SEC-004 created"

# ---------------------------------------------------------------------------
# SEC-005 — CORS trop permissif
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[SEC-005] Restreindre la politique CORS (methods et headers explicites)" \
  --label "severity: high,$AUDIT_LABEL,area: security,phase: 1-security,type: bug" \
  --body '## Contexte (Audit SEC-005)

**Sévérité :** 🟠 HAUTE
**Fichier :** `src/Api/InfraFlowSculptor.Api/Program.cs:16-24`

### Constat

La politique CORS utilise `AllowAnyHeader()` et `AllowAnyMethod()`. Même limitée à `localhost:4200`, cela autorise tous les méthodes HTTP (incluant TRACE, OPTIONS arbitraires) et tous les headers custom.

### Travail à réaliser

Remplacer dans `Program.cs` :
```csharp
// AVANT (permissif)
policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200");

// APRÈS (restrictif)
policy
    .WithOrigins("http://localhost:4200")
    .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
    .WithHeaders("Content-Type", "Authorization", "Accept", "X-Requested-With")
    .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
```

Également : rendre les origines configurables via `appsettings.json` pour faciliter le déploiement en production.

### Critères d'\''acceptation

- [ ] Méthodes HTTP explicitement listées
- [ ] Headers explicitement listés
- [ ] Origines configurables via `appsettings.json`
- [ ] Frontend fonctionne toujours sans erreur CORS

### Estimation
**Effort :** 0.5 jour
'

echo "✅ SEC-005 created"

# ---------------------------------------------------------------------------
# SEC-006 — Limite taille requêtes
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[SEC-006] Configurer la limite de taille des requêtes HTTP (MaxRequestBodySize)" \
  --label "severity: high,$AUDIT_LABEL,area: security,phase: 1-security" \
  --body '## Contexte (Audit SEC-006)

**Sévérité :** 🟠 HAUTE  
**Fichier :** `src/Api/InfraFlowSculptor.Api/Program.cs`

### Constat
Aucune limite `MaxRequestBodySize` n'\''est configurée sur Kestrel. Un attaquant peut envoyer des payloads arbitrairement grands, provoquant un DoS par épuisement mémoire.

### Travail à réaliser
Ajouter dans `Program.cs` :
```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52_428_800; // 50 MB
});
```

### Critères d'\''acceptation
- [ ] Limite de taille configurée à 50 MB
- [ ] Réponse `413 Request Entity Too Large` si dépassée
- [ ] Les endpoints de génération fonctionnent toujours avec des configurations de taille normale

### Estimation
**Effort :** 0.5 jour
'

echo "✅ SEC-006 created"

# ---------------------------------------------------------------------------
# SEC-007 — Information disclosure in errors
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[SEC-007] Sécuriser la gestion des erreurs pour éviter la divulgation d'informations" \
  --label "severity: high,$AUDIT_LABEL,area: security,phase: 1-security,type: bug" \
  --body '## Contexte (Audit SEC-007)

**Sévérité :** 🟠 HAUTE  
**Fichier :** `src/Api/InfraFlowSculptor.Api/Errors/ErrorHandling.cs`

### Constat
En développement, `UseDeveloperExceptionPage()` est activé (correct). Mais en production, il n'\''y a pas de garantie stricte que les stack traces ne fuient pas.

### Travail à réaliser
1. Vérifier que `UseDeveloperExceptionPage()` est conditionnel (`IsDevelopment()` uniquement)
2. Ajouter un middleware global d'\''exception qui retourne un `ProblemDetails` générique en production
3. Logger les exceptions complètes côté serveur (structured logging)
4. Ne jamais exposer de message d'\''exception interne au client en production

### Critères d'\''acceptation
- [ ] Aucune stack trace exposée en mode Production
- [ ] `ProblemDetails` standard (RFC 7807) retourné pour toutes les 500
- [ ] Exceptions loggées avec corrélation ID

### Estimation
**Effort :** 0.5 jour
'

echo "✅ SEC-007 created"

# ---------------------------------------------------------------------------
# DB-001 — HasMaxLength manquant
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DB-001] Ajouter HasMaxLength() sur toutes les propriétés string des configurations EF Core" \
  --label "severity: critical,$AUDIT_LABEL,area: database,phase: 2-database,type: bug" \
  --body '## Contexte (Audit DB-001)

**Sévérité :** 🔴 CRITIQUE  
**Fichiers :** 20+ configurations EF Core dans `src/Api/InfraFlowSculptor.Infrastructure/Persistence/Configurations/`

### Constat

20+ configurations EF Core n'\''ont pas de `HasMaxLength()` sur leurs propriétés string. PostgreSQL crée des colonnes `text` illimitées, ce qui :
- Permet le stockage de données arbitrairement grandes
- Dégrade les performances des index
- Viole les contraintes Azure (ex : noms de blob containers limités à 63 chars)

### Configurations impactées (non exhaustif)

| Configuration | Propriété | Limite Azure recommandée |
|---------------|-----------|--------------------------|
| `BlobContainerConfiguration` | `Name` | 63 chars |
| `StorageQueueConfiguration` | `Name` | 63 chars |
| `StorageTableConfiguration` | `Name` | 63 chars |
| `RoleAssignmentConfiguration` | `RoleDefinitionId` | 256 chars |
| `ParameterDefinitionConfiguration` | `DefaultValue` | 4000 chars |
| `ProjectMemberConfiguration` | Toutes les strings | 256 chars |
| `KeyVaultConfiguration` | Propriétés string | 127 chars (vault name) |
| `ResourceGroupConfiguration` | Propriétés string | 90 chars (RG name) |

### Travail à réaliser

1. **Auditer** toutes les configurations dans `Persistence/Configurations/`
2. **Ajouter** `.HasMaxLength(N)` sur chaque propriété string
3. **Générer une migration** pour appliquer les changements
4. **Tester** que les données existantes ne dépassent pas les nouvelles limites

### Critères d'\''acceptation

- [ ] 100% des propriétés string dans les configurations EF Core ont un `HasMaxLength()`
- [ ] Les limites respectent les contraintes Azure correspondantes
- [ ] Une migration EF Core est générée et testée
- [ ] Aucune donnée existante ne viole les nouvelles contraintes

### Estimation
**Effort :** 2 jours
'

echo "✅ DB-001 created"

# ---------------------------------------------------------------------------
# DB-002 — Index manquants
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DB-002] Ajouter les index manquants sur les colonnes de filtrage/jointure EF Core" \
  --label "severity: critical,$AUDIT_LABEL,area: database,phase: 2-database,type: performance" \
  --body '## Contexte (Audit DB-002)

**Sévérité :** 🔴 CRITIQUE  
**Fichiers :** 30+ configurations EF Core sans `HasIndex`

### Constat

30+ configurations EF Core n'\''ont aucun index. Les colonnes utilisées dans les requêtes de filtrage et de jointure provoquent des **full table scans**.

### Colonnes critiques nécessitant un index

| Table | Colonne(s) | Usage | Type d'\''index |
|-------|------------|-------|---------------|
| `AzureResources` | `ResourceGroupId` | Filtrage par RG | Simple |
| `AzureResources` | `Name` | Recherche | Simple |
| `AzureResources` | `InfrastructureConfigId` | Filtrage principal | Simple |
| `RoleAssignments` | `SourceResourceId` | Jointures | Simple |
| `RoleAssignments` | `TargetResourceId` | Jointures | Simple |
| `RoleAssignments` | `(Source, Target, Identity, Role)` | Unicité | Composite unique |
| `ResourceGroups` | `InfraConfigId` | Access checks | Simple |
| `InfrastructureConfigs` | `ProjectId` | Filtrage principal | Simple |
| `ProjectMembers` | `UserId` | Lookup membership | Simple |

### Travail à réaliser

1. **Analyser** toutes les requêtes dans les repositories pour identifier les colonnes filtrées
2. **Ajouter** `builder.HasIndex(x => x.Column)` dans les configurations
3. **Ajouter les index composites** pour les requêtes multi-colonnes
4. **Générer une migration** pour créer les index
5. **Vérifier** avec `EXPLAIN ANALYZE` les plans d'\''exécution

### Critères d'\''acceptation

- [ ] Index ajoutés sur toutes les colonnes listées ci-dessus
- [ ] Index composites ajoutés pour les requêtes multi-colonnes fréquentes
- [ ] Migration EF Core générée et testée
- [ ] Plans d'\''exécution vérifiés (pas de Seq Scan sur les grandes tables)

### Estimation
**Effort :** 2 jours
'

echo "✅ DB-002 created"

# ---------------------------------------------------------------------------
# DB-003 — AsNoTracking manquant
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DB-003] Ajouter AsNoTracking() sur toutes les requêtes de lecture (queries)" \
  --label "severity: high,$AUDIT_LABEL,area: database,phase: 2-database,type: performance" \
  --body '## Contexte (Audit DB-003)

**Sévérité :** 🟠 HAUTE  
**Fichier :** `src/Api/InfraFlowSculptor.Infrastructure/Persistence/Repositories/BaseRepository.cs`

### Constat
`GetAllAsync()` et les méthodes de lecture ne utilisent pas `.AsNoTracking()`. Le change tracker d'\''EF Core suit inutilement toutes les entités lues, causant un overhead mémoire et CPU.

### Travail à réaliser
1. Créer un `ReadOnlyBaseRepository<T>` qui utilise `.AsNoTracking()` par défaut
2. OU ajouter `.AsNoTracking()` dans toutes les méthodes `Get*` des repositories de lecture
3. S'\''assurer que les méthodes qui modifient les entités (commands) n'\''utilisent PAS `AsNoTracking()`

### Critères d'\''acceptation
- [ ] Toutes les requêtes de lecture utilisent `AsNoTracking()`
- [ ] Les requêtes de modification (commands) gardent le tracking
- [ ] Aucune régression sur les opérations CRUD

### Estimation
**Effort :** 1 jour
'

echo "✅ DB-003 created"

# ---------------------------------------------------------------------------
# DB-004 — 16 requêtes séquentielles dans ResourceGroupRepository
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DB-004] Optimiser les 16 requêtes DB séquentielles dans ResourceGroupRepository" \
  --label "severity: high,$AUDIT_LABEL,area: database,phase: 2-database,type: performance" \
  --body '## Contexte (Audit DB-004)

**Sévérité :** 🟠 HAUTE  
**Fichier :** `src/Api/InfraFlowSculptor.Infrastructure/Persistence/Repositories/ResourceGroupRepository.cs:151-269`

### Constat
`GetConfiguredEnvironmentsByResourceGroupAsync` exécute **16 requêtes DB séparées** (une par type d'\''environment settings). Cela cause une latence proportionnelle au nombre de types de ressources.

### Travail à réaliser
1. Regrouper les 16 requêtes en une seule requête SQL avec UNION ALL
2. OU utiliser `Task.WhenAll()` pour paralléliser les requêtes
3. OU implémenter un système de caching court (30s) sur ce endpoint

### Critères d'\''acceptation
- [ ] Nombre de requêtes DB réduit de 16 à 1-3 maximum
- [ ] Temps de réponse de l'\''endpoint amélioré
- [ ] Résultats identiques (pas de régression)

### Estimation
**Effort :** 1 jour
'

echo "✅ DB-004 created"

# ---------------------------------------------------------------------------
# DB-005 — Contrainte unique manquante sur RoleAssignment
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DB-005] Ajouter une contrainte d'unicité sur RoleAssignment" \
  --label "severity: high,$AUDIT_LABEL,area: database,phase: 2-database,type: bug" \
  --body '## Contexte (Audit DB-005)

**Sévérité :** 🟠 HAUTE  
**Fichier :** `src/Api/InfraFlowSculptor.Infrastructure/Persistence/Configurations/RoleAssignmentConfiguration.cs`

### Constat
Pas de contrainte unique empêchant les doublons de role assignments. Un même rôle peut être assigné plusieurs fois à la même combinaison source-target-identity-role.

### Travail à réaliser
Ajouter dans `RoleAssignmentConfiguration.cs` :
```csharp
builder.HasIndex(r => new { r.SourceResourceId, r.TargetResourceId, r.UserAssignedIdentityId, r.RoleDefinitionId })
    .IsUnique();
```

Générer la migration correspondante.

### Critères d'\''acceptation
- [ ] Contrainte unique créée en base
- [ ] Tentative de doublon retourne une erreur métier propre (pas un 500 EF Core)
- [ ] Migration générée et testée

### Estimation
**Effort :** 0.5 jour
'

echo "✅ DB-005 created"

# ---------------------------------------------------------------------------
# DB-006 — Cascade delete risquée
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DB-006] Auditer et corriger les cascade deletes risquées (AppSetting)" \
  --label "severity: high,$AUDIT_LABEL,area: database,phase: 2-database,type: bug" \
  --body '## Contexte (Audit DB-006)

**Sévérité :** 🟠 HAUTE  
**Fichier :** `src/Api/InfraFlowSculptor.Infrastructure/Persistence/Configurations/AppSettingConfiguration.cs:63-73`

### Constat
`AppSettingConfiguration` a 2 FK vers `AzureResource` avec `DeleteBehavior.Cascade`. La suppression d'\''une resource source ou d'\''un KeyVault cascade-delete **tous** les AppSettings associés, ce qui peut causer des pertes de données inattendues.

### Travail à réaliser
1. Évaluer si `SetNull` ou `Restrict` est plus approprié pour `KeyVaultResourceId`
2. Auditer toutes les autres configurations pour des cascades similaires
3. Documenter la stratégie de cascade delete choisie

### Critères d'\''acceptation
- [ ] Comportement de cascade évalué et corrigé si nécessaire
- [ ] Documentation de la stratégie de cascade dans le code (commentaires)
- [ ] Tests d'\''intégration vérifiant le comportement de suppression

### Estimation
**Effort :** 1 jour
'

echo "✅ DB-006 created"

# ---------------------------------------------------------------------------
# DB-007 — Code dupliqué dans StorageAccountRepository
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DB-007] Éliminer la duplication de code dans StorageAccountRepository" \
  --label "severity: high,$AUDIT_LABEL,area: database,phase: 2-database,type: refactor" \
  --body '## Contexte (Audit DB-007)

**Sévérité :** 🟠 HAUTE  
**Fichier :** `src/Api/InfraFlowSculptor.Infrastructure/Persistence/Repositories/StorageAccountRepository.cs:18-42`

### Constat
`GetByIdAsync` et `GetByIdWithSubResourcesAsync` dupliquent les mêmes chaînes d'\''`.Include()`.

### Travail à réaliser
Extraire les includes communs dans une méthode privée et réutiliser.

### Critères d'\''acceptation
- [ ] Code dupliqué éliminé
- [ ] Comportement identique vérifié

### Estimation
**Effort :** 0.5 jour
'

echo "✅ DB-007 created"

# ---------------------------------------------------------------------------
# DB-008 — Migrations à squasher
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DB-008] Consolider les 56 migrations EF Core accumulées" \
  --label "severity: high,$AUDIT_LABEL,area: database,phase: 2-database,type: tech-debt" \
  --body '## Contexte (Audit DB-008)

**Sévérité :** 🟠 HAUTE  
**Dossier :** `src/Api/InfraFlowSculptor.Infrastructure/Migrations/` — 112 fichiers

### Constat
56 migrations se sont accumulées depuis mars 2026 sans consolidation. Impact sur le temps de démarrage, la lisibilité, et les risques de conflits de merge.

### Travail à réaliser
1. Supprimer toutes les migrations existantes
2. Regénérer une migration initiale propre : `dotnet ef migrations add InitialCreate`
3. Vérifier que le schéma généré est identique au schéma actuel
4. Documenter la procédure dans le README

### Critères d'\''acceptation
- [ ] 1 seule migration initiale propre
- [ ] Schéma identique vérifié (diff du SQL généré)
- [ ] Procédure documentée pour les prochains squash

### Estimation
**Effort :** 1 jour
'

echo "✅ DB-008 created"

# ---------------------------------------------------------------------------
# APP-001 — 70 commands sans validator
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[APP-001] Créer les 70 validators FluentValidation manquants" \
  --label "severity: critical,$AUDIT_LABEL,area: application,phase: 4-application,type: bug" \
  --body '## Contexte (Audit APP-001)

**Sévérité :** 🔴 CRITIQUE  
**Couverture actuelle :** 53/123 commands ont un validator (43%)

### Constat
70 commands CQRS n'\''ont **aucun** validator FluentValidation. Des données invalides peuvent entrer dans le domaine, causant des corruptions silencieuses et des erreurs 500 au lieu de 400.

### Commands critiques sans validator (non exhaustif)

- Tous les `DeleteXxxCommand` (pas de validation que l'\''ID n'\''est pas vide)
- `CreateResourceGroupCommand`
- `CreateAppServicePlanCommand`
- `CreateFunctionAppCommand`
- `CreateCosmosDbCommand`
- `CreateAppConfigurationCommand`
- `CreateContainerAppCommand`
- `CreateLogAnalyticsWorkspaceCommand`
- `UpdateXxxCommand` (multiples)

### Travail à réaliser

Pour chaque command sans validator :
1. Créer un fichier `XxxCommandValidator.cs` dans le même dossier que la command
2. Valider au minimum :
   - `NotEmpty()` sur tous les Guid/ID
   - `MaximumLength()` sur tous les strings
   - `IsInEnum()` ou custom validation sur les enums string
3. Suivre le pattern existant des validators en place

### Critères d'\''acceptation
- [ ] 123/123 commands ont un validator (100% de couverture)
- [ ] Toutes les IDs validées avec `NotEmpty()`
- [ ] Toutes les strings validées avec `MaximumLength()`
- [ ] Les enums validés avec le pattern `EnumValidation`
- [ ] `dotnet build` compile sans erreurs

### Estimation
**Effort :** 5 jours
'

echo "✅ APP-001 created"

# ---------------------------------------------------------------------------
# APP-002 — Autorisations manquantes
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[APP-002] Ajouter les checks d'autorisation manquants sur les handlers sensibles" \
  --label "severity: critical,$AUDIT_LABEL,area: application,area: security,phase: 4-application,type: bug" \
  --body '## Contexte (Audit APP-002)

**Sévérité :** 🔴 CRITIQUE  

### Constat
Les handlers suivants accèdent à des données sensibles **sans vérifier les droits d'\''accès** :

| Handler | Risque |
|---------|--------|
| `GenerateBicepCommandHandler` | Génère du Bicep avec des secrets sans vérifier l'\''accès |
| `GeneratePipelineCommandHandler` | Génère des pipelines sans vérifier l'\''accès |
| `DownloadBicepCommandHandler` | Télécharge des fichiers sans vérifier l'\''accès |
| `DownloadPipelineCommandHandler` | Télécharge des fichiers sans vérifier l'\''accès |
| `CreateProjectCommandHandler` | Pas de restriction sur qui peut créer un projet |

### Travail à réaliser
Pour chaque handler listé :
1. Injecter `IInfraConfigAccessService`
2. Appeler `VerifyReadAccessAsync()` ou `VerifyWriteAccessAsync()` selon l'\''opération
3. Retourner l'\''erreur appropriée si l'\''accès est refusé

### Critères d'\''acceptation
- [ ] Les 5 handlers listés vérifient les droits d'\''accès
- [ ] 100% des handlers ont un check d'\''autorisation
- [ ] Les erreurs d'\''accès retournent 403 Forbidden

### Estimation
**Effort :** 2 jours
'

echo "✅ APP-002 created"

# ---------------------------------------------------------------------------
# APP-003 — N+1 pattern
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[APP-003] Corriger le pattern N+1 dans ListCrossConfigReferencesQueryHandler" \
  --label "severity: critical,$AUDIT_LABEL,area: application,phase: 4-application,type: performance" \
  --body '## Contexte (Audit APP-003)

**Sévérité :** 🔴 CRITIQUE  
**Fichier :** `src/Api/InfraFlowSculptor.Application/InfrastructureConfig/Queries/ListCrossConfigReferences/ListCrossConfigReferencesQueryHandler.cs`

### Constat
Le handler itère sur `config.CrossConfigReferences` et fait un appel `GetByIdAsync()` **par référence** dans une boucle. Cela génère N requêtes DB (N = nombre de références croisées).

### Travail à réaliser
1. Créer une méthode `GetByIdsAsync(IEnumerable<Guid> ids)` dans le repository
2. Charger toutes les références en une seule requête
3. Mapper les résultats en mémoire

### Critères d'\''acceptation
- [ ] 1 seule requête DB au lieu de N
- [ ] `GetByIdsAsync` ajoutée au repository
- [ ] Résultats identiques

### Estimation
**Effort :** 1 jour
'

echo "✅ APP-003 created"

# ---------------------------------------------------------------------------
# APP-004 — dynamic cast
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[APP-004] Remplacer le cast dynamic dans ValidationBehavior" \
  --label "severity: high,$AUDIT_LABEL,area: application,phase: 4-application,type: bug" \
  --body '## Contexte (Audit APP-004)

**Sévérité :** 🟠 HAUTE  
**Fichier :** `src/Api/InfraFlowSculptor.Application/Common/Behaviors/ValidationBehavior.cs:34`

### Constat
`return (dynamic)errors;` contourne le système de types. Peut causer des `RuntimeBinderException` imprévisibles.

### Travail à réaliser
Remplacer par une conversion typée utilisant `ErrorOr.From()` ou réflexion contrôlée.

### Critères d'\''acceptation
- [ ] Plus aucun usage de `dynamic` dans le behavior
- [ ] Compilation sans warnings
- [ ] Comportement de validation identique

### Estimation
**Effort :** 0.5 jour
'

echo "✅ APP-004 created"

# ---------------------------------------------------------------------------
# APP-005 — Handlers god-class
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[APP-005] Décomposer les handlers god-class (400+ lignes)" \
  --label "severity: high,$AUDIT_LABEL,area: application,phase: 4-application,type: refactor" \
  --body '## Contexte (Audit APP-005)

**Sévérité :** 🟠 HAUTE  

### Constat
Plusieurs handlers dépassent 300 lignes avec 5+ responsabilités :
- `GenerateProjectPipelineCommandHandler.cs` — 402 lignes
- `GenerateProjectBicepCommandHandler.cs` — 271 lignes
- `GenerateBicepCommandHandler.cs` — 150+ lignes

### Travail à réaliser
1. Extraire des services dédiés :
   - `IBicepGenerationOrchestrator`
   - `IPipelineGenerationOrchestrator`
   - `IArtifactUploadService`
2. Les handlers deviennent de simples coordinateurs (< 50 lignes)

### Critères d'\''acceptation
- [ ] Aucun handler ne dépasse 100 lignes
- [ ] Services extraits testables indépendamment
- [ ] Comportement identique

### Estimation
**Effort :** 3 jours
'

echo "✅ APP-005 created"

# ---------------------------------------------------------------------------
# APP-006 — .First() sans check
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[APP-006] Remplacer .First() par .FirstOrDefault() + check null dans les handlers" \
  --label "severity: high,$AUDIT_LABEL,area: application,phase: 4-application,type: bug" \
  --body '## Contexte (Audit APP-006)

**Sévérité :** 🟠 HAUTE  
**Fichiers :**
- `GetProjectBicepFileContentQueryHandler.cs:35`
- `GetProjectPipelineFileContentQueryHandler.cs:35`
- `GetBicepFileContentQueryHandler.cs:27`
- `GenerateBicepCommandHandler.cs:105`

### Constat
5+ handlers utilisent `.First()` sur des collections potentiellement vides, causant des `InvalidOperationException` au lieu d'\''erreurs métier propres.

### Travail à réaliser
Remplacer chaque `.First()` par `.FirstOrDefault()` + check null + retour d'\''erreur descriptive `ErrorOr`.

### Critères d'\''acceptation
- [ ] Plus aucun `.First()` sans check dans les handlers
- [ ] Erreurs métier retournées (pas des exceptions)
- [ ] grep `-rn "\.First()" Application/` ne retourne que des usages sûrs

### Estimation
**Effort :** 1 jour
'

echo "✅ APP-006 created"

# ---------------------------------------------------------------------------
# APP-007 — Double chargement agrégat
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[APP-007] Éliminer le double chargement d'agrégat dans GenerateProjectPipelineCommandHandler" \
  --label "severity: high,$AUDIT_LABEL,area: application,phase: 4-application,type: performance" \
  --body '## Contexte (Audit APP-007)

**Sévérité :** 🟠 HAUTE  
**Fichier :** `GenerateProjectPipelineCommandHandler.cs:53-56`

### Constat
Le handler charge le même projet 2 fois via 2 méthodes repository différentes.

### Travail à réaliser
Créer une méthode unique dans le repository qui charge toutes les relations nécessaires en une seule requête.

### Critères d'\''acceptation
- [ ] 1 seule requête DB pour charger le projet
- [ ] Comportement identique

### Estimation
**Effort :** 0.5 jour
'

echo "✅ APP-007 created"

# ---------------------------------------------------------------------------
# DDD-001 — SingleValueObject setter public
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DDD-001] Corriger l'immutabilité de SingleValueObject.Value (setter public)" \
  --label "severity: critical,$AUDIT_LABEL,area: domain,phase: 3-domain,type: bug" \
  --body '## Contexte (Audit DDD-001)

**Sévérité :** 🔴 CRITIQUE  
**Fichier :** `src/Api/InfraFlowSculptor.Domain/Common/Models/SingleValueObject.cs:11`

### Constat
```csharp
public T Value { get; set; } = default!; // ← setter PUBLIC !
```

Tous les value objects basés sur `SingleValueObject<T>` (Name, Location, ShortName, etc.) sont **mutables**. N'\''importe quel code peut modifier `.Value` après la construction, violant le principe fondamental d'\''immutabilité des value objects en DDD.

### Travail à réaliser
Changer en :
```csharp
public T Value { get; private set; } = default!;
```

Ou idéalement avec `init` :
```csharp
public T Value { get; init; } = default!;
```

⚠️ Vérifier que EF Core peut toujours matérialiser les entités (le `private set` est supporté par EF Core).

### Critères d'\''acceptation
- [ ] `Value` n'\''est plus modifiable de l'\''extérieur
- [ ] EF Core matérialise toujours correctement les value objects
- [ ] `dotnet build` compile sans erreur
- [ ] Aucune régression sur les endpoints CRUD

### Estimation
**Effort :** 0.5 jour
'

echo "✅ DDD-001 created"

# ---------------------------------------------------------------------------
# DDD-002 — Aggregats non sealed
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DDD-002] Ajouter sealed sur les 16 classes concrètes d'agrégats AzureResource" \
  --label "severity: critical,$AUDIT_LABEL,area: domain,phase: 3-domain,type: refactor" \
  --body '## Contexte (Audit DDD-002)

**Sévérité :** 🔴 CRITIQUE  

### Constat
Les 16 classes concrètes d'\''agrégats (KeyVault, RedisCache, StorageAccount, WebApp, FunctionApp, etc.) ne sont pas `sealed`. L'\''héritage non contrôlé peut contourner les invariants de domaine.

### Travail à réaliser
Ajouter `sealed` sur toutes les classes concrètes :
```csharp
public sealed class KeyVault : AzureResource { }
public sealed class RedisCache : AzureResource { }
// etc.
```

⚠️ Ne PAS sceller les classes de base (`AzureResource`, `Entity<TId>`, `AggregateRoot`).

### Critères d'\''acceptation
- [ ] Toutes les classes concrètes d'\''agrégats sont `sealed`
- [ ] `dotnet build` compile
- [ ] EF Core TPT fonctionne toujours

### Estimation
**Effort :** 0.5 jour
'

echo "✅ DDD-002 created"

# ---------------------------------------------------------------------------
# DDD-003 — Collections mutables
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DDD-003] Protéger les collections mutables exposées (CorsRule, etc.)" \
  --label "severity: high,$AUDIT_LABEL,area: domain,phase: 3-domain,type: bug" \
  --body '## Contexte (Audit DDD-003)

**Sévérité :** 🟠 HAUTE  
**Fichier :** `src/Api/InfraFlowSculptor.Domain/StorageAccountAggregate/Entities/CorsRule.cs:20-29`

### Constat
`public List<string> AllowedOrigins { get; private set; }` — Le private setter protège la référence, mais `.Add()/.Clear()/.Remove()` sont accessibles depuis l'\''extérieur de l'\''entité.

### Travail à réaliser
Remplacer par un pattern avec backing field :
```csharp
private readonly List<string> _allowedOrigins = [];
public IReadOnlyList<string> AllowedOrigins => _allowedOrigins.AsReadOnly();
```

Appliquer à toutes les collections similaires dans le domaine.

### Critères d'\''acceptation
- [ ] Toutes les collections sont exposées en `IReadOnlyList<T>` ou `IReadOnlyCollection<T>`
- [ ] Les modifications passent par des méthodes de l'\''entité
- [ ] EF Core matérialise correctement via le backing field

### Estimation
**Effort :** 1 jour
'

echo "✅ DDD-003 created"

# ---------------------------------------------------------------------------
# DDD-004 — Navigation properties null-forgiving
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DDD-004] Corriger les propriétés de navigation avec = null! (nullable explicite)" \
  --label "severity: high,$AUDIT_LABEL,area: domain,phase: 3-domain,type: bug" \
  --body '## Contexte (Audit DDD-004)

**Sévérité :** 🟠 HAUTE  
**Fichiers :**
- `ProjectEnvironmentDefinition.cs:21` — `public Project Project { get; set; } = null!;`
- `AzureResource.cs:24` — `public ResourceGroup ResourceGroup { get; set; } = null!;`

### Constat
Le `= null!` masque les warnings du compilateur mais ces propriétés seront `null` en runtime si l'\''eager loading n'\''est pas fait.

### Travail à réaliser
Remplacer par des propriétés nullable explicites :
```csharp
public Project? Project { get; set; }
```

### Critères d'\''acceptation
- [ ] Plus de `= null!` sur les navigation properties
- [ ] Null checks ajoutés dans le code qui accède à ces propriétés
- [ ] Compilation sans null warnings

### Estimation
**Effort :** 1 jour
'

echo "✅ DDD-004 created"

# ---------------------------------------------------------------------------
# DDD-005 — Cycle detection missing
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DDD-005] Implémenter la détection de dépendances circulaires dans AddDependency" \
  --label "severity: high,$AUDIT_LABEL,area: domain,phase: 3-domain,type: bug" \
  --body '## Contexte (Audit DDD-005)

**Sévérité :** 🟠 HAUTE  
**Fichier :** `src/Api/InfraFlowSculptor.Domain/Common/BaseModels/AzureResource.cs:89-98`

### Constat
`AddDependency()` ne détecte pas les cycles (A→B→C→A). Les duplicats sont silencieusement ignorés.

### Travail à réaliser
Implémenter la détection de cycles au niveau du service applicatif (graphe complet des dépendances).

### Critères d'\''acceptation
- [ ] Les dépendances circulaires sont détectées et retournent une erreur
- [ ] Les duplicats retournent une erreur explicite

### Estimation
**Effort :** 2 jours
'

echo "✅ DDD-005 created"

# ---------------------------------------------------------------------------
# DDD-006 — Guard clauses manquantes dans value objects
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DDD-006] Ajouter des guard clauses dans les constructeurs de value objects" \
  --label "severity: medium,$AUDIT_LABEL,area: domain,phase: 3-domain,type: bug" \
  --body '## Contexte (Audit DDD-006)

**Sévérité :** 🟡 MOYENNE  
**Fichiers :** `Name.cs`, `EntraId.cs`, et autres value objects

### Constat
- `Name` ne valide pas les chaînes vides/null
- `EntraId` accepte `Guid.Empty`
- Pas de guard clauses dans les constructeurs

### Travail à réaliser
Ajouter des validations dans chaque constructeur de value object :
```csharp
public Name(string value)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(value);
    Value = value;
}
```

### Critères d'\''acceptation
- [ ] Tous les value objects valident leurs entrées à la construction
- [ ] `Name("")` et `EntraId(Guid.Empty)` lèvent une `ArgumentException`
- [ ] Tests unitaires couvrant les cas limites

### Estimation
**Effort :** 1 jour
'

echo "✅ DDD-006 created"

# ---------------------------------------------------------------------------
# DDD-007 — Aucun domain event
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DDD-007] Implémenter l'infrastructure de domain events" \
  --label "severity: medium,$AUDIT_LABEL,area: domain,phase: 7-quality,type: refactor" \
  --body '## Contexte (Audit DDD-007)

**Sévérité :** 🟡 MOYENNE  

### Constat
Le pattern CQRS est utilisé sans domain events. Aucun événement n'\''est levé lors de la création/modification/suppression d'\''entités. Cela rend impossible :
- Les notifications asynchrones
- L'\''audit trail automatique
- Les side-effects découplés

### Travail à réaliser
1. Ajouter un mécanisme de domain events dans `AggregateRoot`
2. Implémenter un dispatcher dans l'\''infrastructure (EF Core `SaveChangesInterceptor`)
3. Ajouter les premiers events sur les opérations critiques

### Critères d'\''acceptation
- [ ] Infrastructure de domain events en place
- [ ] Au moins 3 events implémentés (Created, Updated, Deleted)
- [ ] Dispatcher testable

### Estimation
**Effort :** 5 jours
'

echo "✅ DDD-007 created"

# ---------------------------------------------------------------------------
# DDD-008 — Duplication pattern Update
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DDD-008] Factoriser le pattern Update dupliqué dans les agrégats AzureResource" \
  --label "severity: medium,$AUDIT_LABEL,area: domain,phase: 7-quality,type: refactor" \
  --body '## Contexte (Audit DDD-008)

**Sévérité :** 🟡 MOYENNE  
**Fichiers :** `StorageAccount.cs`, `WebApp.cs`, `FunctionApp.cs`, `AppServicePlan.cs`

### Constat
Code quasi identique de mise à jour des propriétés communes (Name, Location) répété dans chaque agrégat.

### Travail à réaliser
Extraire `UpdateBasic(Name, Location)` dans `AzureResource` base class.

### Critères d'\''acceptation
- [ ] Code commun factorisé dans la classe de base
- [ ] Aucune duplication restante
- [ ] Comportement identique

### Estimation
**Effort :** 1 jour
'

echo "✅ DDD-008 created"

# ---------------------------------------------------------------------------
# DDD-009 à DDD-015 — Issues medium/low domain
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[DDD-009] Standardiser IReadOnlyCollection vs IReadOnlyList dans le domaine" \
  --label "severity: medium,$AUDIT_LABEL,area: domain,phase: 7-quality,type: refactor" \
  --body '## Contexte (Audit DDD-009)

Utilisation mixte de `IReadOnlyCollection` et `IReadOnlyList` sans règle claire dans `StorageAccount.cs`, `Project.cs`, etc.

### Recommandation
Standardiser sur `IReadOnlyCollection<T>` sauf besoin d'\''accès par index.

### Critères d'\''acceptation
- [ ] Convention documentée et appliquée
- [ ] Compilation sans erreurs
'

echo "✅ DDD-009 created"

gh issue create --repo "$REPO" \
  --title "[DDD-010] Optimiser les computed properties avec allocations inutiles (CorsRules)" \
  --label "severity: medium,$AUDIT_LABEL,area: domain,phase: 7-quality,type: performance" \
  --body '## Contexte (Audit DDD-010)

`StorageAccount.CorsRules` crée une nouvelle `List` + un nouveau `CorsServiceType` à chaque accès via `.Where().ToList()`.

### Travail à réaliser
Pré-calculer avec des constantes `static readonly` ou cacher le résultat.

### Critères d'\''acceptation
- [ ] Plus d'\''allocation à chaque accès aux computed properties
'

echo "✅ DDD-010 created"

gh issue create --repo "$REPO" \
  --title "[DDD-011] Ajouter sealed sur les enum value objects" \
  --label "severity: medium,$AUDIT_LABEL,area: domain,phase: 3-domain,type: refactor" \
  --body '## Contexte (Audit DDD-011)

`StorageAccessTier`, `StorageAccountSku`, `RedisCacheSku`, `Location` et autres classes `EnumValueObject`-derived ne sont pas `sealed`.

### Critères d'\''acceptation
- [ ] Toutes les classes EnumValueObject-derived sont sealed
'

echo "✅ DDD-011 created"

gh issue create --repo "$REPO" \
  --title "[DDD-012] Ajouter ToString() sur SingleValueObject" \
  --label "severity: low,$AUDIT_LABEL,area: domain,phase: 7-quality,type: refactor" \
  --body '## Contexte (Audit DDD-012)

`SingleValueObject` n'\''override pas `ToString()`, retournant le nom du type au lieu de la valeur.

### Fix
```csharp
public override string ToString() => Value?.ToString() ?? "null";
```
'

echo "✅ DDD-012 created"

gh issue create --repo "$REPO" \
  --title "[DDD-013] Standardiser le style de null check (is null vs == null)" \
  --label "severity: low,$AUDIT_LABEL,area: domain,phase: 7-quality,type: refactor" \
  --body '## Contexte (Audit DDD-013)

Utilisation mixte de `== null` et `is null` dans le codebase. Préférer le pattern moderne `is null` / `is not null`.

### Critères d'\''acceptation
- [ ] Tout le code utilise `is null` / `is not null`
- [ ] Configurer un analyzer rule pour enforcer
'

echo "✅ DDD-013 created"

gh issue create --repo "$REPO" \
  --title "[DDD-014] Créer des value objects pour les raw strings du domaine" \
  --label "severity: low,$AUDIT_LABEL,area: domain,phase: 7-quality,type: refactor" \
  --body '## Contexte (Audit DDD-014)

Des raw strings sont utilisées où des value objects fourniraient une meilleure type-safety :
- `StorageAccount.cs` (noms de containers/queues)
- `Project.cs` (resourceType)
- `AppServicePlan.cs` (OsType)

### Travail à réaliser
Créer `BlobContainerName`, `StorageQueueName`, `ResourceTypeName`, `OsType` value objects.
'

echo "✅ DDD-014 created"

# ---------------------------------------------------------------------------
# GEN-001 — God method 950+ lignes
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[GEN-001] Décomposer BicepGenerationEngine.GenerateCore() (950+ lignes)" \
  --label "severity: critical,$AUDIT_LABEL,area: generation,phase: 6-generation,type: refactor" \
  --body '## Contexte (Audit GEN-001)

**Sévérité :** 🔴 CRITIQUE  
**Fichier :** `src/Api/InfraFlowSculptor.BicepGeneration/BicepGenerationEngine.cs`

### Constat
`GenerateCore()` est une méthode unique de **950+ lignes** avec 10+ responsabilités : calcul d'\''identité, résolution de parents, génération de modules, construction de dictionnaires, pruning d'\''outputs.

### Travail à réaliser
Décomposer en méthodes spécialisées :
1. `ComputeIdentityNeeds()`
2. `ResolveParentReferences()`
3. `GenerateModules()`
4. `BuildDictionaries()`
5. `PruneUnusedOutputs()`
6. `AssembleMainBicep()`

### Critères d'\''acceptation
- [ ] Aucune méthode ne dépasse 100 lignes
- [ ] Chaque méthode a une seule responsabilité
- [ ] Le résultat Bicep généré est identique (test diff)
- [ ] Tests unitaires sur chaque méthode extraite

### Estimation
**Effort :** 3 jours
'

echo "✅ GEN-001 created"

# ---------------------------------------------------------------------------
# GEN-002 — String manipulation unsafe
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[GEN-002] Migrer la génération Bicep vers StringBuilder et regex sûres" \
  --label "severity: critical,$AUDIT_LABEL,area: generation,phase: 6-generation,type: bug" \
  --body '## Contexte (Audit GEN-002)

**Sévérité :** 🔴 CRITIQUE  
**Fichier :** `src/Api/InfraFlowSculptor.BicepGeneration/BicepGenerationEngine.cs`

### Constat
- `string.Insert()` en boucle → O(n²) en performance
- Recherches fragiles (`moduleBicep.Contains("\n  identity:")`) qui cassent si l'\''indentation change
- Risque d'\''injection dans le Bicep généré

### Travail à réaliser
1. Migrer vers `StringBuilder` pour la construction incrémentale
2. Utiliser des regex avec détection de limites au lieu de `Contains()`
3. Sanitiser les valeurs interpolées dans le Bicep

### Critères d'\''acceptation
- [ ] Plus de `string.Insert()` en boucle
- [ ] Plus de `Contains()` fragile pour la détection de blocs
- [ ] Bicep généré identique (test diff)

### Estimation
**Effort :** 2 jours
'

echo "✅ GEN-002 created"

# ---------------------------------------------------------------------------
# GEN-003 — .Single() sans gestion erreur
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[GEN-003] Remplacer .Single() par .FirstOrDefault() + erreur descriptive dans les engines" \
  --label "severity: high,$AUDIT_LABEL,area: generation,phase: 6-generation,type: bug" \
  --body '## Contexte (Audit GEN-003)

**Fichier :** `BicepGenerationEngine.cs:151`

`_generators.Single(g => g.ResourceType == resource.Type)` lance une `InvalidOperationException` sans message utile si le générateur n'\''existe pas.

### Fix
```csharp
var generator = _generators.FirstOrDefault(g => g.ResourceType == resource.Type)
    ?? throw new GenerationException($"No Bicep generator found for resource type '\''{resource.Type}'\''");
```
'

echo "✅ GEN-003 created"

# ---------------------------------------------------------------------------
# GEN-004 — YAML injection
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[GEN-004] Protéger contre l'injection YAML dans PipelineGenerationEngine" \
  --label "severity: high,$AUDIT_LABEL,area: generation,area: security,phase: 6-generation,type: bug" \
  --body '## Contexte (Audit GEN-004)

**Sévérité :** 🟠 HAUTE  
**Fichier :** `src/Api/InfraFlowSculptor.PipelineGeneration/PipelineGenerationEngine.cs:192`

### Constat
Interpolation directe de `GroupName` dans le YAML sans échappement. Si `GroupName` contient `${{ }}`, cela crée une injection YAML dans les pipelines Azure DevOps.

### Travail à réaliser
1. Valider/sanitiser les noms de propriétés avant interpolation dans le YAML
2. Échapper les caractères spéciaux YAML : `:`, `#`, `${{ }}`, `|`, `>`

### Critères d'\''acceptation
- [ ] Les valeurs interpolées dans le YAML sont sanitisées
- [ ] Tests avec des valeurs malveillantes (`${{ variables.secret }}`, `key: value # comment`)
- [ ] YAML généré reste valide

### Estimation
**Effort :** 1 jour
'

echo "✅ GEN-004 created"

# ---------------------------------------------------------------------------
# GEN-005 à GEN-010 — Medium/Low generation issues
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[GEN-005] Extraire les magic strings des API versions Bicep dans des constantes" \
  --label "severity: medium,$AUDIT_LABEL,area: generation,phase: 6-generation,type: refactor" \
  --body '## Contexte (Audit GEN-005)

Les API versions Azure (ex: `2023-01-01`) sont hardcodées comme magic strings dans les générateurs Bicep.

### Travail à réaliser
Créer une classe `AzureResourceApiVersions` avec des constantes centralisées.
'

echo "✅ GEN-005 created"

gh issue create --repo "$REPO" \
  --title "[GEN-006] Standardiser la gestion d'erreurs entre les engines de génération" \
  --label "severity: medium,$AUDIT_LABEL,area: generation,phase: 6-generation,type: refactor" \
  --body '## Contexte (Audit GEN-006)

Gestion d'\''erreurs incohérente : `BicepGenerationEngine` (silencieuse), `AppPipelineGenerationEngine` (throws), `PipelineGenerationEngine` (aucune).

### Travail à réaliser
Créer une `GenerationException` commune et standardiser le pattern.
'

echo "✅ GEN-006 created"

gh issue create --repo "$REPO" \
  --title "[GEN-007] Consolider la normalisation des identifiers Bicep" \
  --label "severity: medium,$AUDIT_LABEL,area: generation,phase: 6-generation,type: refactor" \
  --body '## Contexte (Audit GEN-007)

`BicepIdentifierHelper` (camelCase) vs `BicepFormattingHelper.SanitizeBicepKey()` (snake_case) — stratégies incohérentes.

### Travail à réaliser
Consolider en une seule stratégie de nommage documentée.
'

echo "✅ GEN-007 created"

gh issue create --repo "$REPO" \
  --title "[GEN-008] Factoriser le code dupliqué de parsing des CorsRules" \
  --label "severity: medium,$AUDIT_LABEL,area: generation,phase: 6-generation,type: refactor" \
  --body '## Contexte (Audit GEN-008)

`ParseCorsRules()` et `ParseTableCorsRules()` dans `StorageAccountTypeBicepGenerator.cs` sont quasi identiques.

### Travail à réaliser
Consolider en une méthode paramétrée.
'

echo "✅ GEN-008 created"

# ---------------------------------------------------------------------------
# API issues
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[API-001] Standardiser la documentation 401 Unauthorized sur tous les endpoints" \
  --label "severity: medium,$AUDIT_LABEL,area: api,phase: 5-api,type: tech-debt" \
  --body '## Contexte (Audit API-001)

Seulement 2 controllers sur 23+ documentent les réponses 401. Les autres manquent `.ProducesProblem(Status401Unauthorized)`.

### Travail à réaliser
Ajouter `.ProducesProblem(StatusCodes.Status401Unauthorized)` sur tous les endpoints protégés.

### Critères d'\''acceptation
- [ ] Tous les endpoints avec `[Authorize]` documentent le 401
- [ ] OpenAPI/Scalar reflète les réponses 401
'

echo "✅ API-001 created"

gh issue create --repo "$REPO" \
  --title "[API-002] Standardiser les types d'ID (string vs Guid) dans les réponses DTO" \
  --label "severity: medium,$AUDIT_LABEL,area: api,phase: 5-api,type: refactor" \
  --body '## Contexte (Audit API-002)

Incohérence des types d'\''ID : `ProjectResponse.cs` (string), `UserResponse.cs` (Guid), `RoleAssignmentResponse.cs` (Guid).

### Recommandation
Standardiser tous les IDs en `string` dans les DTOs de réponse (meilleur pour JSON/JavaScript).
'

echo "✅ API-002 created"

gh issue create --repo "$REPO" \
  --title "[API-003] Retourner toutes les erreurs de validation (pas seulement la première)" \
  --label "severity: medium,$AUDIT_LABEL,area: api,phase: 5-api,type: bug" \
  --body '## Contexte (Audit API-003)

**Fichier :** `src/Api/InfraFlowSculptor.Api/Errors/ErrorOrExtended.cs:30-52`

`errors.First().Result()` — Seule la première erreur est retournée, les suivantes sont ignorées silencieusement.

### Travail à réaliser
Retourner toutes les erreurs dans un `Results.BadRequest(new { Errors = errors.Select(e => ...) })`.
'

echo "✅ API-003 created"

gh issue create --repo "$REPO" \
  --title "[API-004] Rendre EnumValidation case-insensitive" \
  --label "severity: medium,$AUDIT_LABEL,area: api,phase: 5-api,type: bug" \
  --body '## Contexte (Audit API-004)

**Fichier :** `src/Api/InfraFlowSculptor.Contracts/ValidationAttributes/EnumValidation.cs`

`Enum.IsDefined()` est case-sensitive. `"systemassigned"` est rejeté alors que sémantiquement correct.

### Fix
```csharp
return Enum.TryParse(enumType, value?.ToString(), ignoreCase: true, out _);
```
'

echo "✅ API-004 created"

gh issue create --repo "$REPO" \
  --title "[API-005] Ajouter les validations de limites Azure sur les Tags" \
  --label "severity: medium,$AUDIT_LABEL,area: api,phase: 5-api,type: bug" \
  --body '## Contexte (Audit API-005)

**Fichier :** `src/Api/InfraFlowSculptor.Contracts/Common/Requests/TagRequest.cs`

`Name` et `Value` sont `[Required]` mais sans `[StringLength]`. Azure limite : clé 512 chars, valeur 256 chars, max 15 tags.

### Fix
Ajouter `[StringLength(512)]` sur Name, `[StringLength(256)]` sur Value.
'

echo "✅ API-005 created"

gh issue create --repo "$REPO" \
  --title "[API-006] Corriger le typo KeyVaultControllerController et standardiser le nommage des endpoints" \
  --label "severity: low,$AUDIT_LABEL,area: api,phase: 7-quality,type: refactor" \
  --body '## Contexte (Audit API-006 + API-007)

- Typo dans `Program.cs:69` : `UseKeyVaultControllerController` → `UseKeyVaultController`
- Nommage incohérent : `UseXyzController` vs convention Minimal API `MapXyzEndpoints`

### Travail à réaliser
1. Corriger le typo
2. Standardiser sur `MapXyzEndpoints()` (convention Minimal API)
'

echo "✅ API-006 created"

gh issue create --repo "$REPO" \
  --title "[API-008] Corriger GuidValidation qui accepte les chaînes vides" \
  --label "severity: low,$AUDIT_LABEL,area: api,phase: 7-quality,type: bug" \
  --body '## Contexte (Audit API-008)

`GuidValidation.cs:15-21` — L'\''attribut retourne `true` pour les chaînes vides, ce qui est incohérent avec la sémantique de validation d'\''un Guid.

### Fix
Supprimer la gestion null/empty de `GuidValidation`. Utiliser `[Required]` séparément pour ça.
'

echo "✅ API-008 created"

# ---------------------------------------------------------------------------
# Application medium/low issues
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[APP-008] Standardiser le masquage des erreurs d'autorisation dans les handlers" \
  --label "severity: medium,$AUDIT_LABEL,area: application,phase: 4-application,type: refactor" \
  --body '## Contexte (Audit APP-008)

Certains handlers retournent `NotFound` quand l'\''autorisation échoue (masquage sécuritaire), d'\''autres retournent l'\''erreur auth directement. Pattern incohérent.

### Travail à réaliser
Décider d'\''une stratégie et l'\''appliquer uniformément. Documenter le choix.
'

echo "✅ APP-008 created"

gh issue create --repo "$REPO" \
  --title "[APP-009] Ajouter CancellationToken sur toutes les méthodes async de IRepository" \
  --label "severity: medium,$AUDIT_LABEL,area: application,phase: 4-application,type: refactor" \
  --body '## Contexte (Audit APP-009)

`IRepository.GetAllAsync()` n'\''accepte pas de `CancellationToken`. Certaines opérations ne peuvent pas être annulées proprement.

### Fix
Ajouter `CancellationToken cancellationToken = default` sur toutes les méthodes async de l'\''interface.
'

echo "✅ APP-009 created"

gh issue create --repo "$REPO" \
  --title "[APP-010] Restreindre ValidationBehavior aux commands uniquement (pas les queries)" \
  --label "severity: medium,$AUDIT_LABEL,area: application,phase: 4-application,type: refactor" \
  --body '## Contexte (Audit APP-010)

Le `ValidationBehavior` MediatR s'\''applique à toutes les requêtes (commands + queries). Les queries ne devraient pas être validées car elles ne mutent pas l'\''état.

### Fix
```csharp
if (request is not ICommandBase)
    return await next();
```
'

echo "✅ APP-010 created"

gh issue create --repo "$REPO" \
  --title "[APP-011] Créer des domain services pour la logique métier cross-cutting" \
  --label "severity: medium,$AUDIT_LABEL,area: application,phase: 7-quality,type: refactor" \
  --body '## Contexte (Audit APP-011)

La logique de validation des role assignments, des types d'\''identité, etc. est dupliquée dans les handlers.

### Travail à réaliser
Créer des domain services : `IRoleAssignmentValidator`, `IIdentityTypeValidator`, etc.
'

echo "✅ APP-011 created"

gh issue create --repo "$REPO" \
  --title "[APP-012] Standardiser le pattern d'eager loading dans les repositories" \
  --label "severity: medium,$AUDIT_LABEL,area: application,phase: 7-quality,type: refactor" \
  --body '## Contexte (Audit APP-012)

Chaque repository crée ses propres méthodes `GetByIdWithXAsync`. Pas de convention commune.

### Recommandation
Standardiser avec une surcharge `GetByIdAsync(id, Func<IQueryable<T>, IQueryable<T>> includes)`.
'

echo "✅ APP-012 created"

gh issue create --repo "$REPO" \
  --title "[APP-013] Factoriser la logique de récupération du blob latest" \
  --label "severity: low,$AUDIT_LABEL,area: application,phase: 7-quality,type: refactor" \
  --body '## Contexte (Audit APP-013)

`GetProjectBicepFileContentQueryHandler` et `GetBicepFileContentQueryHandler` dupliquent la logique de tri des blobs par timestamp.

### Fix
Extraire dans un `BlobFileService` partagé.
'

echo "✅ APP-013 created"

gh issue create --repo "$REPO" \
  --title "[APP-014] Standardiser le nommage des méthodes de repository" \
  --label "severity: low,$AUDIT_LABEL,area: application,phase: 7-quality,type: refactor" \
  --body '## Contexte (Audit APP-014)

`GetByResourceIdAsync` vs `GetByIdAsync` vs `GetByIdWithXAsync` — nommage incohérent.

### Recommandation
Établir et documenter une convention :
- `GetByIdAsync(id)` — fetch de base
- `GetByIdAsync(id, includes)` — avec eager loading
- `GetByResourceIdAsync(resourceId)` — par clé alternative
- `GetByIdsAsync(ids)` — batch fetch
'

echo "✅ APP-014 created"

# ---------------------------------------------------------------------------
# Infrastructure issues
# ---------------------------------------------------------------------------
gh issue create --repo "$REPO" \
  --title "[INFRA-001] Documenter la raison du Singleton pour BlobService/GeneratedArtifactService" \
  --label "severity: medium,$AUDIT_LABEL,area: infrastructure,phase: 7-quality,type: tech-debt" \
  --body '## Contexte (Audit INFRA-001)

`BlobService` et `GeneratedArtifactService` sont enregistrés en Singleton (`DependencyInjection.cs:103-104`). Acceptable car `BlobServiceClient` est thread-safe, mais non documenté.

### Travail à réaliser
Ajouter un commentaire expliquant le choix du Singleton et les contraintes de thread-safety.
'

echo "✅ INFRA-001 created"

gh issue create --repo "$REPO" \
  --title "[INFRA-002] Utiliser IHttpClientFactory au lieu de new HttpClient dans GitHubGitProviderService" \
  --label "severity: medium,$AUDIT_LABEL,area: infrastructure,phase: 4-application,type: bug" \
  --body '## Contexte (Audit INFRA-002)

**Fichier :** `src/Api/InfraFlowSculptor.Infrastructure/Services/GitProviders/GitHubGitProviderService.cs:164`

Nouveau `HttpClient` créé à chaque appel au lieu de réutiliser via `IHttpClientFactory`.

### Risque
Socket exhaustion sous charge.

### Fix
Injecter `IHttpClientFactory` et utiliser `.CreateClient("GitHub")`.

### Critères d'\''acceptation
- [ ] `IHttpClientFactory` utilisé
- [ ] Named client configuré dans DI
- [ ] Plus de `new HttpClient()` dans le service
'

echo "✅ INFRA-002 created"

gh issue create --repo "$REPO" \
  --title "[INFRA-003] Ajouter des projections DTO dans les repositories pour les queries simples" \
  --label "severity: low,$AUDIT_LABEL,area: infrastructure,phase: 7-quality,type: performance" \
  --body '## Contexte (Audit INFRA-003)

Les repositories chargent des agrégats entiers quand seuls quelques champs sont nécessaires.

### Travail à réaliser
Créer des méthodes de projection `.Select()` pour les queries list/summary.
'

echo "✅ INFRA-003 created"

gh issue create --repo "$REPO" \
  --title "[INFRA-004] Ajouter DbSet<AzureResource> pour les requêtes polymorphiques TPT" \
  --label "severity: low,$AUDIT_LABEL,area: infrastructure,phase: 7-quality,type: refactor" \
  --body '## Contexte (Audit INFRA-004)

`ProjectDbContext` n'\''expose pas de `DbSet<AzureResource>`, rendant les requêtes polymorphiques TPT plus complexes.

### Fix
Ajouter `public DbSet<AzureResource> AzureResources { get; set; }` dans le DbContext.
'

echo "✅ INFRA-004 created"

echo ""
echo "================================================"
echo "🎉 Toutes les issues d'audit ont été créées !"
echo "================================================"
echo ""
echo "📊 Résumé :"
echo "  🔴 Critiques : 13 issues"
echo "  🟠 Hautes    : 16 issues"
echo "  🟡 Moyennes  : 16 issues"
echo "  🟢 Basses    : 10 issues"
echo "  ─────────────────────"
echo "  TOTAL        : 55 issues"
echo ""
echo "💡 Les issues sont groupées par phase (labels phase: X)"
echo "   Utiliser les GitHub Project boards pour planifier par sprint."
