# Plan de remédiation SonarQube — PR 37

> **402 issues** au total · Généré le 2026-04-14 · Projet `FlorianDrevet_infra-pipeline-editor`
>
> Ce plan est conçu pour être exécuté par des agents Copilot en **passages successifs**.
> Chaque passage est autonome, traite un périmètre cohérent, et peut être validé indépendamment.

---

## Résumé par sévérité

| Sévérité | Nombre | Statut |
|----------|--------|--------|
| 🔴 BLOCKER | 2 | ✅ Corrigé (passage 0) |
| 🟠 CRITICAL | 54 | ✅ Partiellement corrigé (passage 0), reste ~25 |
| 🟡 MAJOR | 161 | ⬜ À traiter |
| 🔵 MINOR | 185 | ⬜ À traiter |

---

## Passage 0 — Blockers & Critical Quick-Fixes ✅

> **Déjà réalisé** dans le commit `462ec7e`.

- [x] **S6418** (BLOCKER) — Faux positif `KeyVaultSecretsUser`/`Officer` → `[SuppressMessage]`
- [x] **S3218** (CRITICAL ×18) — Shadowing dans `AzureResourceTypes.ArmTypes` → `#pragma warning disable`
- [x] **S1186** (CRITICAL ×2) — Migrations vides → commentaires explicatifs
- [x] **S3735** (CRITICAL ×2) — Opérateur `void` TypeScript → préfixe `_`
- [x] **S2365** (CRITICAL ×2) — `CorsRules`/`TableCorsRules` property → méthodes `GetCorsRules()`
- [x] **S7059** (CRITICAL ×1) — Async dans constructor → `afterNextRender()`

---

## Passage 1 — Complexité cognitive C# (CRITICAL S3776)

> **Agent** : `dotnet-dev` · **19 issues dans 14 fichiers** · Priorité haute

La règle S3776 exige une complexité cognitive ≤ 15. Ce passage nécessite de refactorer les handlers et engines les plus complexes.

### Stratégie de refactoring

Pour chaque handler, extraire des méthodes privées bien nommées, ou créer des classes helper dédiées.

### Fichiers à traiter

- [ ] `src/Api/InfraFlowSculptor.BicepGeneration/Assemblers/MainBicepAssembler.cs` (L18 — CC=130 ⚠️ le plus critique)
  - Décomposer `AssembleMainBicep` en méthodes par section : paramètres, modules, outputs
- [ ] `src/Api/InfraFlowSculptor.BicepGeneration/BicepGenerationEngine.cs` (L68 CC=49, L292 CC=23, L358 CC=46, L505 CC=17, L733 CC=25)
  - 5 méthodes à refactorer — extraire des sous-méthodes par type de ressource
- [ ] `src/Api/InfraFlowSculptor.BicepGeneration/BicepAssembler.cs` (L20 CC=19, L122 CC=17)
  - Extraire la logique de rendering par section
- [ ] `src/Api/InfraFlowSculptor.Application/AppSettings/Commands/AddAppSetting/AddAppSettingCommandHandler.cs` (L33 CC=36)
  - Extraire la validation et la logique conditionnelle en méthodes
- [ ] `src/Api/InfraFlowSculptor.Application/AppConfigurations/Commands/AddAppConfigurationKey/AddAppConfigurationKeyCommandHandler.cs` (L25 CC=35)
  - Même pattern : extraire sous-validations
- [ ] `src/Api/InfraFlowSculptor.Application/InfrastructureConfig/Queries/ListIncomingCrossConfigReferences/ListIncomingCrossConfigReferencesQueryHandler.cs` (L22 CC=27)
- [ ] `src/Api/InfraFlowSculptor.Application/RedisCaches/Commands/CreateRedisCache/CreateRedisCacheCommandHandler.cs` (L20 CC=22)
- [ ] `src/Api/InfraFlowSculptor.Application/RedisCaches/Commands/UpdateRedisCache/UpdateRedisCacheCommandHandler.cs` (L19 CC=23)
- [ ] `src/Api/InfraFlowSculptor.Application/AppConfigurations/Queries/ListAppConfigurationKeys/ListAppConfigurationKeysQueryHandler.cs` (L20 CC=19)
- [ ] `src/Api/InfraFlowSculptor.Application/AppSettings/Queries/ListAppSettings/ListAppSettingsQueryHandler.cs` (L21 CC=19)
- [ ] `src/Api/InfraFlowSculptor.Application/RoleAssignments/Queries/ListRoleAssignmentsByIdentity/ListRoleAssignmentsByIdentityQueryHandler.cs` (L26 CC=17)
- [ ] `src/Api/InfraFlowSculptor.Infrastructure/Persistence/Repositories/InfrastructureConfigReadRepository.cs` (L81 CC=42)
- [ ] `src/Api/InfraFlowSculptor.Infrastructure/Services/GitProviders/AzureDevOpsGitProviderService.cs` (L46 CC=36)
- [ ] `src/Api/InfraFlowSculptor.Infrastructure/Services/GitProviders/GitHubGitProviderService.cs` (L36 CC=16)

### Vérification

```bash
dotnet build InfraFlowSculptor.slnx
```

---

## Passage 2 — Complexité cognitive TypeScript (CRITICAL S3776)

> **Agent** : `angular-front` · **8 issues dans 4 fichiers**

- [ ] `src/Front/src/app/features/project-detail/project-detail.component.ts` (L175 CC=65, L244 CC=49)
  - Les deux fonctions les plus complexes du frontend — extraire en sous-méthodes
- [ ] `src/Front/src/app/features/config-detail/config-detail.component.ts` (L180 CC=35, L1410 CC=16)
- [ ] `src/Front/src/app/features/resource-edit/add-app-config-key-dialog/add-app-config-key-dialog.component.ts` (L149 CC=22, L432 CC=22)
- [ ] `src/Front/src/app/features/resource-edit/add-app-setting-dialog/add-app-setting-dialog.component.ts` (L162 CC=30, L438 CC=23)

### Vérification

```bash
cd src/Front && npm run typecheck && npm run build
```

---

## Passage 3 — Contraste CSS (MAJOR S7924)

> **Agent** : `angular-front` + skill `ui-ux-front-saas` · **109 issues dans 15 fichiers**

Le problème : des couleurs de texte ne respectent pas le ratio de contraste WCAG AA (4.5:1).

### Fichiers à traiter

- [ ] `src/Front/src/app/features/resource-edit/add-app-config-key-dialog/add-app-config-key-dialog.component.scss` (14 issues)
- [ ] `src/Front/src/app/features/resource-edit/resource-edit.component.scss` (13 issues)
- [ ] `src/Front/src/app/features/resource-edit/add-app-setting-dialog/add-app-setting-dialog.component.scss` (12 issues)
- [ ] `src/Front/src/app/features/config-detail/config-detail.component.scss` (11 issues)
- [ ] `src/Front/src/app/features/project-detail/project-detail.component.scss` (13 issues)
- [ ] `src/Front/src/app/shared/components/generation-diagnostics-dialog/generation-diagnostics-dialog.component.scss` (7 issues)
- [ ] `src/Front/src/app/shared/components/deployment-config/deployment-config.component.scss` (6 issues)
- [ ] `src/Front/src/app/shared/components/diagnostic-popover/diagnostic-popover.component.scss` (5 issues)
- [ ] `src/Front/src/app/shared/components/compact-select/compact-select.component.scss` (5 issues)
- [ ] `src/Front/src/app/features/config-detail/push-to-git-dialog/push-to-git-dialog.component.scss` (5 issues)
- [ ] `src/Front/src/app/features/resource-edit/create-uai-dialog/create-uai-dialog.component.scss` (4 issues)
- [ ] `src/Front/src/app/features/project-detail/git-config-dialog/git-config-dialog.component.scss` (4 issues)
- [ ] `src/Front/src/app/features/config-detail/add-resource-dialog/add-resource-dialog.component.scss` (3 issues)
- [ ] `src/Front/src/app/app.component.scss` (3 issues)
- [ ] `src/Front/src/app/features/resource-edit/add-role-assignment-dialog/add-role-assignment-dialog.component.scss` (2 issues)

### Stratégie

1. Identifier les couleurs de texte insuffisamment contrastées (souvent des gris `#999`, `#aaa` sur fond blanc/sombre)
2. Remplacer par des couleurs plus foncées respectant WCAG AA (ratio ≥ 4.5:1)
3. Utiliser un outil comme [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/) pour valider
4. Veiller à conserver la cohérence du design system existant

---

## Passage 4 — Trop de paramètres (MAJOR S107)

> **Agent** : `dotnet-dev` · **17 issues dans 14 fichiers** + **1 TypeScript**

### C# — Introduire des objets de paramétrage

- [ ] `src/Api/InfraFlowSculptor.Application/Projects/Commands/GenerateProjectPipeline/GenerateProjectPipelineCommandHandler.cs` (ctor 10 params)
- [ ] `src/Api/InfraFlowSculptor.Application/InfrastructureConfig/Commands/GeneratePipeline/GeneratePipelineCommandHandler.cs` (ctor 9 params)
- [ ] `src/Api/InfraFlowSculptor.BicepGeneration/Assemblers/MainBicepAssembler.cs` (méthode 8 params)
- [ ] `src/Api/InfraFlowSculptor.Domain/ContainerAppAggregate/ContainerApp.cs` (méthode Create 9 params)
- [ ] `src/Api/InfraFlowSculptor.BicepGeneration/BicepAssembler.cs` (8 params)
- [ ] `src/Api/InfraFlowSculptor.BicepGeneration/BicepGenerationEngine.cs` (8+ params dans 3 méthodes)
- [ ] `src/Api/InfraFlowSculptor.BicepGeneration/Generators/` — plusieurs generators (8 params chacun)
- [ ] `src/Api/InfraFlowSculptor.Application/RoleAssignments/Common/RoleAssignmentImpactAnalyzer.cs`
- [ ] `src/Api/InfraFlowSculptor.PipelineGeneration/Generators/App/AppPipelineYamlHelper.cs`

### TypeScript

- [ ] `src/Front/src/app/shared/pipes/bicep-highlight.pipe.ts` (S107 — trop de params dans transform)

### Stratégie

- Pour les **constructeurs DI** > 7 params : grouper les dépendances connexes dans un service agrégé
- Pour les **méthodes** > 7 params : créer un record/class `Options` ou `Parameters`
- Pour les **factory** `Create()` domain : introduire un `CreateParams` record

---

## Passage 5 — Ternaires imbriquées et code smells TypeScript (MAJOR)

> **Agent** : `angular-front` · **~20 issues**

### Ternaires imbriquées (S3358 — 11 + 1 C#)

- [ ] `src/Front/src/app/features/resource-edit/resource-edit.component.ts` (2 issues)
- [ ] `src/Front/src/app/features/config-detail/add-resource-dialog/add-resource-dialog.component.ts` (1)
- [ ] `src/Front/src/app/features/config-detail/config-detail.component.ts` (4)
- [ ] `src/Front/src/app/features/config-detail/push-to-git-dialog/push-to-git-dialog.component.ts` (2)
- [ ] `src/Front/src/app/features/project-detail/project-detail.component.ts` (2)
- [ ] `src/Api/InfraFlowSculptor.Domain/ProjectAggregate/Entities/GitRepositoryConfiguration.cs` (1 C#)

### Fonctions dupliquées (S4144 — 3)

- [ ] `src/Front/src/app/features/config-detail/add-resource-dialog/add-resource-dialog.component.ts`
- [ ] `src/Front/src/app/features/resource-edit/add-app-config-key-dialog/add-app-config-key-dialog.component.ts`
- [ ] `src/Front/src/app/features/resource-edit/add-role-assignment-dialog/add-role-assignment-dialog.component.ts`

### Dead stores (S1854 — 3)

- [ ] `src/Front/src/app/features/project-detail/project-detail.component.ts` (2)
- [ ] `src/Front/src/app/shared/components/diagnostic-popover/diagnostic-popover.component.ts` (1)

### Accessibilité HTML (Web:S6853 — 3)

- [ ] `src/Front/src/app/features/config-detail/add-resource-dialog/add-resource-dialog.component.html`
- [ ] `src/Front/src/app/features/resource-edit/add-role-assignment-dialog/add-role-assignment-dialog.component.html`

### Autres MAJOR TypeScript

- [ ] `css:S4666` — Sélecteurs CSS dupliqués (2 dans `add-app-config-key-dialog`)
- [ ] `S6660` — Enum member value pas littéral (1)
- [ ] `S2933` — Propriété readonly manquante (1 dans `app-setting.service.ts`)

---

## Passage 6 — Paramètres inutilisés et code dead C# (MAJOR)

> **Agent** : `dotnet-dev` · **~9 issues**

### Paramètres inutilisés (S1172 — 6)

- [ ] `src/Api/InfraFlowSculptor.PipelineGeneration/AppPipelineGenerationEngine.cs` (L76 — `configName`)
- [ ] `src/Api/InfraFlowSculptor.Application/InfrastructureConfig/Commands/PushBicepToGit/PushBicepToGitCommandHandler.cs`
- [ ] `src/Api/InfraFlowSculptor.Application/Projects/Commands/PushProjectBicepToGit/PushProjectBicepToGitCommandHandler.cs`
- [ ] `src/Api/InfraFlowSculptor.Application/Projects/Commands/PushProjectPipelineToGit/PushProjectPipelineToGitCommandHandler.cs`
- [ ] `src/Api/InfraFlowSculptor.BicepGeneration/BicepGenerationEngine.cs`
- [ ] `src/Api/InfraFlowSculptor.Infrastructure/Persistence/Repositories/InfrastructureConfigReadRepository.cs`

### Autres MAJOR C#

- [ ] `S4581` — Migration avec `Guid.Parse()` non sécurisé (1)
- [ ] `S2068` — Password potentiel détecté dans `ResourceOutputCatalog.cs` (1) — probablement faux positif
- [ ] `S2583` — Condition toujours vraie dans `GitHubGitProviderService.cs` (1)

---

## Passage 7 — Code smells MINOR — C# (rapides)

> **Agent** : `dotnet-dev` · **~130 issues**

### Chaînes magiques dans les migrations (S1192 — 56 issues)

⚠️ **Attention** : les fichiers de migration EF Core ne doivent PAS être modifiés après leur application en base.
- [ ] **Décision** : Supprimer ces warnings via un commentaire `// Sonar: migrations are auto-generated` ou un `.editorconfig` exclusion
- [ ] Alternative : ajouter `[assembly: SuppressMessage(...)]` dans un fichier `GlobalSuppressions.cs` dans le projet Migrations

### Chaînes magiques hors migrations (S1192 — ~50 issues restantes)

- [ ] `src/Api/InfraFlowSculptor.PipelineGeneration/` — extraire les constantes YAML
- [ ] `src/Api/InfraFlowSculptor.BicepGeneration/` — extraire les constantes Bicep
- [ ] `src/Api/InfraFlowSculptor.Api/Controllers/ProjectController.cs`

### Casts inutiles (S1905 — 4)

- [ ] `GenerateBicepCommandHandler.cs`
- [ ] `GeneratePipelineCommandHandler.cs`
- [ ] `GenerateProjectBicepCommandHandler.cs`
- [ ] `GenerateProjectPipelineCommandHandler.cs`

### Null-checks inutiles (S4201 — 6)

- [ ] `AddAppConfigurationKeyCommandHandler.cs` (5)
- [ ] `AddAppSettingCommandHandler.cs` (1)

### LINQ au lieu de boucles (S3267 — 3)

- [ ] `ListIncomingCrossConfigReferencesQueryHandler.cs`
- [ ] `BicepGenerationEngine.cs`
- [ ] `AzureDevOpsGitProviderService.cs`

### Divers MINOR C#

- [ ] `S1481` — Variable locale inutilisée dans `AppPipelineYamlHelper.cs`
- [ ] `S4136` — Méthodes non ordonnées dans `ProjectPipelineVariableGroup.cs`
- [ ] `S3887` — Champ mutable sur type readonly dans `AzureResourceTypes.cs`
- [ ] `S2386` — Champ mutable public dans `AzureResourceTypes.cs`
- [ ] `S1075` — URIs en dur dans `AzureRoleDefinitionCatalog.cs`
- [ ] `S2344` — Enum dans type englobant même nom dans `GitProviderType.cs`, `RepositoryMode.cs`
- [ ] `S2325` — Méthode statique possible dans `BicepGenerationEngine.cs`

---

## Passage 8 — Code smells MINOR — TypeScript (rapides)

> **Agent** : `angular-front` · **~60 issues**

### `String#replaceAll()` au lieu de `String#replace()` (S7781 — 21)

- [ ] `add-cross-config-reference-dialog.component.ts`
- [ ] `add-app-config-key-dialog.component.ts`
- [ ] `add-app-setting-dialog.component.ts`
- [ ] `bicep-highlight.pipe.ts`

### Imports inutilisés (S1128 — 13)

- [ ] `compact-select.component.ts` (4 imports)
- [ ] `create-uai-dialog.component.ts` (1)
- [ ] `add-variable-group-dialog.component.ts`
- [ ] `config-detail.component.ts`
- [ ] `git-config-dialog.component.ts`
- [ ] `project-detail.component.ts`
- [ ] `add-app-config-key-dialog.component.ts`
- [ ] `resource-edit.component.ts`
- [ ] `add-app-setting-dialog.component.ts`
- [ ] `add-role-assignment-dialog.component.ts`

### Regex literals au lieu de RegExp (S7780 — 8)

- [ ] `bicep-highlight.pipe.ts` (8 occurrences)

### Divers MINOR TypeScript

- [ ] `S3863` — `@switch` exhaustivité (4 — config-detail, project-detail)
- [ ] `S7755` — Préférer `@let` (3 — config-detail, project-detail)
- [ ] `S7735` — Fallthrough dans `@switch` (3 — add-resource-dialog, resource-edit)
- [ ] `S7773` — `@empty` manquant dans `@for` (2 — resource-edit)
- [ ] `S7771` — `@default` dans `@switch` (1 — project-detail)
- [ ] `S7778` — `track` expression manquante (1 — config-detail)

---

## Passage 9 — Scripts Python (CRITICAL S3776)

> **Agent** : `dev` (pas un agent spécialisé) · **2 issues**

- [ ] `.github/skills/draw-io-diagram-generator/scripts/validate-drawio.py` (L24 CC=80 ⚠️)
  - Décomposer la fonction de validation en sous-fonctions
- [ ] `.github/skills/draw-io-diagram-generator/scripts/add-shape.py` (L28 CC=18)
  - Extraire les branches conditionnelles

---

## Récapitulatif des passages

| # | Périmètre | Agent | Issues | Priorité |
|---|-----------|-------|--------|----------|
| 0 | Blockers & Critical quick-fixes | `dev` | ~27 | ✅ Fait |
| 1 | Complexité cognitive C# | `dotnet-dev` | 19 | 🔴 Haute |
| 2 | Complexité cognitive TypeScript | `angular-front` | 8 | 🔴 Haute |
| 3 | Contraste CSS (WCAG) | `angular-front` | 109 | 🟡 Moyenne |
| 4 | Trop de paramètres (C# + TS) | `dotnet-dev` | 18 | 🟡 Moyenne |
| 5 | Ternaires, duplications, dead code TS | `angular-front` | ~20 | 🟡 Moyenne |
| 6 | Params inutilisés et dead code C# | `dotnet-dev` | 9 | 🟡 Moyenne |
| 7 | Code smells MINOR C# | `dotnet-dev` | ~130 | 🔵 Basse |
| 8 | Code smells MINOR TypeScript | `angular-front` | ~60 | 🔵 Basse |
| 9 | Scripts Python | `dev` | 2 | 🔵 Basse |

---

## Instructions pour chaque passage agent

Chaque passage doit suivre cette séquence :

1. **Lire ce fichier** pour connaître le scope exact
2. **Lire MEMORY.md** pour les conventions projet
3. **Pour le backend** : charger les conventions `dotnet-dev` (`.github/agents/dotnet-dev.agent.md`)
4. **Pour le frontend** : utiliser l'agent `angular-front` + skill `ui-ux-front-saas` si UI
5. **Appliquer les corrections** fichier par fichier
6. **Vérifier le build** :
   - C# : `dotnet build InfraFlowSculptor.slnx`
   - Frontend : `cd src/Front && npm run typecheck && npm run build`
7. **Cocher les items dans ce fichier** au fur et à mesure
8. **Commit et push** via `report_progress`

---

## Notes importantes

- **Migrations EF Core** : Ne JAMAIS modifier le contenu fonctionnel d'une migration déjà appliquée. Pour S1192 dans les migrations, préférer une suppression globale du warning.
- **Faux positifs** : Certains issues (S6418, S2068) sont des faux positifs — les supprimer avec `[SuppressMessage]` ou `// NOSONAR`.
- **Design system** : Avant de modifier des couleurs CSS (passage 3), vérifier la cohérence avec le design existant de la page login (référence visuelle dans `ui-ux-front-saas`).
- **Impact analysis** : Pour les refactorings de complexité (passages 1-2), exécuter `gitnexus_impact` avant de modifier un symbole partagé.
