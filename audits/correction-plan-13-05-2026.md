# Plan de correctifs — Audit 13 mai 2026

> Ce document présente les 66 findings de l'audit du 13 mai 2026, classés par phase de correction avec les recommandations de fix et les issues GitHub associées.

---

## Résumé

| Sévérité | Count | Issues GitHub |
|----------|:-----:|---------------|
| Critical | 2 | #330, #331 |
| High | 19 | #332–#351 |
| Medium | 28 | #352–#379 |
| Low | 15 | #380–#394 |
| **Total** | **65** | **#330–#394** |

**Évolution :** 81 findings (23-04-2026) → **66 findings** (-15 résolus, +14 nouvelles catégories FRONT/MCP/TEST).

---

## Phase P0 — Fondation observabilité et sécurité (sprint courant)

> **Objectif :** Rendre le système observable et sécuriser les surfaces d'attaque ouvertes sur le MCP.

| # | ID | Issue | Titre | Effort | Comment fix |
|---|---|---|---|---|---|
| 1 | SEC-007 | [#330](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/330) | Global exception handler ne loggue pas | Faible | Injecter `ILogger` dans `ErrorHandling.cs`, appeler `LogError(ex, "Unhandled")`, ajouter `Activity.Current?.Id` au ProblemDetails |
| 2 | MCP-001 | [#350](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/350) | MCP sans rate limiting ni security headers | Moyen | Réutiliser `SecurityHeadersMiddleware` + `RateLimitingServiceCollectionExtensions` dans `Mcp/Program.cs` |
| 3 | MCP-002 | [#351](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/351) | MCP tools sans CancellationToken | Faible | Ajouter `CancellationToken ct` à chaque méthode `[McpServerTool]` et propager à `mediator.Send(cmd, ct)` |
| 4 | TEST-001 | [#347](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/347) | Pas de collecte de coverage | Faible | `dotnet add package coverlet.collector` aux 10 projets test + script CI `dotnet test --collect:"XPlat Code Coverage"` |

---

## Phase P1 — Domain integrity et protection données (sprint 1)

> **Objectif :** Garantir les invariants DDD et corriger les failles de concurrence.

| # | ID | Issue | Titre | Effort | Comment fix |
|---|---|---|---|---|---|
| 5 | DOM-002 | [#331](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/331) | AzureResource setters publics | Moyen | `private set` sur toutes les properties + créer méthodes `Rename()`, `MoveToResourceGroup()`, `OverrideName()`. Adapter les handlers qui mutent directement. |
| 6 | DOM-003 | [#336](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/336) | Collections mutables exposées | Moyen | `private readonly List<T> _items = []; public IReadOnlyCollection<T> Items => _items;` + méthodes `AddItem()` / `RemoveItem()` |
| 7 | DOM-005 | [#337](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/337) | AddDependency cycles/self-deps | Faible | Guard `if (dependency.Id == Id) throw new DomainException("Self-dependency")` + `IResourceDependencyValidator` en Application |
| 8 | DOM-006 | [#338](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/338) | Factories Create non uniformisées | Moyen | Pattern: `public static XResource Create(...) { validate; return new XResource { ... }; }` + `protected XResource() { }` |
| 9 | SEC-008 | [#332](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/332) | UserProvisioningMiddleware → Application | Moyen | Créer `IUserProvisioningService` dans Application, implémenter dans Infrastructure avec `INSERT ... ON CONFLICT DO NOTHING`. Middleware appelle le service. |
| 10 | DB-001 | [#333](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/333) | HasMaxLength manquant | Moyen | Test scanner qui détecte toute property `string` sans `HasMaxLength` via réflexion sur le DbModel |
| 11 | DB-003 | [#334](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/334) | AsNoTracking sur GetAllAsync | Faible | 1 ligne : `.AsNoTracking()` dans `GetAllAsync()` de `BaseRepository.cs` |
| 12 | APP-003 | [#339](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/339) | N+1 ListCrossConfigReferences | Moyen | Ajouter `GetByIdsAsync(IEnumerable<InfrastructureConfigId>)` dans le repository |
| 13 | APP-010 | [#364](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/364) | UserProvisioning dans Application | Moyen | Même fix que SEC-008 (#332) |
| 14 | ARCH-001 | [#346](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/346) | Api → DbContext direct | Moyen | Même fix que SEC-008 (#332) |

---

## Phase P2 — Qualité maintenabilité et génération (sprint 2-3)

> **Objectif :** Réduire la complexité des fichiers, décomposer les god classes.

| # | ID | Issue | Titre | Effort | Comment fix |
|---|---|---|---|---|---|
| 15 | GEN-001 | [#343](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/343) | MainBicepAssembler 928 lines | Élevé | Extraire `OutputAssembler`, `ParameterAssembler`, `ModuleDeclarationAssembler`. Chacun < 250 lignes. |
| 16 | GEN-002 | [#344](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/344) | Générateurs > 500 lines | Élevé | Pattern: extraire `Identity`, `Properties`, `EnvSpecific` par générateur. Target < 300 lignes. |
| 17 | GEN-003 | [#345](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/345) | bicep build en CI | Moyen | GitHub Action step: `az bicep build --file <fixture>.bicep` pour chaque fixture de `GenerationParity.Tests` |
| 18 | APP-005 | [#340](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/340) | God-handlers > 300 lines | Élevé | Extraire des orchestrators : `GenerationOrchestrator`, `PushArtifactsOrchestrator`. Handler = dispatch + error mapping. |
| 19 | APP-008 | [#341](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/341) | Duplication mapping Generate* | Moyen | `IGenerationContextBuilder.BuildFrom(Project, InfraConfig, ...)` retournant un VO `GenerationContext` |
| 20 | DB-007 | [#335](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/335) | Duplication Include | Moyen | `private static IQueryable<T> WithSubResources(IQueryable<T> q) => q.Include(...).Include(...)` |
| 21 | MCP-004 | [#379](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/379) | MCP → Api couplage | Moyen | Extraire `InfraFlowSculptor.Shared.DI` avec les registrations communes. MCP référence Application + Infrastructure + Contracts uniquement. |
| 22 | ARCH-003 | [#371](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/371) | GenerationCore ownership | Moyen | Séparer en `GenerationCore.Contracts` (interfaces, DTOs) et `GenerationCore.Engine` (implémentations) |
| 23 | FRONT-001 | [#348](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/348) | resource-edit 3838 lines | Élevé | Créer un composant par type : `web-app-edit/`, `key-vault-edit/`, etc. Le parent devient un routeur `<app-resource-edit-router>` |
| 24 | FRONT-002 | [#349](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/349) | config-detail 2201 lines | Élevé | Extraire : `config-resources-section`, `config-environments-section`, `config-repositories-section`, `config-pipelines-section` |
| 25 | FRONT-005 | [#393](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/393) | Tests frontend | Moyen | Ajouter Jest/Vitest + tests pour les 5 composants les plus critiques |

---

## Phase P3 — Backlog hygiène et dette (backlog long terme)

> **Objectif :** Amélioration continue, dette technique, préparation prod.

### Sécurité

| # | ID | Issue | Titre | Effort | Comment fix |
|---|---|---|---|---|---|
| 26 | SEC-009 | [#352](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/352) | Health checks sans rate limit | Faible | `.RequireRateLimiting("HealthChecks")` policy 30 req/min |
| 27 | SEC-010 | [#353](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/353) | IsAdmin magic string | Faible | `static class AuthPolicies { public const string IsAdmin = "IsAdmin"; }` |
| 28 | SEC-011 | [#354](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/354) | PAT SaveChanges chaque requête | Moyen | `Channel<PatUsageEvent>` + `BackgroundService` qui batch les écritures toutes les 5 min |
| 29 | SEC-012 | [#380](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/380) | PAT scopes absents | Moyen | Modèle `PatScope` (enum flags: Read, Write, Generate, Admin). Claims dans le handler PAT. |

### Base de données

| # | ID | Issue | Titre | Effort | Comment fix |
|---|---|---|---|---|---|
| 30 | DB-008 | [#355](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/355) | 73 migrations sans squash | Moyen | `dotnet ef migrations squash --name InitialSquash` avant go-live |
| 31 | DB-009 | [#356](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/356) | AddAsync simule l'async | Faible | Renommer en `Add` / `Update` synchrones |
| 32 | DB-011 | [#357](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/357) | Conventions EF partagées | Moyen | `BaseEntityConfiguration<T>` + `ApplyConfigurationsFromAssembly` |
| 33 | DB-012 | [#358](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/358) | ExecuteDeleteAsync bulk | Faible | Remplacer les boucles `Remove()` par `ExecuteDeleteAsync(x => condition)` |
| 34 | DB-014 | [#359](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/359) | QueryFilter global | Moyen | Interface `ISoftDeletable` + `HasQueryFilter(x => !x.IsDeleted)` |
| 35 | DB-015 | [#381](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/381) | Conversions Guid? répétées | Faible | `NullableIdValueConverter<TId>` réutilisable |
| 36 | DB-016 | [#382](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/382) | ConcurrencyCheck absent | Moyen | `Property("xmin").IsRowVersion()` sur les agrégats principaux |
| 37 | DB-017 | [#383](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/383) | Transactions multi-agrégats | Faible | Documenter la stratégie et identifier les cas nécessitant `BeginTransaction` |

### Domain / DDD

| # | ID | Issue | Titre | Effort | Comment fix |
|---|---|---|---|---|---|
| 38 | DOM-008 | [#360](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/360) | Discriminator ResourceType | Moyen | Créer VO `ResourceTypeName` ou utiliser shadow property EF |
| 39 | DOM-010 | [#361](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/361) | IsExisting mutable | Faible | `init` ou `private set` + factory param |
| 40 | DOM-011 | [#362](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/362) | Pas d'IDomainEvent | Moyen | `_domainEvents` list + `MediatR.INotification` dispatch post-SaveChanges |
| 41 | DOM-012 | [#384](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/384) | GetEqualityComponents | Faible | Test réflexion vérifiant que toutes les properties sont incluses |
| 42 | DOM-013 | [#385](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/385) | ResourceType primitive obsession | Moyen | VO `ResourceTypeName` encapsulant la string |

### Application / CQRS

| # | ID | Issue | Titre | Effort | Comment fix |
|---|---|---|---|---|---|
| 43 | APP-009 | [#363](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/363) | Behaviors non hiérarchisés | Faible | Test d'intégration vérifiant l'ordre |
| 44 | APP-012 | [#365](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/365) | Validators duplication | Moyen | `EntityCommandValidator<T>` base class |
| 45 | APP-013 | [#386](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/386) | Mapster workaround | Faible | Centraliser dans `MapsterConfig.cs` + `TypeAdapterConfig.Compile()` |
| 46 | APP-014 | [#387](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/387) | CancellationToken non propagé | Faible | Activer `CA2016` en erreur dans `.editorconfig` |

### API / Contracts

| # | ID | Issue | Titre | Effort | Comment fix |
|---|---|---|---|---|---|
| 47 | API-005 | [#366](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/366) | Response DTOs Guid vs string | Faible | Audit complet des Responses, convertir les `Guid` en `string` |
| 48 | API-006 | [#367](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/367) | Magic strings de routes | Moyen | `static class Routes` par controller |
| 49 | API-008 | [#388](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/388) | Cache-Control / ETag | Moyen | `[ResponseCache]` + `ETag` middleware sur les GET fréquents |
| 50 | API-009 | [#389](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/389) | Mapping dans Api | Moyen | Migrer le mapping vers Application layer |

### Génération

| # | ID | Issue | Titre | Effort | Comment fix |
|---|---|---|---|---|---|
| 51 | GEN-004 | [#368](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/368) | Regex fragile Bicep | Élevé | Long terme : IR/AST Bicep typé |
| 52 | GEN-005 | [#369](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/369) | Duplication mono/multi | Moyen | `GenerateInternal` + delegate de pruning |
| 53 | GEN-006 | [#370](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/370) | Magic strings ARM | Faible | Centraliser dans `ArmResourceTypes` constants |
| 54 | GEN-007 | [#390](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/390) | CancellationToken génération | Faible | Propager `CancellationToken` dans les interfaces et implémentations |

### Architecture

| # | ID | Issue | Titre | Effort | Comment fix |
|---|---|---|---|---|---|
| 55 | ARCH-004 | [#372](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/372) | TreatWarningsAsErrors | Faible | `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` dans `Directory.Build.props` + fix warnings existants |
| 56 | ARCH-005 | [#373](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/373) | Roslyn analyseurs | Faible | `<AnalysisLevel>latest-all</AnalysisLevel>` dans `Directory.Build.props` |
| 57 | ARCH-007 | [#391](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/391) | Azure Key Vault config | Moyen | `builder.Configuration.AddAzureKeyVault(...)` conditionnel en prod |

### Tests

| # | ID | Issue | Titre | Effort | Comment fix |
|---|---|---|---|---|---|
| 58 | TEST-002 | [#374](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/374) | Testcontainers | Élevé | Ajouter `Testcontainers.PostgreSql` + `WebApplicationFactory` pour tests d'intégration |
| 59 | TEST-003 | [#375](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/375) | Tests frontend absents | Élevé | Configurer Jest/Vitest + tests composants critiques |
| 60 | TEST-004 | [#392](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/392) | Snapshot tests DTOs | Moyen | `Verify` sur les DTOs de Response pour détecter breaking changes |

### Frontend

| # | ID | Issue | Titre | Effort | Comment fix |
|---|---|---|---|---|---|
| 61 | FRONT-003 | [#376](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/376) | add-resource-dialog 1662 lines | Moyen | Extraire formulaires par type de ressource |
| 62 | FRONT-004 | [#377](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/377) | project-detail 1444 lines | Moyen | Extraire sections (download, generation, members, settings) |

### MCP

| # | ID | Issue | Titre | Effort | Comment fix |
|---|---|---|---|---|---|
| 63 | MCP-003 | [#378](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/378) | Drafts in-memory sans limite | Faible | `MaxDraftCount` option + rejet 429 `TooManyRequests` |
| 64 | MCP-005 | [#394](https://github.com/FlorianDrevet/infra-pipeline-editor/issues/394) | HTTP sans TLS warning | Faible | `if (!env.IsDevelopment() && listenUrl.StartsWith("http://")) logger.LogWarning(...)` |

---

## Métriques cibles (T+90 jours)

| Métrique | Actuel | Cible |
|----------|--------|-------|
| Code coverage | inconnu | ≥ 60 % Domain, ≥ 40 % Application |
| Validators / Commands | 99 % | 100 % |
| AsNoTracking queries lecture | partiel | 100 % |
| Plus gros fichier .NET | 948 lignes | < 400 |
| Plus gros handler | 380 lignes | < 150 |
| Plus gros Angular component | 3 838 lignes | < 500 |
| Findings critiques ouverts | 2 | 0 |
| Findings haute ouverts | 19 | < 5 |
| Tests frontend | 0 | ≥ 30 |
| MCP rate limiting | aucun | global + per-tool |

---

## Priorisation recommandée (Top 10 ROI)

1. **SEC-007** #330 — Logger les exceptions (1h, observabilité immédiate)
2. **MCP-001** #350 — Rate limiting MCP (2h, surface d'attaque ouverte)
3. **DOM-002** #331 — AzureResource private set (4h, débloque DOM-003/005/006)
4. **MCP-002** #351 — CancellationToken MCP (1h, resource leak)
5. **TEST-001** #347 — Coverlet CI (1h, mesure nécessaire)
6. **DB-003** #334 — AsNoTracking GetAll (15min, perf listings)
7. **SEC-008** #332 — UserProvisioning → App (3h, intégrité architecturale)
8. **GEN-001** #343 — Décomposer MainBicepAssembler (8h, file critique)
9. **FRONT-001** #348 — resource-edit → sous-composants (12h, dette frontend)
10. **APP-005** #340 — God handlers (8h, maintenabilité)

---

## Références

- Rapport d'audit complet : [`audits/audit-13-05-2026.md`](audits/audit-13-05-2026.md)
- Audit précédent : [`audits/audit-23-04-2026.md`](audits/audit-23-04-2026.md)
- Issues GitHub : [#330–#394](https://github.com/FlorianDrevet/infra-pipeline-editor/issues?q=label%3A%22audit%3A+2026-05%22)
