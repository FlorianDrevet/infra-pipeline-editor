# D09 — Bootstrap MultiRepo : tracker d'implémentation

> Fichier de suivi vivant pour la réparation de D09 (bootstrap inatteignable en MultiRepo).
> Plan produit par l'agent `architect` le 2026-08-28. Lu en premier en cas de reprise sur un
> autre poste. Mis à jour après chaque lot, avant de passer au suivant.

## Décision d'architecture (résumé)

Le bootstrap n'avait qu'un niveau (projet), contrairement à Bicep (F11b) et Pipeline (F12) qui
ont un niveau config déjà fonctionnel en MultiRepo. Correction : ajouter un niveau config au
bootstrap, symétrique à `GenerateBicepCommand`/`GeneratePipelineCommand`, en réutilisant
`IProjectBootstrapDefinitionBuilder` tel quel (lui passer une liste à un seul élément :
`configs: [configReadModel]`). Mode toujours `FullOwner` au niveau config (pas
`ApplicationOnly`, réservé à la topologie fixe SplitInfraCode). Le bootstrap projet est gardé
explicitement contre MultiRepo via `Project.CanGenerateAllFromProjectLevel()`, comme
Bicep/Pipeline le font déjà.

Détail complet : voir la sortie de l'agent `architect` archivée dans l'historique de session du
2026-08-28 (message contenant "Analyse de la demande" / "Verdict architectural" / "Décisions
d'architecture" / "Plan d'implémentation").

## Statut des lots

| Lot | Contenu | Statut | Owner | Dernière mise à jour | Reste à faire |
|-----|---------|--------|-------|----------------------|----------------|
| Lot 1 | Garde-fou MultiRepo sur le bootstrap projet | Done | dotnet-dev | 2026-08-28 | Guard ajouté sur `GenerateProjectBootstrapPipelineCommandHandler` et `PushProjectBootstrapPipelineToGitCommandHandler` (via `authResult.Value.CanGenerateAllFromProjectLevel()`, juste après le check d'accès). Commentaires corrigés (`ArtifactKind.Bootstrap`, branche `else` du handler generate, XML doc du handler push référence désormais `PushBootstrapToGitCommand` niveau config). 2 tests handler `MultiRepo -> AmbiguousProjectLevelGeneration` ajoutés (RED confirmé puis GREEN). Effet de bord découvert et corrigé : `PushProjectBootstrapPipelineToGitCommandHandlerTests` construisait `_project` sans layout explicite (défaut domaine = `MultiRepo`), ce qui cassait 2 tests existants (`Given_AllStepsSucceed_...`, `Given_ProjectNotFound_...`) une fois le garde ajouté — fixé en posant `AllInOne` dans le constructeur de test. `dotnet build` 0 erreur, 6/6 tests handler OK. |
| Lot 2 | Génération bootstrap niveau config | Done | dotnet-dev | 2026-08-28 | Nouveau slice `InfrastructureConfig/Commands/GenerateBootstrap/` (Command/Validator/Handler) mirroring exact `GeneratePipeline`, réutilise `IProjectBootstrapDefinitionBuilder.BuildAsync(project, configs: [config], ...)` avec `Mode = FullOwner`, upload via `IGeneratedArtifactService` type `"bootstrap"`. `DownloadBootstrap` (Command/Handler/Validator) et `GetBootstrapFileContent` (Query/Handler) ajoutés en mirroring `DownloadPipeline`/`GetPipelineFileContent`. `Errors.InfrastructureConfig.BootstrapFilesNotFoundError`/`BootstrapFileNotFoundError` ajoutés (additif). 15 tests ajoutés (5 handler generate, 3 handler download, 2 query, 2 validators x2 slices) — tous GREEN. Pas de déviation : vérifié que `InfrastructureConfigRepository.GetByIdAsync` (utilisé par `IInfraConfigAccessService.VerifyWriteAccessAsync`) charge bien `.Include(c => c.Repositories)`, donc `domainConfig.Repositories` est disponible pour `RepositoryTargetResolver.Resolve` sans contournement. `RepositoryTargetResolver.Resolve` gérait déjà `ArtifactKind.Bootstrap` avec `config` non-null pour MultiRepo (rôle = Infrastructure) — aucune modification nécessaire sur le resolver. `dotnet build` 0 erreur. |
| Lot 3 | Push bootstrap niveau config | Done | dotnet-dev | 2026-08-28 | Nouveau slice `InfrastructureConfig/Commands/PushBootstrapToGit/` (Command/Validator/Handler) mirroring exact `PushPipelineToGit`, résout via `targetResolver.Resolve(project, config, ArtifactKind.Bootstrap)`, `BasePath = target.PipelineBasePath`, fichiers poussés sans `GeneratedPipelinePathNormalizer.Normalize` (bootstrap.pipeline.yml est un fichier plat unique à la racine du pipeline base path — normalizer non applicable, différence documentée dans le XML doc du handler). 10 tests ajoutés (4 handler + 6 validator) — tous GREEN. `dotnet build` 0 erreur. |
| Lot 4 | Contracts + API + Mapster | Done | dotnet-dev + orchestrateur | 2026-08-31 | Interrompu par une limite de session juste avant la fin (Contracts/Controller/`Routes.cs` déjà créés par l'agent précédent) ; terminé par l'orchestrateur : `app.UseBootstrapGenerationController();` ajouté dans `Program.cs` (manquant, seul bloqueur réel). Vérifié qu'aucun ajout Mapster n'était nécessaire : le endpoint generate construit `GenerateBootstrapResponse` manuellement (comme `GeneratePipelineResponse`), le endpoint push utilise `mapper.Map<PushBootstrapToGitResponse>(value)` par convention Mapster sans `NewConfig` explicite (comme `PushPipelineToGitResponse`) — donc pas de régression ni d'oubli. `dotnet build .\InfraFlowSculptor.slnx` → 0 erreur. `dotnet test .\InfraFlowSculptor.slnx` → 51 échecs total (2 Contracts.Tests + 44 Application.Tests + 4 Api.Tests + 1 Infrastructure.Tests), exactement la baseline connue du 2026-08-28 — aucune régression. |
| Lot 5 | Front service + interfaces | Done | angular-front | 2026-08-31 | `shared/interfaces/bootstrap-generator.interface.ts` (`GenerateBootstrapRequest/Response`, `PushBootstrapToGitRequest/Response`) et `shared/services/bootstrap-generator.service.ts` (`extends BaseGeneratorService`, `basePath = '/generate-bootstrap'`), mirroring exact de `pipeline-generator.*`. `npm run typecheck` OK. |
| Lot 6 | Front onglet Bootstrap + push dédié + i18n | Done | angular-front | 2026-08-31 | `config-detail-generation-section.controller.ts`/`.view-model.ts` : signaux/actions bootstrap (loading, result, errorKey, panelOpen, downloading, nodes, loadFile) mirroring bicep/pipeline ; `generateAll`/`generateAllLoading` incluent désormais bootstrap (`Promise.all([doGenerateBicep(), doGeneratePipeline(), doGenerateBootstrap()])`). Nouvelle dépendance `openBootstrapPushToGitDialog()` injectée par `config-detail.component.ts` (méthode privée dédiée, distincte de `openPushToGitDialog()` de l'en-tête — ne touche pas au défaut D12). `config-detail-generation-section.component.ts/.html` : 3ᵉ onglet `TAB_BOOTSTRAP` (icône `rocket_launch`), contenu intégrant `<app-bootstrap-setup-guide />` existant + bouton "Push to Git" dédié (`.config-detail-generation__push-bootstrap`) posant `isBootstrap: true`. `config-detail-tree.helpers.ts` : `buildConfigBootstrapNodes` (liste plate, pas de dossier, mirroring simplifié de `buildConfigPipelineNodes`). `push-to-git-dialog.component.ts` : injection `BootstrapGeneratorService`, `pushConfigLevelArtifactsToGit` route désormais `isBootstrap` → `bootstrapService.pushToGit` avant le fallback pipeline/bicep. i18n `en.json`/`fr.json` : `CONFIG_DETAIL.GENERATION.TAB_BOOTSTRAP` + groupe complet `CONFIG_DETAIL.BOOTSTRAP.*` (GENERATING, DOWNLOAD, DOWNLOADING, RETRY, ARTIFACTS, TERMINAL_DONE, FILE_LOADING, FILE_ERROR, NO_FILES, GENERATE_ERROR, GENERATE_AUTH_ERROR). Tabs exposés strictement sous `isProjectMultiRepo()` (inchangé, même garde que F11b/F12). Tests ajoutés : 3 nouveaux specs bootstrap dans `config-detail-generation-section.component.spec.ts`, 1 nouveau spec bootstrap-routing dans `push-to-git-dialog.component.spec.ts`, 1 nouveau spec `buildConfigBootstrapNodes` dans `config-detail-tree.helpers.spec.ts`. Pas de `config-detail-generation-section.controller.spec.ts` créé (fichier inexistant avant ce lot — hors périmètre, conforme à la clause "si présent" de la consigne). **Écart au plan (bug pré-existant corrigé au passage, hors scope D09 mais bloquant pour un run vert) :** les 2 tests bicep pré-existants dans `config-detail-generation-section.component.spec.ts` et 2 tests dans `push-to-git-dialog.component.spec.ts` ciblaient le mauvais élément DOM (`app-ds-button` host au lieu du `<button>` interne, et `By.directive(DsButtonComponent)` matchait le bouton "Close" au lieu de "Push" quand plusieurs `app-ds-button` existent) — corrigés (`getButton` cible désormais `${selector} button`, nouveau helper `getPushButton()` prend le dernier `DsButtonComponent`). `npm run typecheck` OK (0 erreur). `npm run build` OK (0 erreur, seul le warning pré-existant de bundle-budget +23 Ko subsiste, non lié à ce lot). Suite Karma complète : lancée en isolation sur les 3 fichiers touchés → tous verts (5/5, 6/6, 3/3). Un `ng test` pleine suite révèle une instabilité pré-existante et non liée (déconnexion du navigateur headless après un test `SplitGenerationSwitcherComponent`/`ConfigDetailTagsSectionComponent` qui déclenche un full page reload vers `/login`) — signalé pour investigation au lot 7, non traité ici (hors périmètre D09). |
| Lot 7 | Validation build/tests/exécution réelle | Partiellement fait | dev (orchestrateur) | 2026-08-31 | Validation automatisée complète : `dotnet build` 0 erreur, `dotnet test` 51 échecs = baseline exacte du 2026-08-28 (0 régression). `npm run typecheck` + `npm run build` 0 erreur. **Reste à faire** : revalidation en conditions réelles (génération + push bootstrap niveau config contre un vrai projet MultiRepo) — volontairement différée : le plan initial (`NEXT.md`, étape 4) prévoyait cette revalidation une fois **D09 ET D10** traités ensemble, et D10 (push projet combiné MultiRepo) n'est pas encore fait. À faire en une seule passe avec D10. Investigation de l'instabilité Karma pré-existante (déconnexion navigateur headless sur `SplitGenerationSwitcherComponent`/`ConfigDetailTagsSectionComponent`, signalée par `angular-front` au lot 6, non liée à D09) reportée — non bloquante pour D09. |

## Règle d'exécution

- Chaque lot suit strictement TDD (`tdd-workflow`) : tests d'abord, RED→GREEN→REFACTOR→VERIFY.
- Après chaque lot backend, mettre à jour le tableau ci-dessus (statut, owner, date, reste à
  faire) avant de passer au lot suivant.
- Le lot 7 ne démarre qu'une fois les lots 1 à 6 à `Done` et le build/tests passés une première
  fois.
- Toute déviation par rapport à ce plan (ex. `accessResult.Value` ne charge pas `.Repositories`
  comme supposé au lot 2) doit être documentée dans la colonne « Reste à faire » du lot
  concerné, pas silencieusement contournée.

## Checklist reprise sur un autre PC

- [ ] Lire `docs/stabilization/NEXT.md` et ce tracker avant de reprendre.
- [ ] Vérifier l'état de la baseline de tests (`dotnet test .\InfraFlowSculptor.slnx`, comparer
      au nombre d'échecs connu — 51 au 2026-08-28).
- [ ] Reprendre au premier lot marqué `Not started` ou `In progress`.
- [ ] Ne pas commencer D10 avant que ce tracker affiche tous les lots à `Done` — D10 dépend de
      l'existence de `PushBootstrapToGitCommand` (lot 3) pour pouvoir router correctement le
      push combiné MultiRepo.

## Défaut adjacent découvert (hors périmètre D09)

**Candidat D12** : le bouton unique « Push to Git » de `config-detail.component.html:29-36`
n'active jamais `isPipeline` sur `PushToGitDialogData` — il ne pousse donc que le Bicep, jamais
le Pipeline, quel que soit l'onglet actif. Non corrigé ici. Le nouvel onglet Bootstrap (lot 6)
évite délibérément ce piège avec un bouton dédié posant `isBootstrap: true` explicitement. À
trier séparément dans `feature-map.md`.

## Plan détaillé par lot

### Lot 1 — Garde-fou MultiRepo sur le bootstrap projet

Fichiers :
- `src/Api/InfraFlowSculptor.Application/Projects/Commands/GenerateProjectBootstrapPipeline/GenerateProjectBootstrapPipelineCommandHandler.cs`
  — après le chargement de `project`, ajouter
  `if (!project.CanGenerateAllFromProjectLevel()) return Errors.GitRouting.AmbiguousProjectLevelGeneration;`
  Renommer le commentaire de la branche `else` (`// AllInOne / MultiRepo: ...` →
  `// AllInOne: ...`).
- `src/Api/InfraFlowSculptor.Application/Projects/Commands/PushProjectBootstrapPipelineToGit/PushProjectBootstrapPipelineToGitCommandHandler.cs`
  — même garde-fou. Corriger le XML doc du type (actuellement trompeur : prétend que
  `PushProjectGeneratedArtifactsToGit` est un fallback MultiRepo valide, ce qui est faux) pour
  pointer vers `PushBootstrapToGitCommand` (`InfrastructureConfig/Commands/PushBootstrapToGit`,
  lot 3).
- `src/Api/InfraFlowSculptor.Application/Common/GitRouting/ArtifactKind.cs` — corriger le XML
  doc de `Bootstrap` (affirme à tort « Project-level bootstrap pipeline artifacts »)
  pour couvrir aussi l'usage niveau config (lot 2).

Validation : tests handlers existants mis à jour avec cas MultiRepo →
`AmbiguousProjectLevelGeneration` (RED avant, GREEN après). `dotnet build .\InfraFlowSculptor.slnx`.

### Lot 2 — Génération bootstrap niveau config

Nouveau slice `InfrastructureConfig/Commands/GenerateBootstrap/` (Command, Validator, Handler)
mirroring `GeneratePipelineCommand`. Réutilise `IProjectBootstrapDefinitionBuilder.BuildAsync`
avec `configs: [configReadModel]`, `Mode = BootstrapMode.FullOwner`. Ajoute
`Errors.InfrastructureConfig.BootstrapFilesNotFoundError` / `BootstrapFileNotFoundError`.
Ajoute aussi `DownloadBootstrap` (Command/Handler) et `GetBootstrapFileContent` (Query/Handler),
mirroring `DownloadPipeline`/`GetPipelineFileContent`.

Validation : `GenerateBootstrapCommandHandlerTests` (config introuvable, pas de dépôt configuré,
génération réussie avec pipelines limités à la config). `AllCommandsHaveValidatorsTests` doit
passer. `dotnet build .\InfraFlowSculptor.slnx`.

### Lot 3 — Push bootstrap niveau config

Nouveau slice `InfrastructureConfig/Commands/PushBootstrapToGit/` (Command, Validator, Handler),
mirroring `PushPipelineToGitCommand`/`PushBicepToGitCommand`. Résout via
`targetResolver.Resolve(project, config, ArtifactKind.Bootstrap)` (config non-null → fonctionne
nativement en MultiRepo). `BasePath = target.PipelineBasePath` (bootstrap.pipeline.yml à la
racine, pas de `GeneratedPipelinePathNormalizer.Normalize`).

Validation : `PushBootstrapToGitCommandHandlerTests` (config introuvable, PAT manquant, aucun
fichier généré, push réussi). `dotnet build .\InfraFlowSculptor.slnx`.

### Lot 4 — Contracts + API + Mapster

- Contracts : `GenerateBootstrapRequest`, `PushBootstrapToGitRequest`,
  `GenerateBootstrapResponse`, `PushBootstrapToGitResponse` (mirroring Pipeline).
- `BootstrapGenerationController.cs` (mirroring `PipelineGenerationController.cs`) : 4 routes
  (generate, download, get-file-content, push-to-git), groupe `Routes.GenerateBootstrap`,
  `RateLimitingPolicyNames.Expensive` sur generate/push, `.ProducesProblem(401)` partout.
- `Routes.cs` : `GenerateBootstrap = "/generate-bootstrap"`.
- `Program.cs` : `app.UseBootstrapGenerationController();`.
- `InfraConfigMappingConfig.cs` : mappings `GenerateBootstrapResult → GenerateBootstrapResponse`,
  `PushBicepToGitResult → PushBootstrapToGitResponse`.

Validation : `dotnet build .\InfraFlowSculptor.slnx` (0 erreur), OpenAPI généré sans warning.

### Lot 5 — Front service + interfaces

- `shared/interfaces/bootstrap-generator.interface.ts` (mirroring pipeline).
- `shared/services/bootstrap-generator.service.ts` extends `BaseGeneratorService`, basePath
  `/generate-bootstrap`.

Validation : `npm run typecheck`.

### Lot 6 — Front onglet Bootstrap + push dédié + i18n

- `config-detail-generation-section.controller.ts` / `.view-model.ts` : signaux + actions
  bootstrap (mirroring bicep/pipeline), `generateAll` inclut désormais bootstrap.
- `config-detail-generation-section.component.ts/.html` : 3ᵉ onglet `TAB_BOOTSTRAP` (icône
  `rocket_launch`), intègre `<app-bootstrap-setup-guide />` existant, bouton « Push to Git »
  dédié posant `isBootstrap: true` (volontairement séparé du bouton d'en-tête ambigu — voir
  défaut D12 adjacent ci-dessus).
- `config-detail-tree.helpers.ts` : `buildConfigBootstrapNodes` (fichier unique).
- `push-to-git-dialog.component.ts` : injecte `BootstrapGeneratorService`, branche
  `isBootstrap` dans `pushConfigLevelArtifactsToGit`.
- i18n `en.json`/`fr.json` : `CONFIG_DETAIL.GENERATION.TAB_BOOTSTRAP` +
  `CONFIG_DETAIL.BOOTSTRAP.*` (chemin imbriqué complet, mirroring `CONFIG_DETAIL.PIPELINE.*`).
- Exposition strictement conditionnée à `isProjectMultiRepo()`, comme F11b.

Validation : `npm run typecheck && npm run build`. Tests spec mis à jour.

### Lot 7 — Validation

- `dotnet build .\InfraFlowSculptor.slnx` (0 erreur).
- `dotnet test .\InfraFlowSculptor.slnx` — comparer au nombre d'échecs baseline (51 au
  2026-08-28), aucun nouvel échec toléré.
- `npm run typecheck && npm run build` (depuis `src\Front`).
- Revalidation en conditions réelles contre un vrai projet MultiRepo (génération + push
  bootstrap niveau config), comme prévu à l'étape 4 de `NEXT.md` pour D01.

## Points à reporter en mémoire projet en fin de chantier

- `.claude/memory/07-bicep-generation.md`, section « Multi-Repo Git Routing » : bootstrap suit
  désormais le même patron à deux niveaux que Bicep/Pipeline.
- `.claude/memory/12-api-endpoints.md` : 4 nouvelles routes `/generate-bootstrap*`.
- `.claude/memory/04-cqrs-pattern.md`, section « Shared Handler Extraction With Leverage » :
  `IProjectBootstrapDefinitionBuilder` appelé aux deux niveaux avec une liste de configs de
  taille variable.
- `feature-map.md` : ajouter le défaut D12 (bouton Push to Git unique n'active jamais
  `isPipeline`).
