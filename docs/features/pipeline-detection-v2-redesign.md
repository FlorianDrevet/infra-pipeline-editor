# Pipeline Detection v2 — Design de refonte

> **Statut :** Proposition d'architecte — à valider avant exécution
> **Auteur :** `architect`
> **Cible :** Refonte de la détection automatique de stack + des frameworks de tests + UX progression, sans casser l'existant
> **Fichiers de référence :**
> - Service actuel : [PipelineOptionDetectionService.cs](src/Api/InfraFlowSculptor.Infrastructure/Services/PipelineDetection/PipelineOptionDetectionService.cs)
> - Contrat actuel : [DetectedPipelineOptionsResponse.cs](src/Api/InfraFlowSculptor.Contracts/Common/DetectedPipelineOptionsResponse.cs)
> - Handler : [DetectPipelineOptionsQueryHandler.cs](src/Api/InfraFlowSculptor.Application/Common/Queries/DetectPipelineOptions/DetectPipelineOptionsQueryHandler.cs)
> - Endpoint : [PipelineOptionDetectionController.cs](src/Api/InfraFlowSculptor.Api/Controllers/PipelineOptionDetectionController.cs)
> - Composant : [pipeline-options.component.ts](src/Front/src/app/features/resource-edit/components/pipeline-options/pipeline-options.component.ts)
> - Modèle front : [pipeline-step-options.model.ts](src/Front/src/app/features/resource-edit/models/pipeline-step-options.model.ts)

---

## A. Compréhension de la feature

### A.1 Ce que l'utilisateur pourra faire après refonte

1. Sélectionner la stack applicative et obtenir **automatiquement** la liste des frameworks de tests/lint/coverage **compatibles** (plus de champs texte libres incohérents).
2. Lancer une détection automatique qui analyse **réellement** le repo : multi-stack, multi-framework, monorepo, présence Docker / CI existante.
3. Voir la progression en temps réel dans un overlay non bloquant (étapes, sous-étapes, signaux trouvés), pouvoir **annuler** à tout moment.
4. Recevoir des **résultats structurés avec scores de confiance** : "xUnit (confiance 0.95, signal: 12 .Tests.csproj + PackageReference)", et arbitrer en cas de conflit via une modale dédiée.
5. Re-lancer la détection à volonté (idempotent) et garder un **historique** des runs pour audit.
6. Construire un pipeline qui exploite intelligemment la détection multi-composant : un projet `backend .NET + frontend Angular` produit un pipeline avec deux jobs de tests, sans étapes incohérentes.

### A.2 User journey concret

**Avant (état actuel)**
```
1. Utilisateur ouvre l'écran "Pipeline Options" d'une Container App.
2. Sélectionne ".NET" dans Stack.
3. Tape "xUnit" dans Framework de test (champ libre, pas de validation).
4. Clique "Détection automatique" → freeze UI ~10s, pas de feedback.
5. Reçoit un objet plat : `{ testFramework: "xunit", suggestedTestCommand: "dotnet test...", lintingAvailable: true }`.
6. Aucune indication de confiance, aucun moyen de voir d'où vient l'info, pas de gestion multi-projet.
7. Si le repo est mono-repo (.NET + Angular), seul le premier framework détecté gagne.
```

**Après (cible)**
```
1. Utilisateur clique "Détection automatique" → overlay s'ouvre avec timeline.
2. Étape 1: "Analyse du repo" (résultat: 42 fichiers, mono-repo Nx détecté).
3. Étape 2: "Détection des stacks" (résultat: .NET 10 + Angular 21, confiance 0.95 chacune).
4. Étape 3: "Détection des frameworks de tests" (xUnit confiance 0.95, Jasmine confiance 0.85).
5. Étape 4: "Détection outillage qualité" (sonar-project.properties trouvé, ESLint actif, dotnet format).
6. Si conflit (2 frameworks .NET trouvés) → modale "Lequel utiliser ?".
7. Utilisateur clique "Appliquer" → la config se peuple : 2 composants détectés, chacun avec son profile typé.
8. Sélecteur "Framework de tests .NET" propose [xUnit, NUnit, MSTest] (filtré, pas libre).
```

### A.3 Ce qui change fonctionnellement

| Aspect | Avant | Après |
|---|---|---|
| Stacks supportées simultanément | 1 (switch) | N (multi-stack) |
| Frameworks détectés | 1 par stack | N par composant + scoring |
| Champs front | Libres ou semi-typés | Strictement contextualisés par stack |
| Mode d'appel | Synchrone, bloquant | Async avec progression observable |
| Annulation | Non | Oui (CancellationToken propagé) |
| Historique | Aucun | `DetectionRun` persistés |
| Source de vérité catalogues | Dupliquée (back + front) | Unique côté Domain, projetée vers le front |
| Mono-repo | Ignoré | Détecté (Nx/Turbo/Lerna/dotnet sln multi-projet) |

---

## B. Analyse critique de l'existant

### B.1 Limitations concrètes du service actuel

Référence : [PipelineOptionDetectionService.cs](src/Api/InfraFlowSculptor.Infrastructure/Services/PipelineDetection/PipelineOptionDetectionService.cs)

1. **Switch monolithique sur `runtimeStack`** (lignes ~38-46) : un seul stack à la fois, choix exclusif. Impossible de gérer `.NET + Angular` simultanément.
2. **`string.Contains` brute** (lignes ~278-287, ~155-184) : `csprojContent.Contains("xunit")` matche aussi un commentaire `<!-- migration from xunit -->`. Aucune analyse XML/JSON/TOML structurée.
3. **Le premier fichier gagne** (lignes ~67-75) : `testProjects[0]` est lu en aveugle. Si le repo a `Foo.Tests.csproj` (xUnit) et `Bar.IntegrationTests.csproj` (NUnit), le second est ignoré.
4. **Pas de scoring** : `TestFramework` est `string?`. Aucune notion de confiance, aucune trace des signaux ayant conduit à la conclusion.
5. **Mapping stack → outils figé** dans le code : `SuggestedDependencyScanTool = "OWASPDependencyCheck"` pour .NET (ligne ~123), `"NpmAudit"` pour Node (ligne ~203). Ces règles sont enterrées dans les méthodes privées et non testables indépendamment.
6. **Cas "Angular" mappé sur "Node"** (ligne ~43) : `"Node" or "NodeJs" or "Angular"` route au même détecteur Node, ignorant les spécificités Angular (Karma, Jasmine, `angular.json`, `ng test`).
7. **Couplage à `IGitProviderService`** : le service refait `SearchFilesAsync` + `GetFileContentAsync` une fois par stack, sans cache. Sur un gros repo, c'est N+1 sur l'API GitHub/Azure DevOps.
8. **Pas de cancellation effective** : `cancellationToken` est passé aux appels git mais aucune logique de bail-out entre les étapes.

### B.2 Limitations côté contrat

Référence : [DetectedPipelineOptionsResponse.cs](src/Api/InfraFlowSculptor.Contracts/Common/DetectedPipelineOptionsResponse.cs)

- DTO **plat single-framework** : un seul `TestFramework`, une seule `SuggestedTestCommand`, etc.
- Aucun champ `Confidence`, `DetectedComponents`, `DetectionSignals`.
- Pas de versioning : impossible de faire évoluer le schéma sans casser les consommateurs.
- Le front a déjà un modèle plus riche ([pipeline-step-options.model.ts](src/Front/src/app/features/resource-edit/models/pipeline-step-options.model.ts) — `PipelineStackProfile` discriminated union), donc le contrat backend est **en retard** sur le besoin déjà exprimé côté UI.

### B.3 Limitations côté UX

- Endpoint `GET /azure-resources/{resourceId}/detect-pipeline-options` ([PipelineOptionDetectionController.cs](src/Api/InfraFlowSculptor.Api/Controllers/PipelineOptionDetectionController.cs)) : synchrone, pas de progression possible.
- Frontend (`onAutoDetect()` dans le composant) émet un simple événement, l'orchestration appelle l'API et attend. Aucun overlay, aucune timeline, aucune annulation.

### B.4 Dettes techniques identifiées

1. **Duplication des catalogues** : `DOTNET_TEST_FRAMEWORK_OPTIONS`, `NODE_TEST_FRAMEWORK_OPTIONS`... sont déclarés en dur côté front ([pipeline-step-options.model.ts](src/Front/src/app/features/resource-edit/models/pipeline-step-options.model.ts) lignes ~80-118), et les valeurs analogues sont en dur dans le service back (`"xunit"`, `"jest"`, `"vitest"`). Aucune source de vérité unique.
2. **Convention de casing incohérente** : back émet `"xunit"`, `"jest"` (lowercase). Front utilise `"XUnit"`, `"Jest"` (PascalCase) dans `DOTNET_TEST_FRAMEWORK_OPTIONS`. Le mapping silencieux est cassé : un résultat back ne sélectionnera pas l'option front.
3. **Le handler `DetectPipelineOptionsQueryHandler`** descend toute la chaîne `WebApp → ResourceGroup → InfraConfig → Project` pour récupérer un PAT. Cette logique sera dupliquée pour la v2 si on n'extrait pas un `IRepositoryAccessResolver`.
4. **Pas de tests** de bout en bout sur la détection : seulement le service unitaire dans [PipelineOptionDetectionServiceTests.cs](tests/InfraFlowSculptor.Infrastructure.Tests/Services/PipelineDetection/PipelineOptionDetectionServiceTests.cs).

### B.5 Pourquoi le modèle plat single-framework ne tient pas

Un projet réel typique : monorepo `Backend (.NET 10, xUnit + integration tests NUnit) + Frontend (Angular 21, Jasmine + tests E2E Playwright) + Worker (Python, pytest)`. Le DTO actuel oblige à choisir **un seul** framework de tests pour tout le pipeline. La conséquence : le pipeline généré ne couvre qu'un sous-ensemble du repo, et l'utilisateur doit éditer à la main des étapes pour les autres composants — ce qui annule l'intérêt de la détection automatique.

Le passage à un modèle `DetectedComponent[]` (chaque composant ayant son `Stack`, son `RootPath`, ses `TestFrameworks[]` avec scores) est la **seule** façon de représenter fidèlement la réalité.

---

## C. Architecture cible

### C.1 Modèle de données (Domain)

#### C.1.1 Choix de pattern

**Patterns envisagés :**
- **Option A — Tout en Application (services + DTOs)** : pas d'agrégat, simple flow service → DB. Rejeté : on perd l'invariant "un DetectionRun appartient à une AzureResource et a un statut séquentiel", et la persistance devient ad-hoc.
- **Option B — `DetectionRun` comme petit agrégat dédié** (retenu) : aggregate root encapsulant le run, ses étapes, ses signaux, son résultat. Justifié par : invariants de transition (Pending → Running → Completed/Failed/Cancelled), entités enfants (`DetectionStep`, `DetectedComponent`, `DetectionSignal`), historique requis.
- **Option C — Étendre `AzureResource`** : ajouter `LastDetection` directement sur l'agrégat compute. Rejeté : pollution sémantique (la détection n'est pas un invariant métier de la ressource), et empêche l'historique.

**Décision : Option B.** Un nouvel agrégat `PipelineDetectionRunAggregate` dans le Domain, avec son repository et son DbContext set.

#### C.1.2 Structure

```
PipelineDetectionRunAggregate (sealed, root)
├── Id: PipelineDetectionRunId
├── ResourceId: AzureResourceId            // FK soft (pas de navigation)
├── ProjectId: ProjectId                    // dénormalisé pour query
├── Status: DetectionRunStatus              // smart enum: Pending|Running|Completed|Failed|Cancelled|PartiallyCompleted
├── RulesVersion: int                       // versioning du moteur (cf. C.2)
├── StartedAt / CompletedAt
├── CancellationReason: string?
├── Steps: IReadOnlyList<DetectionStep>     // owned entities
├── DetectedComponents: IReadOnlyList<DetectedComponent>  // owned
└── TopologyHint: RepoTopology              // value object (Nx, Turbo, DotnetSolution, FlatRepo, Unknown)

DetectionStep (owned)
├── Order: int
├── Kind: DetectionStepKind                 // smart enum (RepoScan, StackDetection, TestFrameworkDetection, QualityToolsDetection, Aggregation)
├── Status: DetectionStepStatus             // Pending|Running|Succeeded|Failed|Skipped
├── StartedAt / CompletedAt
└── Diagnostics: IReadOnlyList<DetectionDiagnostic>  // warnings / infos

DetectedComponent (owned)
├── ComponentId: Guid                       // stable pour referencing en UI
├── Stack: ApplicationStack                 // value object existant réutilisé
├── RootPath: string                        // ex. "src/Api/" pour mono-repo
├── ConfidenceScore: ConfidenceScore        // value object: double [0..1] + Level (Low/Medium/High)
├── TestFrameworks: IReadOnlyList<DetectedTestFramework>
├── QualitySignals: IReadOnlyList<DetectedQualityTool>  // linting, sonar, dependency scan
└── BuildArtifacts: IReadOnlyList<DetectedBuildArtifact> // Dockerfile, CI files, etc.

DetectedTestFramework (owned)
├── FrameworkKey: TestFrameworkKey          // value object typé fort: { Stack, Framework } (ex. DotNet/XUnit)
├── Confidence: ConfidenceScore
├── SuggestedCommand: string?
├── ResultsFormat: TestResultsFormat        // smart enum
├── CoverageTool: CoverageTool?             // smart enum
└── Signals: IReadOnlyList<DetectionSignal>  // owned, audit trail

DetectionSignal (owned)
├── Source: SignalSource                    // FilePresence | FileContent | DirectoryStructure | ManifestParse
├── FilePath: string?
├── Pattern: string?                        // ce qui a matché (ex. "<PackageReference Include='xunit'>")
└── Weight: double                          // contribution au score
```

**Règles métier portées par l'agrégat** :
- Une transition de statut invalide → erreur ErrorOr (`Errors.PipelineDetection.InvalidTransition`).
- L'ajout d'un `DetectedComponent` exige un statut `Running`.
- `MarkCompleted` exige au moins un step succeeded.
- `MarkPartiallyCompleted` si certains steps ont failed mais qu'on a au moins un composant.

#### C.1.3 Réutilisation existante

- `ApplicationStack` (value object existant côté `Domain.Common.OwnedEntities.Stacks`) est réutilisé tel quel — étendu si besoin pour les nouveaux stacks futurs.
- `AzureResourceId`, `ProjectId` : valeurs existantes.

### C.2 Source de vérité des compatibilités stack ↔ framework

#### C.2.1 Patterns envisagés

- **Option A — Constantes en dur côté Domain code** : `TestFrameworkCatalog.cs` statique. Avantage : compile-time safety. Inconvénient : recompilation pour ajouter un framework.
- **Option B — Table EF Core `test_framework_catalog`** : seed initial, admin peut ajouter. Avantage : extensibilité dynamique. Inconvénient : I/O DB pour un catalogue qui change tous les 6 mois, complexité de cache.
- **Option C — Fichier JSON versionné dans le repo** : `src/Api/InfraFlowSculptor.Infrastructure/Catalogs/test-frameworks.v1.json`, lu au démarrage et exposé via service. Avantage : éditable hors code, versionnable Git. Inconvénient : un peu de plomberie.
- **Option D — Hybride Domain code + extension future** (retenu) : catalogue **en code** sous forme de classes immutables strongly-typed dans `InfraFlowSculptor.Domain.PipelineDetection.Catalogs`, exposé via `ITestFrameworkCatalog` (Application) qui peut **plus tard** être backé par DB ou JSON sans casser les consommateurs.

**Justification de D** : le catalogue change peu (xUnit/NUnit/MSTest sont stables depuis 10 ans). Un type-safe code-first donne compile-time safety, IntelliSense, refactoring sûr. La porte est laissée ouverte via l'interface pour migrer vers JSON/DB si un besoin métier d'admin dynamique apparaît.

#### C.2.2 Structure du catalogue

```
ITestFrameworkCatalog (Application/Common/Interfaces/Catalogs)
├── GetFrameworksForStack(ApplicationStack): IReadOnlyList<TestFrameworkDefinition>
├── GetById(TestFrameworkKey): TestFrameworkDefinition?
└── AllSupportedStacks(): IReadOnlyList<ApplicationStack>

TestFrameworkDefinition (record sealed, Domain)
├── Key: TestFrameworkKey (ex. { Stack: DotNet, Framework: "XUnit" })
├── DisplayName: string
├── DefaultCommand: string
├── DefaultResultsFormat: TestResultsFormat
├── DefaultCoverageTool: CoverageTool
├── DefaultCoverageReportGlob: string
├── DetectionHints: IReadOnlyList<DetectionHint>    // patterns à matcher pour scorer
└── i18nKey: string                                  // pour le front
```

**Un seul fichier par stack** dans `Domain/PipelineDetection/Catalogs/Stacks/` :
- `DotNetTestFrameworks.cs` (XUnit, NUnit, MSTest)
- `NodeJsTestFrameworks.cs` (Jest, Vitest, Mocha, Playwright)
- `AngularTestFrameworks.cs` (Jasmine/Karma, Jest, Vitest)
- `JavaTestFrameworks.cs` (JUnit5, JUnit4, TestNG)
- `PythonTestFrameworks.cs` (pytest, unittest)

**Ajouter Rust demain** = 1 fichier `RustTestFrameworks.cs` + ajouter `Rust` à `ApplicationStack`. Zéro modification ailleurs.

#### C.2.3 Projection vers le front

Endpoint dédié `GET /api/catalogs/test-frameworks` exposant le catalogue typé (cachable). Le front consomme un service Angular `TestFrameworkCatalogService` qui hydrate un `Signal<TestFrameworkDefinition[]>` au démarrage. Les composants UI dérivent les options via `computed()`. **Disparition** des constantes hard-codées `DOTNET_TEST_FRAMEWORK_OPTIONS` & co.

### C.3 Moteur de détection (Application/Infrastructure)

#### C.3.1 Pattern retenu : Pipeline de détecteurs + Agrégateur

**Patterns envisagés :**
- **Chain of Responsibility pure** : chaque détecteur passe au suivant. Rejeté : couplage séquentiel rigide, difficile à paralléliser.
- **Strategy unique par stack** : un détecteur par stack. Rejeté : ne résout pas le multi-stack.
- **Pipeline de détecteurs composables + Aggregator** (retenu) : chaque détecteur est indépendant, produit des `DetectionSignal[]`, et un agrégateur final consolide en `DetectedComponent[]` avec scoring.

#### C.3.2 Architecture du moteur

```
IDetectionEngine (Application)
└── RunAsync(DetectionContext, IProgress<DetectionProgress>, CancellationToken): Task<ErrorOr<DetectionRunResult>>

DetectionContext (Application, immutable)
├── RepositorySnapshot                      // résultat d'un seul SearchFilesAsync, partagé entre détecteurs (élimine N+1)
├── ScopePrefix
└── Hints                                   // hints utilisateurs optionnels (force stack, exclude path)

Détecteurs (Application, contracts) — chacun pure, testable, sans état
├── IRepoTopologyDetector                   // détecte Nx, Turbo, Lerna, .sln multi-projet, monorepo flat
├── IStackDetector                          // une implémentation par stack (DotNetStackDetector, AngularStackDetector...)
├── ITestFrameworkDetector                  // une implémentation par stack
├── IQualityToolsDetector                   // ESLint, dotnet format, ruff, checkstyle, sonar-project.properties
└── IBuildArtifactDetector                  // Dockerfile, .github/workflows, azure-pipelines.yml

DetectionAggregator (Infrastructure)
└── Reçoit IReadOnlyList<DetectionSignal>, produit IReadOnlyList<DetectedComponent>
    Règles de scoring : voir C.3.3
```

Le moteur exécute le pipeline ainsi :
1. **RepoScan** : un seul appel `SearchFilesAsync` → `RepositorySnapshot` immuable réutilisé partout.
2. **Topology** : `IRepoTopologyDetector` examine la racine (`nx.json`, `turbo.json`, `*.sln`, `lerna.json`).
3. **Stack detection** : tous les `IStackDetector` exécutés en parallèle (chacun retourne `IReadOnlyList<StackCandidate>` avec root path + score).
4. **Per-component, per-stack detection** : pour chaque composant détecté, on exécute en parallèle les détecteurs `ITestFrameworkDetector` / `IQualityToolsDetector` correspondants.
5. **Aggregation** : `DetectionAggregator` consolide les signaux, applique le scoring, élimine les doublons, ranke.
6. **Progress reporting** : à chaque étape, un `IProgress<DetectionProgress>` est notifié (typé fort : `{ StepKind, Index, Total, Message, SignalsFound }`).

#### C.3.3 Scoring de confiance

Pour chaque framework détecté dans un composant :
```
confidence = sigmoid(Σ signal.weight)  borné [0..1]
```
Exemples de poids (immutables, définis sur chaque `DetectionHint`) :
- `PackageReference Include="xunit"` dans `.csproj` : **+3** (signal très fort)
- Fichier `.Tests.csproj` contenant `xunit` : **+2**
- Présence d'un `using Xunit;` dans `.cs` : **+1**
- Pas de signal contraire : **0**
- Présence d'un signal contraire (`using NUnit.Framework;` dans le même projet) : **-2**

Le score est exposé en `ConfidenceLevel` (`Low` < 0.5, `Medium` < 0.8, `High` ≥ 0.8) pour l'UI.

#### C.3.4 Gestion des conflits

Si deux frameworks ont `High` confidence sur le même composant (ex. xUnit ET NUnit dans le même `.csproj`), l'agrégateur :
1. Garde les deux dans `DetectedComponent.TestFrameworks`.
2. Marque un `DetectionDiagnostic` de niveau `ConflictRequiresUserDecision`.
3. Le front affiche une modale "Quel framework prioriser ?".

### C.4 Contrats API

#### C.4.1 Patterns envisagés pour le transport de progression

- **A. SignalR** : déjà compatible avec Aspire, push natif, idéal pour UX riche. Coût : configuration cluster pour scale-out.
- **B. SSE (Server-Sent Events)** : plus léger, unidirectionnel server → client, supporté nativement par ASP.NET Core 10.
- **C. Long polling** : simple, dégradé.

**Décision : SSE (Option B) en v1, SignalR si besoin futur de bidirectionnel.**
Justification : SSE suffit (la progression est unidirectionnelle), zéro dépendance supplémentaire, debug trivial dans DevTools, compatible Aspire/proxy/L7 sans config. Si plus tard on veut "pause/resume" ou interaction utilisateur en cours de run, on migre vers SignalR.

#### C.4.2 Nouveaux endpoints

```
POST   /api/azure-resources/{resourceId}/detection-runs
       → 202 Accepted, body { runId, statusUrl, streamUrl, cancelUrl }
       Lance un nouveau run, retourne immédiatement.

GET    /api/detection-runs/{runId}/stream  (SSE, text/event-stream)
       Stream des événements de progression : step-started, step-completed, signal-found, run-completed, run-failed.

GET    /api/detection-runs/{runId}
       Récupère l'état + résultat complet (utilisé en fallback si SSE échoue / pour replay).

POST   /api/detection-runs/{runId}/cancel
       → 204 No Content. Annule via CancellationTokenSource côté serveur.

GET    /api/azure-resources/{resourceId}/detection-runs?limit=10
       Historique paginé des runs (audit).

GET    /api/catalogs/test-frameworks
       Catalogue typé, cachable (ETag).
```

**L'endpoint GET legacy `/azure-resources/{resourceId}/detect-pipeline-options` est conservé** en v1 pour backward compat (cf. stratégie de migration C.6 et E).

#### C.4.3 DTOs principaux

```
DetectionRunResponse (Contracts)
├── RunId: string (Guid)
├── ResourceId: string
├── Status: string (smart enum value)
├── StartedAt / CompletedAt
├── Steps: DetectionStepResponse[]
├── Components: DetectedComponentResponse[]
├── Topology: string
└── RulesVersion: int

DetectedComponentResponse
├── ComponentId: string
├── Stack: string
├── RootPath: string
├── Confidence: ConfidenceResponse { Score: double, Level: string }
├── TestFrameworks: DetectedTestFrameworkResponse[]
├── QualitySignals: DetectedQualityToolResponse[]
└── BuildArtifacts: DetectedBuildArtifactResponse[]

DetectedTestFrameworkResponse
├── Stack: string
├── Framework: string                       // PascalCase aligné avec le front
├── Confidence: ConfidenceResponse
├── SuggestedCommand: string
├── ResultsFormat: string
├── CoverageTool: string?
└── Signals: DetectionSignalResponse[]      // pour transparence & debug
```

**Un fichier par DTO** dans `InfraFlowSculptor.Contracts/PipelineDetection/`. Pas de poubelle.

### C.5 Persistance

#### C.5.1 Schéma EF Core

Nouveau DbSet `pipeline_detection_runs` dans le DbContext existant ([qui ?]). Owned types pour `Steps`, `DetectedComponents`, `DetectionSignals` (split en tables filles via owned navigation collections, EF Core 10 supporté).

Tables :
- `pipeline_detection_runs` (root)
- `pipeline_detection_steps` (owned, FK runId)
- `pipeline_detected_components` (owned, FK runId)
- `pipeline_detected_test_frameworks` (owned, FK componentId)
- `pipeline_detection_signals` (owned, FK frameworkId — ou componentId selon granularité finale)

**Indexes** : `(ResourceId, StartedAt DESC)` pour récupérer le dernier run rapidement, `(ProjectId)` pour les requêtes admin.

**Cascade delete** : Si une `AzureResource` est supprimée, les runs associés cascade (FK soft sur `ResourceId`, application-level cleanup ou ON DELETE CASCADE selon politique projet).

**Migration EF** : `dotnet ef migrations add AddPipelineDetectionRuns`.

#### C.5.2 Faut-il stocker les runs ?

**Oui**, pour 4 raisons :
1. **Audit** : trace de "pourquoi le pipeline généré contient telle étape" (les `DetectionSignal` répondent).
2. **Replay** : re-exécuter une génération de pipeline sans re-scanner le repo.
3. **Idempotence UX** : "voir la dernière détection" sans relancer.
4. **Téléchargement** futur des rapports de détection.

**Stratégie de rétention** : garder les 10 derniers runs par `ResourceId` (cleanup en background job hors scope v1).

### C.6 Génération pipeline en aval

Le générateur de pipeline (Azure DevOps YAML / GitHub Actions / etc.) consomme actuellement les flags plats du `PipelineStepOptions`. **Migration en deux temps** :

1. **Phase A (compat)** : adapter `PipelineGeneration` pour accepter, en plus des flags existants, un `DetectionRunResult?` optionnel. Si présent, il génère **un job par composant détecté** avec les commandes du framework détecté.
2. **Phase B (cible)** : `PipelineStepOptions` lui-même devient multi-composant — le `ContainerApp.PipelineStepOptions` actuel évolue vers une liste de `ComponentPipelineOptions`. Migration EF + Mapster.

La phase A permet de **livrer la valeur tôt** sans casser les pipelines déjà générés.

---

## D. Architecture frontend

### D.1 Composants Angular (DS-first OBLIGATOIRE)

**Référence DS-first** : tout pattern visuel doit utiliser les `app-ds-*` existants (cf. règle 13 de `copilot-instructions.md`).

#### D.1.1 Nouveau composant `pipeline-detection-overlay`

- Base : `app-ds-dialog` (modale plein écran ou large, non bloquante au sens où l'utilisateur peut annuler).
- Contenu : timeline verticale des steps (`app-ds-stepper` si existe, sinon composer avec `app-ds-card` + `app-ds-progress`).
- Pied : `app-ds-button` "Annuler" en `tertiary`, `app-ds-button` "Voir résultats" en `primary` (disabled tant que pas completed).
- Si aucun composant DS ne couvre `stepper` ou `progress`, créer un `app-ds-stepper` / `app-ds-progress` réutilisable dans `shared/components/ds/` avant d'écrire l'overlay (règle 13, exception interdite hors mémoire).

#### D.1.2 Sélecteur de framework contextualisé

- Composant `test-framework-picker` (standalone, signal-based, OnPush).
- Inputs : `stack: InputSignal<ApplicationStack | null>`, `selectedFrameworkKey: ModelSignal<TestFrameworkKey | null>`, `detection?: InputSignal<DetectedTestFramework[] | null>`.
- Internals : consomme `TestFrameworkCatalogService` (cf. C.2.3), filtre via `computed()` les options pour la stack courante.
- Visuel : `app-ds-autocomplete` (ou `app-ds-select`) avec chips `app-ds-chip` montrant le score de confiance si détecté ("xUnit — Détecté, confiance élevée").

#### D.1.3 Modale de résolution de conflit

- `app-ds-dialog` modale.
- Liste verticale des frameworks en conflit (chacun avec son score + ses signaux résumés).
- `app-ds-radio-group` ou `app-ds-card` cliquables pour choisir.
- Si aucun composant DS radio-group n'existe : créer `app-ds-radio-group` réutilisable dans `shared/components/ds/`.

#### D.1.4 Réutilisation

- `app-ds-tabs` pour grouper les composants détectés par stack dans le résumé final.
- `app-ds-chip` pour les frameworks et leur niveau de confiance (couleurs DS : success/warn/error pour High/Medium/Low).
- `app-ds-toast` pour notifier "Détection terminée" / "Détection annulée".

### D.2 State management (Signals)

#### D.2.1 Service `PipelineDetectionStore`

```
PipelineDetectionStore (injectable, signal-based)
├── state: Signal<DetectionState>
├── start(resourceId): void
├── cancel(): void
├── retry(): void
└── reset(): void

type DetectionState =
  | { kind: 'Idle' }
  | { kind: 'Starting', resourceId: string }
  | { kind: 'Running', runId: string, progress: DetectionProgress, components: DetectedComponent[] }
  | { kind: 'PartialSuccess', runId: string, components: DetectedComponent[], warnings: Diagnostic[] }
  | { kind: 'Completed', runId: string, components: DetectedComponent[] }
  | { kind: 'Failed', runId: string, error: ErrorInfo }
  | { kind: 'Cancelled', runId: string }
```

Tagged-union strict → exhaustive matching dans le template via `@switch` Angular.

#### D.2.2 Stratégie de subscription

- À l'ouverture de l'overlay, `start()` POST l'endpoint, récupère `streamUrl`, ouvre un `EventSource` (SSE).
- Chaque événement met à jour le `Signal` via `update()`.
- Si SSE échoue (proxy, réseau), fallback **automatique** : polling `GET /detection-runs/{runId}` toutes les 2s avec backoff.
- `cancel()` : appelle `POST /detection-runs/{runId}/cancel`, ferme le SSE, transitionne vers `Cancelled`.
- L'`EventSource` est nettoyé via `DestroyRef` / `effect` cleanup.

### D.3 UX détaillée

#### D.3.1 Wireframe textuel — Overlay de progression

```
┌────────────────────────────────────────────────────────────────┐
│  Détection automatique de pipeline                       [×]   │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│  Repository : github.com/acme/billing-platform (branche main)  │
│  Lancée à 14:23:01 — Run #r-9f3c                               │
│                                                                │
│  ◉━━━ Analyse du repository .............. ✓ 42 fichiers     │
│   │                                                            │
│  ◉━━━ Détection des stacks ............... ✓ 2 détectées     │
│   │       • .NET 10 (src/Api/, confiance élevée)               │
│   │       • Angular 21 (src/Front/, confiance élevée)          │
│   │                                                            │
│  ◉━━━ Détection des frameworks de tests . ⟳ en cours…        │
│   │       • Analyse src/Api/...                                │
│   │                                                            │
│  ○━━━ Détection outillage qualité ........ en attente          │
│   │                                                            │
│  ○━━━ Consolidation des résultats ........ en attente          │
│                                                                │
│  ┌────────────────────────────────────────────────────────┐    │
│  │ ⚠ 1 conflit potentiel détecté — sera proposé à la fin │    │
│  └────────────────────────────────────────────────────────┘    │
│                                                                │
├────────────────────────────────────────────────────────────────┤
│                                  [Annuler]    [Voir résultats] │
└────────────────────────────────────────────────────────────────┘
```

#### D.3.2 Wireframe — Modale de résolution de conflit

```
┌─────────────────────────────────────────────────────────────┐
│ Conflit de détection : src/Api/                       [×]   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ Plusieurs frameworks de tests détectés. Lequel utiliser ?   │
│                                                             │
│ ◉ xUnit                              [confiance: 0.92] ★★★ │
│   Signaux : PackageReference (3), namespace Xunit (8)       │
│                                                             │
│ ○ NUnit                              [confiance: 0.81] ★★☆ │
│   Signaux : PackageReference (1), Setup attribute (2)       │
│                                                             │
│ ○ Aucun (configurer manuellement)                           │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│                                            [Plus tard] [OK] │
└─────────────────────────────────────────────────────────────┘
```

#### D.3.3 Accessibilité

- **Focus trap** : overlay dialog → cycle Tab dans `[Annuler, Voir résultats, ×]`, blocage hors modale.
- **ARIA live region** (`aria-live="polite"`) sur la zone de progression pour annoncer les changements de step aux lecteurs d'écran.
- **Escape** : déclenche `cancel()` après confirmation (`app-ds-dialog` natif).
- **Reduced motion** : respecter `prefers-reduced-motion` pour les animations de transition de step.
- **Contraste** : niveaux de confiance via chips DS qui respectent déjà WCAG AA.

#### D.3.4 Perception de performance

- Affichage **immédiat** de l'overlay dès le clic (état `Starting`), sans attendre la réponse POST.
- Skeleton sur la timeline pendant le `Starting`.
- Optimistic display des composants au fil de l'eau (pas d'attente du run complet).
- Délai max perçu visé : aucun, car streaming continu.

---

## E. Plan d'implémentation

### Vue d'ensemble — Découpage en lots

| Lot | Objectif | Quick win ? | Dépendances | Agents |
|---|---|---|---|---|
| **1** | Catalogue de frameworks typé + contextualisation UI (sans toucher au moteur) | **OUI** | aucune | `dotnet-dev` puis `angular-front` |
| **2** | Agrégat `PipelineDetectionRun` + persistance + endpoints async (squelette) | Non | Lot 1 (catalogue) | `dotnet-dev` |
| **3** | Moteur de détection multi-signal + détecteurs unitaires + scoring | Non | Lot 2 | `dotnet-dev` |
| **4** | UX progression (overlay SSE + modale conflit + store) | Non | Lot 2 (endpoints), Lot 3 (sémantique du résultat) | `angular-front` |
| **5** | Génération pipeline exploite multi-composant (Phase A puis B) | Non | Lot 3 | `dotnet-dev` |

### Lot 1 — Catalogue typé + contextualisation UI (**quick win, autonome**)

**Agent : `dotnet-dev` puis `angular-front`. Workflow TDD obligatoire (skill `tdd-workflow`).**

**Backend (étape 1.A)**
- Fichiers à créer :
  - `src/Api/InfraFlowSculptor.Domain/PipelineDetection/Catalogs/TestFrameworkKey.cs` (value object)
  - `src/Api/InfraFlowSculptor.Domain/PipelineDetection/Catalogs/TestFrameworkDefinition.cs`
  - `src/Api/InfraFlowSculptor.Domain/PipelineDetection/Catalogs/Stacks/DotNetTestFrameworks.cs`
  - `...NodeJsTestFrameworks.cs`, `AngularTestFrameworks.cs`, `JavaTestFrameworks.cs`, `PythonTestFrameworks.cs`
  - `src/Api/InfraFlowSculptor.Application/Common/Interfaces/Catalogs/ITestFrameworkCatalog.cs`
  - `src/Api/InfraFlowSculptor.Infrastructure/Catalogs/TestFrameworkCatalog.cs` (impl statique agrégeant les fichiers stack)
  - `src/Api/InfraFlowSculptor.Contracts/Catalogs/TestFrameworkDefinitionResponse.cs`
  - `src/Api/InfraFlowSculptor.Api/Controllers/CatalogController.cs` (`GET /api/catalogs/test-frameworks`)
  - Mapping Mapster + tests xUnit (`TestFrameworkCatalogTests`, `CatalogControllerTests`).
- Critères : catalogue queryable, endpoint répond 200, ETag présent, mapping vérifié, 100% des stacks couvertes.

**Frontend (étape 1.B)**
- Fichiers à créer :
  - `src/Front/src/app/shared/services/test-framework-catalog/test-framework-catalog.service.ts` (hydrate au bootstrap)
  - `src/Front/src/app/shared/services/test-framework-catalog/test-framework-catalog.types.ts`
  - `src/Front/src/app/shared/components/test-framework-picker/test-framework-picker.component.ts` (+ html, scss, spec)
- Fichiers à modifier :
  - `src/Front/src/app/features/resource-edit/components/pipeline-options/pipeline-options.component.html` (remplacer le champ texte `Framework de test` par `<app-test-framework-picker>`)
  - `src/Front/src/app/features/resource-edit/models/pipeline-step-options.model.ts` : **supprimer** les constantes `DOTNET_TEST_FRAMEWORK_OPTIONS`, `NODE_TEST_FRAMEWORK_OPTIONS`, etc. (rendre une seule source).
- Critères : `npm run typecheck && npm run build` verts, le sélecteur affiche uniquement les frameworks compatibles avec la stack sélectionnée, plus aucun champ texte libre pour framework de tests.

**Validation Lot 1**
- `dotnet build .\InfraFlowSculptor.slnx`
- `dotnet test .\tests\InfraFlowSculptor.Application.Tests\... .\tests\InfraFlowSculptor.Api.Tests\...`
- `npm run typecheck && npm run build` (depuis `src/Front`)

**Risques Lot 1**
- Mismatch de casing entre catalogue back (`"XUnit"`) et résultats legacy back (`"xunit"`) : à corriger en passant tout en PascalCase, le legacy étant éphémère.
- Régression UI sur les écrans existants qui consommaient les constantes supprimées.

---

### Lot 2 — Agrégat `PipelineDetectionRun` + endpoints async

**Agent : `dotnet-dev`. Workflow TDD obligatoire.**

**Étapes**
1. Créer l'agrégat sealed `PipelineDetectionRunAggregate` + value objects (`PipelineDetectionRunId`, `ConfidenceScore`, smart enums `DetectionRunStatus`/`DetectionStepKind`/etc.) dans `src/Api/InfraFlowSculptor.Domain/PipelineDetection/`.
2. Créer `IPipelineDetectionRunRepository` (Application) + impl EF Core (Infrastructure) — **sans `SaveChangesAsync` dans le repo** (rappel règle 3 du repo).
3. Migration EF Core `AddPipelineDetectionRuns`.
4. Commands MediatR + handlers + validators :
   - `StartPipelineDetectionRunCommand` → POST endpoint
   - `CancelPipelineDetectionRunCommand` → POST cancel endpoint
   - `GetPipelineDetectionRunQuery` → GET endpoint
   - `GetPipelineDetectionRunsForResourceQuery` → GET historique
5. Implémenter le **mécanisme de cancellation** : `IPipelineDetectionRunCoordinator` singleton qui détient un `ConcurrentDictionary<Guid, CancellationTokenSource>` ; le handler `Start` enregistre, le handler `Cancel` annule.
6. Endpoint SSE `GET /api/detection-runs/{runId}/stream` : utiliser `IAsyncEnumerable<DetectionEvent>` + `Results.Stream` ASP.NET Core 10.
7. Squelette du `IDetectionEngine` qui retourne un résultat factice mais émet de vrais événements (le moteur réel est Lot 3).
8. Endpoint legacy `GET /azure-resources/{resourceId}/detect-pipeline-options` **conservé** : son handler appelle le moteur en mode synchrone (boucle attente du nouveau pipeline) pour ne rien casser pendant la phase de coexistence.

**Critères**
- Tous les endpoints répondent (test d'intégration sur runs factices).
- SSE stream fonctionne en local (test Playwright optionnel).
- Cancellation effective (test e2e).
- Migration EF appliquée, `dotnet ef database update` OK.

**Risques Lot 2**
- SSE derrière reverse proxy : prévoir headers `X-Accel-Buffering: no` + désactiver le buffering Kestrel pour la route.
- Lifetime des `CancellationTokenSource` : leak si jamais on n'annule ni ne complète. Prévoir cleanup `IHostedService` (TTL 1h).

---

### Lot 3 — Moteur de détection multi-signal

**Agent : `dotnet-dev`. Workflow TDD obligatoire.**

**Étapes**
1. Définir tous les contrats (`IRepoTopologyDetector`, `IStackDetector`, `ITestFrameworkDetector`, `IQualityToolsDetector`, `IBuildArtifactDetector`) dans `InfraFlowSculptor.Application/PipelineDetection/Detectors/`.
2. Implémenter le `RepositorySnapshot` (cache des `SearchFilesAsync` + `GetFileContentAsync` par run).
3. Implémenter détecteurs **par stack** dans `InfraFlowSculptor.Infrastructure/PipelineDetection/Detectors/Stacks/`. Un fichier par détecteur (règle 14).
4. Implémenter `DetectionAggregator` (scoring sigmoid, dédup, conflits → diagnostics).
5. Implémenter `IDetectionEngine` réel qui orchestre + reporte via `IProgress<DetectionProgress>` (le coordinator du Lot 2 transforme ce progress en événements SSE).
6. **Remplacer** `PipelineOptionDetectionService` : devient un adapteur qui appelle le nouveau moteur en mode synchrone et applique un projecteur "legacy" → `DetectedPipelineOptionsResult` plat (le premier composant gagne, pour compat).

**Critères**
- Couverture xUnit ≥ 85% sur les détecteurs (cf. skill `xunit-unit-testing`).
- Tests de fixtures réelles : 1 fixture par cas (mono-stack .NET, mono-stack Angular, monorepo .NET+Angular, mono-repo Nx Node, repo vide).
- Mutation testing optionnel sur l'aggregator.
- Performance : 1 run ≤ 5s sur repo 500 fichiers (cf. règle perf).

**Risques Lot 3**
- Parser XML/JSON robuste vs `string.Contains` : prendre `System.Xml.Linq`, `System.Text.Json`, `Tomlyn` (NuGet) pour TOML.
- Faux positifs : un commentaire `<!-- using xunit -->` doit ignorer. Tests négatifs obligatoires.

---

### Lot 4 — UX progression (overlay SSE + store + modale)

**Agent : `angular-front`. Workflow TDD obligatoire (specs Vitest/Jasmine selon le projet).**

**Étapes**
1. Créer `PipelineDetectionStore` (signal-based).
2. Si `app-ds-stepper` / `app-ds-progress` / `app-ds-radio-group` n'existent pas : les créer dans `src/Front/src/app/shared/components/ds/` avec leurs tests.
3. Créer `PipelineDetectionOverlayComponent` (host `app-ds-dialog`).
4. Créer `PipelineDetectionConflictDialogComponent`.
5. Service `PipelineDetectionApiClient` (Axios-based comme le reste du projet, ou Fetch pour SSE).
6. Intégrer dans `PipelineOptionsComponent` : remplacer `onAutoDetect()` qui émettait juste un event → ouvre l'overlay via `MatDialog` (ou pattern DS-équivalent).
7. Branchement i18n via `ngx-translate` (clés sous `PIPELINE_DETECTION.*`).

**Critères**
- Build front vert.
- Spec : cancellation ferme le SSE proprement.
- Spec : fallback polling si SSE 4xx/5xx.
- A11y : tests ARIA via Testing Library.

**Risques Lot 4**
- SSE + Axios = incompatible. Soit utiliser `EventSource` natif, soit `fetch` avec `ReadableStream`. Choisir `EventSource` (plus simple).

---

### Lot 5 — Génération pipeline multi-composant

**Agent : `dotnet-dev`.**

**Phase A** : adapter le générateur existant pour accepter un `DetectionRunResult` optionnel et émettre 1 job par composant.
**Phase B** : faire évoluer `PipelineStepOptions` (Domain) vers une collection `ComponentPipelineOptions`. Migration EF + Mapster + UI. Lot risqué — à isoler.

---

### Stratégie de migration

- **Coexistence** : l'endpoint legacy reste opérationnel jusqu'à la fin du Lot 4. Les pipelines déjà générés ne changent pas tant que Lot 5 n'est pas livré.
- **Feature flag** : non requis. Le nouvel endpoint est sur un autre path (`/detection-runs`), l'ancien continue de répondre.
- **Suppression du legacy** : planifiée après Lot 5, communication users-side (changelog).

---

## F. Edge cases & cas fonctionnels

| Cas | Comportement attendu |
|---|---|
| Plusieurs frameworks détectés dans la même stack | `DetectionDiagnostic.ConflictRequiresUserDecision` → modale UI |
| `csproj` sans `<IsPackable>` ni `<PackageReference>` test | Stack `.NET` détectée (confiance Medium), aucun `DetectedTestFramework` → message "Aucun framework de tests détecté, configurer manuellement" |
| Repo vide | Run complete `PartiallyCompleted` avec diagnostic `EmptyRepository` |
| Repo legacy (que des binaires) | Stack `Unknown`, diagnostic `UnsupportedRepoLayout` |
| Mono-repo Nx | `RepoTopology = Nx`, chaque `apps/*` et `libs/*` devient un composant candidat |
| Mono-repo Turborepo | `RepoTopology = Turborepo`, idem |
| Détection contradictoire (xUnit + NUnit dans le même csproj) | Garde les deux, score abaissé, diagnostic conflit |
| Annulation pendant l'analyse | `CancellationToken` propagé jusqu'aux détecteurs, run passe en `Cancelled`, SSE émet `run-cancelled` puis ferme |
| Timeout réseau sur l'API Git | Step `RepoScan` passe en `Failed`, run passe en `Failed` avec diagnostic `GitTimeout` |
| Token PAT expiré en cours d'analyse | Erreur `Unauthorized` propagée, run `Failed`, diagnostic `TokenExpired`, UI propose lien vers la gestion des secrets |
| Repo très volumineux (>10k fichiers) | `SearchFilesAsync` paginé ; cap configurable `MaxFilesPerRun = 20000` ; diagnostic `RepoTooLarge` au-delà |
| Stack ajoutée sans framework de tests référencé | Composant détecté, `TestFrameworks` vide, diagnostic `NoTestFrameworkForStack` (informatif) |
| Re-détection (idempotence) | Nouveau `DetectionRun` créé, ancien conservé (historique), UI affiche le dernier |
| Branche par défaut différente | `target.Branch` déjà résolu par `IRepositoryTargetResolver`, pas de changement |
| SSE bloqué par proxy | Fallback polling auto côté front sur `GET /detection-runs/{runId}` |

---

## G. Auto-critique

### G.1 Limites de la proposition

1. **Catalogue en code Domain** : ajouter un framework côté admin nécessite encore une PR + déploiement. Si le besoin de configuration dynamique émerge, il faudra migrer vers JSON/DB (l'interface `ITestFrameworkCatalog` rend la migration facile, mais c'est du travail supplémentaire).
2. **Scoring sigmoïde simple** : les poids sont définis empiriquement et peuvent induire en erreur sur des projets atypiques. Pas de ML, pas d'apprentissage des corrections utilisateurs en v1.
3. **SSE non bidirectionnel** : impossible de pousser des questions interactives au cours du run (ex. "On a trouvé Docker mais le Dockerfile semble cassé, on continue ?"). Si besoin futur, migrer vers SignalR.
4. **Persistance des `DetectionRun`** : coût DB non négligeable sur un gros tenant. Cleanup en background hors scope v1.
5. **Pas de parallélisme inter-runs limité** : si 100 utilisateurs lancent simultanément, le pool de coordination doit être borné (à prévoir un `SemaphoreSlim` global).
6. **Détection JavaScript dans des `.vue`, `.svelte`** : non couverte en v1 (pas dans les stacks supportées).

### G.2 Compromis assumés

- **Casing PascalCase** côté contrats : casse l'API legacy `"xunit"` lowercase. Acceptable car la legacy reste mais devient interne / dépréciée.
- **Un seul `RepositorySnapshot` par run** : optimisation prioritaire vs over-engineering caching cross-run.
- **Pas de webhook GitHub/AzDO** pour relancer la détection sur push : la détection reste user-triggered.

### G.3 Simplifications acceptées pour v1

- Pas de "presets" d'utilisateur (sauvegarder ses préférences).
- Pas de diff entre 2 runs ("qu'est-ce qui a changé depuis le dernier scan ?").
- Pas d'export PDF / JSON du rapport de détection.
- Pas de support multi-branche dans le même run.

### G.4 Améliorations futures

- **ML-based detection** : entraîner un modèle léger sur les corrections utilisateurs (signal "j'ai changé le framework détecté" → ajuste les poids).
- **Suggestions communautaires** : agréger les détections anonymisées pour suggérer des configs populaires.
- **Detection scheduled** : re-détection automatique hebdomadaire avec notification si drift.
- **MCP tool** : exposer `infraflowsculptor_run_pipeline_detection` via MCP pour pilotage depuis Copilot/Claude. **Pertinent à terme** mais hors v1.
- **Détection des `Dockerfile`** structurés (multi-stage, base image, taille) → suggérer optimisations de build.

---

## H. Fichier de suivi vivant (obligatoire)

**Chemin du tracker :** `docs/features/pipeline-detection-v2-implementation-tracker.md`

### Statut des lots

| Lot | Contenu | Statut | Owner | Dernière mise à jour | Reste à faire |
|---|---|---|---|---|---|
| Lot 1 | Catalogue typé + UI contextualisée | Not started | — | — | Tout |
| Lot 2 | Agrégat + endpoints async + SSE squelette | Not started | — | — | Tout |
| Lot 3 | Moteur multi-signal + détecteurs + scoring | Not started | — | — | Tout |
| Lot 4 | UX overlay + store + conflit | Not started | — | — | Tout |
| Lot 5 | Génération pipeline multi-composant (A puis B) | Not started | — | — | Tout |

### Règle d'exécution

- Le tracker `pipeline-detection-v2-implementation-tracker.md` doit être créé par `dev` au démarrage du Lot 1 (template ci-dessus) et mis à jour à chaque PR mergée.
- Chaque PR : nom = `Lot N — <titre court>`, lien dans la cellule "Reste à faire" → "Done" après merge.
- Chaque lot : critères de validation (build, tests, typecheck) cochés explicitement dans la PR description.

### Checklist reprise sur un autre PC

1. Cloner le repo, restaurer NuGet (`dotnet restore`), `npm install` depuis `src/Front`.
2. Lire `docs/features/pipeline-detection-v2-redesign.md` (ce document).
3. Lire `docs/features/pipeline-detection-v2-implementation-tracker.md` pour le statut.
4. Identifier le prochain lot non démarré.
5. Charger les skills : `tdd-workflow`, et selon le lot, `dotnet-patterns`/`xunit-unit-testing` ou `angular-patterns`/`ui-ux-front-saas`.
6. Pour chaque symbole modifié : requête Graphify `query`/`explain`/`path`, puis validation par `git diff`, build et tests.

---

## I. Points d'attention pour MEMORY.md

À ajouter dans `.github/memory/` ou `MEMORY.md` à la fin du Lot 1 :

- **Catalogue test frameworks unique** : source unique côté Domain (`InfraFlowSculptor.Domain.PipelineDetection.Catalogs.Stacks.*`), exposée via `ITestFrameworkCatalog`. Les constantes front sont supprimées. Toute extension passe par un nouveau fichier `*TestFrameworks.cs`.
- **Convention casing frameworks** : PascalCase strict bout en bout (`"XUnit"`, `"Vitest"`). Le legacy lowercase est déprécié.
- **Pattern SSE pour progression long-running** : référence implémentation `GET /api/detection-runs/{runId}/stream`. Réutilisable pour future génération pipeline streamée.
- **`IPipelineDetectionRunCoordinator`** : singleton de cancellation runs longs. Référence pour futures opérations cancellables.
- **DS components créés** : `app-ds-stepper`, `app-ds-progress`, `app-ds-radio-group` (si non déjà existants). À déclarer dans la doc DS.
