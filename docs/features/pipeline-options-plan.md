# Plan : Options modulaires de pipeline applicatif

> **Objectif :** Permettre à chaque ressource compute (WebApp, FunctionApp, ContainerApp) de configurer indépendamment des étapes optionnelles dans ses pipelines CI/CD générés (tests unitaires, analyse Sonar, couverture de code, linting, etc.), tout en gardant un système de templates partagés génériques.

---

## Table des matières

1. [Vue d'ensemble](#1-vue-densemble)
2. [Catalogue des options de pipeline](#2-catalogue-des-options-de-pipeline)
3. [Détection automatique de framework](#3-détection-automatique-de-framework)
4. [Architecture technique](#4-architecture-technique)
   - 4.1 [Domain Model](#41-domain-model)
   - 4.2 [Application Layer (CQRS)](#42-application-layer-cqrs)
   - 4.3 [Infrastructure / Persistence](#43-infrastructure--persistence)
   - 4.4 [Contracts](#44-contracts)
   - 4.5 [Pipeline Generation Engine](#45-pipeline-generation-engine)
   - 4.6 [Frontend Angular](#46-frontend-angular)
5. [Exemples de YAML générés](#5-exemples-de-yaml-générés)
6. [Analyse d'impact](#6-analyse-dimpact)
7. [Plan d'implémentation par phases](#7-plan-dimplémentation-par-phases)
8. [Risques et décisions ouvertes](#8-risques-et-décisions-ouvertes)

---

## 1. Vue d'ensemble

### Situation actuelle

Le système de génération de pipelines applicatifs (`AppPipelineGenerationEngine`) produit déjà :
- Des pipelines CI (build + publish) et Release (deploy par environnement)
- Un support container (Docker build → ACR push) et code (restore → build → publish → deploy)
- Des propriétés par ressource : `DockerfilePath`, `SourceCodePath`, `BuildCommand`, `ApplicationName`
- Un champ `TestCommand` dans `AppPipelineGenerationRequest` **mais non branché** au domain model
- Un champ `EnableSecurityScans` pour Trivy/Syft en mode container

### Ce qui manque

- Aucun mécanisme pour activer/désactiver des étapes de pipeline **par ressource compute**
- Pas de détection automatique du framework de test
- Pas d'intégration SonarQube/SonarCloud
- Pas de publication de couverture de tests
- Pas de linting/formatting check
- Pas de scan de dépendances (OWASP, Snyk) en mode code
- Le `TestCommand` existe dans le modèle de génération mais n'est jamais peuplé depuis le domaine

### Principe architectural

```
┌─────────────────────────────────────────────────────────┐
│  Resource (WebApp / FunctionApp / ContainerApp)          │
│                                                          │
│  PipelineOptions (owned entity collection)               │
│  ├─ RunUnitTests         = true/false                    │
│  ├─ TestCommand          = "dotnet test" (auto ou custom)│
│  ├─ TestFramework        = "xunit" (auto-detected)       │
│  ├─ PublishTestResults   = true/false                    │
│  ├─ PublishCodeCoverage  = true/false                    │
│  ├─ RunSonarAnalysis     = true/false                    │
│  ├─ SonarProjectKey      = "my-project"                  │
│  ├─ RunLinting           = true/false                    │
│  ├─ LintCommand          = "dotnet format --verify..."   │
│  ├─ RunSecurityScan      = true/false                    │
│  ├─ ...                                                  │
│                                                          │
└──────────────┬──────────────────────────────────────────┘
               │ mapped via AppPipelineRequestFactory
               ▼
┌─────────────────────────────────────────────────────────┐
│  AppPipelineGenerationRequest                            │
│  (extended with PipelineStepOptions)                     │
└──────────────┬──────────────────────────────────────────┘
               │ consumed by generators
               ▼
┌─────────────────────────────────────────────────────────┐
│  Shared Pipeline Templates (.step.yml, .job.yml)         │
│  Conditional sections: ${{ if eq(params.X, true) }}      │
│  Chaque option = un bloc conditionnel dans le template   │
└─────────────────────────────────────────────────────────┘
```

---

## 2. Catalogue des options de pipeline

### Options applicables en mode **Code** et **Container**

| # | Option | Description | Paramètres | Mode |
|---|--------|-------------|------------|------|
| 1 | **Tests unitaires** | Exécuter les TU dans le CI | `runUnitTests`, `testCommand`, `testFramework`, `testResultsFormat` | Code + Container |
| 2 | **Publication résultats de tests** | Publier les résultats JUnit/TRX dans Azure DevOps | `publishTestResults`, `testResultsPath`, `testResultsFormat` | Code + Container |
| 3 | **Couverture de code** | Générer et publier un rapport de couverture | `publishCodeCoverage`, `coverageTool` (Cobertura/JaCoCo), `coverageReportPath` | Code + Container |
| 4 | **Analyse SonarQube / SonarCloud** | Scanner le code avec Sonar | `runSonarAnalysis`, `sonarProjectKey`, `sonarOrganization`, `sonarHostUrl`, `sonarServiceConnection` | Code + Container |
| 5 | **Linting / Formatting** | Vérifier le formatage du code | `runLinting`, `lintCommand` | Code + Container |
| 6 | **Scan de sécurité des dépendances** | OWASP Dependency-Check, npm audit, pip-audit | `runDependencyScan`, `dependencyScanTool` | Code |
| 7 | **Scan de sécurité container** | Trivy / Syft (déjà partiellement implémenté via `EnableSecurityScans`) | `enableContainerSecurityScan` | Container |
| 8 | **Build validation (dry-run)** | Compiler sans déployer sur PR | `runBuildValidation` | Code |
| 9 | **Cache des dépendances** | Activer le caching NuGet/npm/pip | `enableDependencyCache`, `cacheKey` | Code |
| 10 | **Smoke tests post-deploy** | Exécuter un health-check après déploiement | `runSmokeTests`, `smokeTestUrl`, `smokeTestCommand` | Code + Container |
| 11 | **License compliance** | Vérifier les licences des dépendances | `runLicenseCheck`, `licenseCheckTool` | Code |
| 12 | **Notifications** | Webhook Teams/Slack en fin de pipeline | `enableNotifications`, `notificationWebhookUrl` | Code + Container |

### Priorité recommandée

| Priorité | Options |
|----------|---------|
| **P0 — MVP** | Tests unitaires, Publication résultats de tests, Couverture de code |
| **P1 — Core** | Analyse Sonar, Linting, Build validation |
| **P2 — Extended** | Scan de sécurité dépendances, Cache dépendances, Smoke tests |
| **P3 — Nice-to-have** | License compliance, Notifications |

---

## 3. Détection automatique de framework

### Stratégie

La détection se fait **côté backend** lors de la création/mise à jour d'une ressource, en analysant le `SourceCodePath` (mode code) ou le `DockerfilePath` (mode container) dans le repository Git configuré.

### Matrice de détection par runtime

#### .NET (`RuntimeStack = DOTNETCORE`)

| Signal | Framework détecté | TestCommand suggéré | TestResultsFormat |
|--------|-------------------|---------------------|-------------------|
| `<PackageReference Include="xunit"` dans `*.csproj` | xUnit | `dotnet test --logger "trx;LogFileName=results.trx" --collect:"XPlat Code Coverage"` | `VSTest` (TRX) |
| `<PackageReference Include="NUnit"` dans `*.csproj` | NUnit | `dotnet test --logger "trx;LogFileName=results.trx" --collect:"XPlat Code Coverage"` | `VSTest` (TRX) |
| `<PackageReference Include="MSTest"` dans `*.csproj` | MSTest | `dotnet test --logger "trx;LogFileName=results.trx" --collect:"XPlat Code Coverage"` | `VSTest` (TRX) |
| Aucun `*Test*` project trouvé | Aucun | _(option désactivée par défaut)_ | — |

**Heuristiques supplémentaires :**
- Chercher les fichiers `*.Tests.csproj` ou `*Tests.csproj` dans l'arborescence
- Scanner les `<PackageReference>` pour identifier le framework
- Proposer automatiquement `--collect:"XPlat Code Coverage"` si `coverlet.collector` est référencé
- Détecter `dotnet-format` ou `.editorconfig` pour le linting

#### Node.js (`RuntimeStack = NODE`)

| Signal | Framework détecté | TestCommand suggéré | TestResultsFormat |
|--------|-------------------|---------------------|-------------------|
| `"jest"` dans `devDependencies` | Jest | `npx jest --ci --reporters=default --reporters=jest-junit` | `JUnit` |
| `"vitest"` dans `devDependencies` | Vitest | `npx vitest run --reporter=junit --outputFile=results.xml` | `JUnit` |
| `"mocha"` dans `devDependencies` | Mocha | `npx mocha --reporter mocha-junit-reporter` | `JUnit` |
| `"test"` script dans `package.json` | Générique | `npm test` | `JUnit` |
| Aucun script `test` | Aucun | _(option désactivée par défaut)_ | — |

**Heuristiques supplémentaires :**
- Détecter `"eslint"` ou `"prettier"` pour le linting
- Détecter `"c8"`, `"nyc"`, ou `"istanbul"` pour la couverture
- Vérifier `jest.config.*` ou `vitest.config.*`

#### Python (`RuntimeStack = PYTHON`)

| Signal | Framework détecté | TestCommand suggéré | TestResultsFormat |
|--------|-------------------|---------------------|-------------------|
| `pytest` dans `requirements.txt` / `pyproject.toml` | pytest | `python -m pytest --junitxml=results.xml --cov --cov-report=xml` | `JUnit` |
| `unittest` import dans `test_*.py` | unittest | `python -m pytest --junitxml=results.xml` | `JUnit` |

**Heuristiques :**
- Détecter `pylint`, `flake8`, `ruff`, `black` pour le linting
- Détecter `coverage` ou `pytest-cov` pour la couverture

#### Java (`RuntimeStack = JAVA`)

| Signal | Framework détecté | TestCommand suggéré | TestResultsFormat |
|--------|-------------------|---------------------|-------------------|
| `junit-jupiter` dans `pom.xml` | JUnit 5 | `mvn test` | `JUnit` |
| `junit` (v4) dans `pom.xml` | JUnit 4 | `mvn test` | `JUnit` |
| `testng` dans `pom.xml` | TestNG | `mvn test` | `JUnit` |
| `build.gradle` avec `testImplementation 'junit'` | JUnit | `gradle test` | `JUnit` |

**Heuristiques :**
- Détecter `spotbugs`, `checkstyle`, `pmd` pour le linting
- Détecter `jacoco` pour la couverture

### API de détection

La détection peut être exposée comme une query CQRS :

```
GET /api/resources/{resourceId}/detect-pipeline-options
→ DetectedPipelineOptionsResponse
{
  "testFramework": "xunit",
  "suggestedTestCommand": "dotnet test --logger trx --collect:\"XPlat Code Coverage\"",
  "suggestedTestResultsFormat": "VSTest",
  "suggestedCoverageTool": "Cobertura",
  "lintingAvailable": true,
  "suggestedLintCommand": "dotnet format --verify-no-changes",
  "sonarConfigDetected": false,
  "dependencyScanAvailable": true
}
```

> **Note :** Cette détection nécessite un accès au contenu du repository Git (via le PAT configuré au niveau du projet). Si le repo n'est pas accessible, les suggestions restent vides et l'utilisateur configure manuellement.

---

## 4. Architecture technique

### 4.1 Domain Model

#### Nouveau value object : `AppPipelineStepOptions`

Stocker les options comme une entité possédée directement sur la ressource compute, car les options sont **par ressource** et non par environnement.

```csharp
// src/Api/InfraFlowSculptor.Domain/Common/ValueObjects/AppPipelineStepOptions.cs

/// <summary>
/// Configurable pipeline step options for a compute resource's CI/CD pipeline.
/// Stored as an owned entity on WebApp, FunctionApp, and ContainerApp.
/// </summary>
public sealed class AppPipelineStepOptions
{
    // ── Tests ──────────────────────────────────────────────────
    /// <summary>Whether to run unit tests in the CI pipeline.</summary>
    public bool RunUnitTests { get; private set; }

    /// <summary>Custom test command. Auto-detected if null.</summary>
    public string? TestCommand { get; private set; }

    /// <summary>Detected or user-specified test framework (xunit, nunit, jest, pytest, etc.).</summary>
    public string? TestFramework { get; private set; }

    /// <summary>Test results output format (VSTest, JUnit).</summary>
    public string? TestResultsFormat { get; private set; }

    /// <summary>Glob pattern for test result files.</summary>
    public string? TestResultsPath { get; private set; }

    /// <summary>Whether to publish test results to Azure DevOps.</summary>
    public bool PublishTestResults { get; private set; }

    // ── Couverture ─────────────────────────────────────────────
    /// <summary>Whether to collect and publish code coverage.</summary>
    public bool PublishCodeCoverage { get; private set; }

    /// <summary>Coverage tool: Cobertura or JaCoCo.</summary>
    public string? CoverageTool { get; private set; }

    /// <summary>Path to the coverage report file.</summary>
    public string? CoverageReportPath { get; private set; }

    // ── Sonar ──────────────────────────────────────────────────
    /// <summary>Whether to run SonarQube/SonarCloud analysis.</summary>
    public bool RunSonarAnalysis { get; private set; }

    /// <summary>Sonar project key.</summary>
    public string? SonarProjectKey { get; private set; }

    /// <summary>Sonar organization (SonarCloud only).</summary>
    public string? SonarOrganization { get; private set; }

    /// <summary>Service connection name for Sonar in Azure DevOps.</summary>
    public string? SonarServiceConnection { get; private set; }

    // ── Linting ────────────────────────────────────────────────
    /// <summary>Whether to run linting/formatting checks.</summary>
    public bool RunLinting { get; private set; }

    /// <summary>Custom lint command.</summary>
    public string? LintCommand { get; private set; }

    // ── Sécurité ───────────────────────────────────────────────
    /// <summary>Whether to scan dependencies for known vulnerabilities.</summary>
    public bool RunDependencyScan { get; private set; }

    /// <summary>Dependency scan tool: OWASPDependencyCheck, NpmAudit, PipAudit, Snyk.</summary>
    public string? DependencyScanTool { get; private set; }

    // ── Build ──────────────────────────────────────────────────
    /// <summary>Whether to run build validation on PR pipelines.</summary>
    public bool RunBuildValidation { get; private set; }

    // ── Cache ──────────────────────────────────────────────────
    /// <summary>Whether to cache dependencies (NuGet, npm, pip).</summary>
    public bool EnableDependencyCache { get; private set; }

    // ── Post-deploy ────────────────────────────────────────────
    /// <summary>Whether to run smoke tests after deployment.</summary>
    public bool RunSmokeTests { get; private set; }

    /// <summary>Custom smoke test command or URL to health-check.</summary>
    public string? SmokeTestCommand { get; private set; }
}
```

#### Intégration dans les aggregates compute

Les 3 aggregates compute possèdent déjà `DockerfilePath`, `SourceCodePath`, `BuildCommand`, `ApplicationName`. On ajoute `PipelineStepOptions` comme owned entity :

```csharp
// Dans WebApp, FunctionApp, ContainerApp :
public AppPipelineStepOptions PipelineStepOptions { get; private set; } = new();

public void SetPipelineStepOptions(AppPipelineStepOptions options)
{
    if (IsExisting) return; // Guard: pas de pipeline pour les resources existantes
    PipelineStepOptions = options ?? throw new ArgumentNullException(nameof(options));
}
```

#### Alternative envisagée : collection de key-value

❌ **Rejetée** — Violerait le principe de typage fort du projet. `Dictionary<string, object>` interdit.

#### Alternative envisagée : entité séparée `PipelineProfile`

❌ **Rejetée** — Surcharge d'abstraction. Les options sont 1:1 avec la ressource compute, pas un concept partagé entre ressources.

### 4.2 Application Layer (CQRS)

#### Nouvelles commandes

| Commande | Description |
|----------|-------------|
| `SetPipelineStepOptionsCommand` | Met à jour les options de pipeline pour une ressource compute |
| `DetectPipelineOptionsQuery` | Analyse le repo Git et suggère des options basées sur le framework détecté |

#### Modifications de commandes existantes

| Commande existante | Modification |
|---------------------|-------------|
| `CreateWebAppCommand` | Ajouter `PipelineStepOptions?` optionnel dans la request |
| `UpdateWebAppCommand` | Ajouter `PipelineStepOptions?` optionnel dans la request |
| `CreateFunctionAppCommand` | Idem |
| `UpdateFunctionAppCommand` | Idem |
| `CreateContainerAppCommand` | Idem |
| `UpdateContainerAppCommand` | Idem |

#### Modification du `AppPipelineRequestFactory`

```csharp
// Mapper PipelineStepOptions du domain vers AppPipelineGenerationRequest
request.TestCommand = resource.PipelineStepOptions.RunUnitTests 
    ? resource.PipelineStepOptions.TestCommand 
    : null;
request.EnableSecurityScans = resource.PipelineStepOptions.RunDependencyScan;
// + tous les nouveaux champs
```

### 4.3 Infrastructure / Persistence

#### EF Core — Owned Entity Configuration

```csharp
// Dans WebAppConfiguration, FunctionAppConfiguration, ContainerAppConfiguration :
builder.OwnsOne(x => x.PipelineStepOptions, options =>
{
    options.Property(o => o.RunUnitTests).HasDefaultValue(false);
    options.Property(o => o.TestCommand).HasMaxLength(500);
    options.Property(o => o.TestFramework).HasMaxLength(50);
    options.Property(o => o.TestResultsFormat).HasMaxLength(20);
    options.Property(o => o.TestResultsPath).HasMaxLength(500);
    options.Property(o => o.PublishTestResults).HasDefaultValue(false);
    options.Property(o => o.PublishCodeCoverage).HasDefaultValue(false);
    options.Property(o => o.CoverageTool).HasMaxLength(20);
    options.Property(o => o.CoverageReportPath).HasMaxLength(500);
    options.Property(o => o.RunSonarAnalysis).HasDefaultValue(false);
    options.Property(o => o.SonarProjectKey).HasMaxLength(200);
    options.Property(o => o.SonarOrganization).HasMaxLength(200);
    options.Property(o => o.SonarServiceConnection).HasMaxLength(200);
    options.Property(o => o.RunLinting).HasDefaultValue(false);
    options.Property(o => o.LintCommand).HasMaxLength(500);
    options.Property(o => o.RunDependencyScan).HasDefaultValue(false);
    options.Property(o => o.DependencyScanTool).HasMaxLength(50);
    options.Property(o => o.RunBuildValidation).HasDefaultValue(false);
    options.Property(o => o.EnableDependencyCache).HasDefaultValue(false);
    options.Property(o => o.RunSmokeTests).HasDefaultValue(false);
    options.Property(o => o.SmokeTestCommand).HasMaxLength(500);
});
```

#### Migration EF Core

```
AddPipelineStepOptionsToComputeResources
```

Ajoute les colonnes `PipelineStepOptions_*` aux tables `WebApps`, `FunctionApps`, `ContainerApps`. Toutes les colonnes nullable avec `HasDefaultValue(false)` pour les booléens → rétrocompatible.

### 4.4 Contracts

#### Nouveau DTO : `PipelineStepOptionsRequest` / `PipelineStepOptionsResponse`

```csharp
// src/Api/InfraFlowSculptor.Contracts/Common/PipelineStepOptionsRequest.cs
public sealed record PipelineStepOptionsRequest
{
    public bool RunUnitTests { get; init; }
    public string? TestCommand { get; init; }
    public string? TestFramework { get; init; }
    public string? TestResultsFormat { get; init; }
    public string? TestResultsPath { get; init; }
    public bool PublishTestResults { get; init; }
    public bool PublishCodeCoverage { get; init; }
    public string? CoverageTool { get; init; }
    public string? CoverageReportPath { get; init; }
    public bool RunSonarAnalysis { get; init; }
    public string? SonarProjectKey { get; init; }
    public string? SonarOrganization { get; init; }
    public string? SonarServiceConnection { get; init; }
    public bool RunLinting { get; init; }
    public string? LintCommand { get; init; }
    public bool RunDependencyScan { get; init; }
    public string? DependencyScanTool { get; init; }
    public bool RunBuildValidation { get; init; }
    public bool EnableDependencyCache { get; init; }
    public bool RunSmokeTests { get; init; }
    public string? SmokeTestCommand { get; init; }
}
```

```csharp
// src/Api/InfraFlowSculptor.Contracts/Common/DetectedPipelineOptionsResponse.cs
public sealed record DetectedPipelineOptionsResponse
{
    public string? TestFramework { get; init; }
    public string? SuggestedTestCommand { get; init; }
    public string? SuggestedTestResultsFormat { get; init; }
    public string? SuggestedCoverageTool { get; init; }
    public string? SuggestedCoverageReportPath { get; init; }
    public bool LintingAvailable { get; init; }
    public string? SuggestedLintCommand { get; init; }
    public bool SonarConfigDetected { get; init; }
    public string? SuggestedSonarProjectKey { get; init; }
    public bool DependencyScanAvailable { get; init; }
    public string? SuggestedDependencyScanTool { get; init; }
}
```

#### Modification des DTOs existants

Ajouter `PipelineStepOptions? PipelineStepOptions` dans :
- `CreateWebAppRequest` / `UpdateWebAppRequest`
- `CreateFunctionAppRequest` / `UpdateFunctionAppRequest`
- `CreateContainerAppRequest` / `UpdateContainerAppRequest`
- Les responses correspondantes

### 4.5 Pipeline Generation Engine

#### Nouvelles propriétés sur `AppPipelineGenerationRequest`

```csharp
// Ajouter dans AppPipelineGenerationRequest :
public bool RunUnitTests { get; set; }
public string? TestResultsFormat { get; set; }     // "VSTest" | "JUnit"
public string? TestResultsPath { get; set; }
public bool PublishTestResults { get; set; }
public bool PublishCodeCoverage { get; set; }
public string? CoverageTool { get; set; }           // "Cobertura" | "JaCoCo"
public string? CoverageReportPath { get; set; }
public bool RunSonarAnalysis { get; set; }
public string? SonarProjectKey { get; set; }
public string? SonarOrganization { get; set; }
public string? SonarServiceConnection { get; set; }
public bool RunLinting { get; set; }
public string? LintCommand { get; set; }
public bool RunDependencyScan { get; set; }
public string? DependencyScanTool { get; set; }
public bool RunBuildValidation { get; set; }
public bool EnableDependencyCache { get; set; }
public bool RunSmokeTests { get; set; }
public string? SmokeTestCommand { get; set; }
```

#### Nouveaux templates `.step.yml` générés

Les templates suivants seront ajoutés dans `AppPipelineTemplatesGenerator` :

| Template | Condition d'émission | Contenu |
|----------|---------------------|---------|
| `steps/app-publish-test-results.step.yml` | `publishTestResults = true` | `PublishTestResults@2` task |
| `steps/app-publish-code-coverage.step.yml` | `publishCodeCoverage = true` | `PublishCodeCoverageResults@2` task |
| `steps/app-sonar-prepare.step.yml` | `runSonarAnalysis = true` | `SonarQubePrepare@6` ou `SonarCloudPrepare@2` |
| `steps/app-sonar-analyze.step.yml` | `runSonarAnalysis = true` | `SonarQubeAnalyze@6` ou `SonarCloudAnalyze@2` |
| `steps/app-sonar-publish.step.yml` | `runSonarAnalysis = true` | `SonarQubePublish@6` ou `SonarCloudPublish@2` |
| `steps/app-lint.step.yml` | `runLinting = true` | Custom lint command execution |
| `steps/app-dependency-scan.step.yml` | `runDependencyScan = true` | OWASP/npm audit/pip-audit |
| `steps/app-cache-dependencies.step.yml` | `enableDependencyCache = true` | `Cache@2` task |
| `steps/app-smoke-test.step.yml` | `runSmokeTests = true` | Post-deploy verification |

#### Modification des templates existants

**`app-ci-code.job.yml`** — Ordre d'exécution des steps dans le job CI :

```yaml
jobs:
  - job: BuildAndTest
    pool: ...
    steps:
      # 1. Checkout
      - template: ../steps/checkout.step.yml

      # 2. Cache (optionnel)
      - ${{ if eq(parameters.enableDependencyCache, true) }}:
        - template: ../steps/app-cache-dependencies.step.yml
          parameters:
            runtimeStack: ${{ parameters.runtimeStack }}

      # 3. SDK Setup
      - template: ../steps/app-sdk-setup.step.yml
        parameters:
          runtimeStack: ${{ parameters.runtimeStack }}
          runtimeVersion: ${{ parameters.runtimeVersion }}

      # 4. Sonar Prepare (optionnel, AVANT le build)
      - ${{ if eq(parameters.runSonarAnalysis, true) }}:
        - template: ../steps/app-sonar-prepare.step.yml
          parameters:
            sonarProjectKey: ${{ parameters.sonarProjectKey }}
            sonarOrganization: ${{ parameters.sonarOrganization }}
            sonarServiceConnection: ${{ parameters.sonarServiceConnection }}

      # 5. Lint (optionnel)
      - ${{ if eq(parameters.runLinting, true) }}:
        - template: ../steps/app-lint.step.yml
          parameters:
            lintCommand: ${{ parameters.lintCommand }}
            sourcePath: ${{ parameters.sourcePath }}

      # 6. Build
      - template: ../steps/app-build-code.step.yml
        parameters:
          buildCommand: ${{ parameters.buildCommand }}
          sourcePath: ${{ parameters.sourcePath }}

      # 7. Tests unitaires (optionnel)
      - ${{ if eq(parameters.runUnitTests, true) }}:
        - template: ../steps/app-run-tests.step.yml
          parameters:
            testCommand: ${{ parameters.testCommand }}
            sourcePath: ${{ parameters.sourcePath }}

      # 8. Publish Test Results (optionnel)
      - ${{ if eq(parameters.publishTestResults, true) }}:
        - template: ../steps/app-publish-test-results.step.yml
          parameters:
            testResultsFormat: ${{ parameters.testResultsFormat }}
            testResultsPath: ${{ parameters.testResultsPath }}

      # 9. Publish Code Coverage (optionnel)
      - ${{ if eq(parameters.publishCodeCoverage, true) }}:
        - template: ../steps/app-publish-code-coverage.step.yml
          parameters:
            coverageTool: ${{ parameters.coverageTool }}
            coverageReportPath: ${{ parameters.coverageReportPath }}

      # 10. Sonar Analyze + Publish (optionnel, APRÈS le build+tests)
      - ${{ if eq(parameters.runSonarAnalysis, true) }}:
        - template: ../steps/app-sonar-analyze.step.yml
        - template: ../steps/app-sonar-publish.step.yml

      # 11. Scan dépendances (optionnel)
      - ${{ if eq(parameters.runDependencyScan, true) }}:
        - template: ../steps/app-dependency-scan.step.yml
          parameters:
            dependencyScanTool: ${{ parameters.dependencyScanTool }}
            sourcePath: ${{ parameters.sourcePath }}

      # 12. Publish artifacts
      - template: ../steps/app-publish-artifact.step.yml
        parameters:
          sourcePath: ${{ parameters.sourcePath }}
```

### 4.6 Frontend Angular

#### Nouveau composant : `PipelineOptionsPanel`

```
src/Front/src/app/shared/components/pipeline-options/
├── pipeline-options.component.ts
├── pipeline-options.component.html
├── pipeline-options.component.scss
└── pipeline-options.interface.ts
```

**Intégration dans `resource-edit.component.html`** :

Le composant est intégré dans le tab "App Pipeline" existant, après les champs actuels (`applicationName`, `dockerfilePath`, `sourceCodePath`, `buildCommand`) :

```html
<!-- Dans le tab App Pipeline -->
<mat-accordion>
  <!-- Section existante : Deployment Config -->
  <mat-expansion-panel [expanded]="true">
    <mat-expansion-panel-header>
      <mat-panel-title>{{ 'RESOURCE_EDIT.APP_PIPELINE.DEPLOYMENT' | translate }}</mat-panel-title>
    </mat-expansion-panel-header>
    <!-- Champs existants : applicationName, dockerfilePath, etc. -->
  </mat-expansion-panel>

  <!-- NOUVELLE SECTION : Pipeline Options -->
  <mat-expansion-panel>
    <mat-expansion-panel-header>
      <mat-panel-title>{{ 'RESOURCE_EDIT.APP_PIPELINE.OPTIONS' | translate }}</mat-panel-title>
    </mat-expansion-panel-header>
    <app-pipeline-options
      [options]="pipelineStepOptions()"
      [runtimeStack]="runtimeStack()"
      [deploymentMode]="deploymentMode()"
      (optionsChanged)="onPipelineOptionsChanged($event)"
      (detectOptions)="onDetectPipelineOptions()">
    </app-pipeline-options>
  </mat-expansion-panel>
</mat-accordion>
```

#### Structure du composant `PipelineOptionsPanel`

Le panel regroupe les options par catégories avec des toggle switches :

```
┌─────────────────────────────────────────────────────────┐
│ 🔧 Pipeline Options                                     │
│                                                          │
│  [🔍 Auto-detect]  ← bouton qui appelle l'API detection │
│                                                          │
│  ── Tests ──────────────────────────────────────────     │
│  [✓] Run unit tests                                      │
│      Framework: [xUnit ▾]  (auto-detected)               │
│      Test command: [dotnet test --logger trx ...]        │
│  [✓] Publish test results                                │
│      Format: [VSTest ▾]  Path: [**/*.trx]               │
│  [✓] Publish code coverage                               │
│      Tool: [Cobertura ▾]  Path: [**/coverage.cobertura] │
│                                                          │
│  ── Code Quality ───────────────────────────────────     │
│  [ ] SonarQube / SonarCloud analysis                     │
│      Project key: [____]  Organization: [____]           │
│      Service connection: [____]                          │
│  [ ] Run linting / formatting check                      │
│      Command: [dotnet format --verify-no-changes ...]    │
│                                                          │
│  ── Security ───────────────────────────────────────     │
│  [ ] Dependency vulnerability scan                       │
│      Tool: [OWASP Dependency-Check ▾]                    │
│  [✓] Container security scan (Trivy/Syft)                │
│      (visible uniquement en mode Container)              │
│                                                          │
│  ── Performance ────────────────────────────────────     │
│  [ ] Cache dependencies                                  │
│  [ ] Build validation on PR                              │
│                                                          │
│  ── Post-Deploy ────────────────────────────────────     │
│  [ ] Smoke tests after deployment                        │
│      Command: [curl -f https://myapp.azurewebsites...]   │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

#### Clés i18n

```json
{
  "RESOURCE_EDIT": {
    "APP_PIPELINE": {
      "OPTIONS": {
        "TITLE": "Pipeline Options",
        "AUTO_DETECT": "Auto-detect from repository",
        "AUTO_DETECT_TOOLTIP": "Analyze the repository to suggest test framework, commands, and tools",
        "TESTS": {
          "TITLE": "Tests",
          "RUN_UNIT_TESTS": "Run unit tests",
          "TEST_FRAMEWORK": "Test framework",
          "TEST_COMMAND": "Test command",
          "PUBLISH_TEST_RESULTS": "Publish test results",
          "TEST_RESULTS_FORMAT": "Results format",
          "TEST_RESULTS_PATH": "Results path",
          "PUBLISH_CODE_COVERAGE": "Publish code coverage",
          "COVERAGE_TOOL": "Coverage tool",
          "COVERAGE_REPORT_PATH": "Coverage report path"
        },
        "CODE_QUALITY": {
          "TITLE": "Code Quality",
          "RUN_SONAR": "SonarQube / SonarCloud analysis",
          "SONAR_PROJECT_KEY": "Project key",
          "SONAR_ORGANIZATION": "Organization (SonarCloud)",
          "SONAR_SERVICE_CONNECTION": "Service connection",
          "RUN_LINTING": "Linting / formatting check",
          "LINT_COMMAND": "Lint command"
        },
        "SECURITY": {
          "TITLE": "Security",
          "RUN_DEPENDENCY_SCAN": "Dependency vulnerability scan",
          "DEPENDENCY_SCAN_TOOL": "Scan tool",
          "CONTAINER_SECURITY_SCAN": "Container security scan (Trivy / Syft)"
        },
        "PERFORMANCE": {
          "TITLE": "Performance",
          "ENABLE_CACHE": "Cache dependencies",
          "BUILD_VALIDATION": "Build validation on PR"
        },
        "POST_DEPLOY": {
          "TITLE": "Post-Deploy",
          "RUN_SMOKE_TESTS": "Smoke tests after deployment",
          "SMOKE_TEST_COMMAND": "Smoke test command"
        }
      }
    }
  }
}
```

---

## 5. Exemples de YAML générés

### 5.1 Pipeline CI applicative — .NET avec toutes les options activées

```yaml
# CI Application Pipeline for my-api — Auto-generated by InfraFlowSculptor
name: $(Date:yyyyMMdd).$(Rev:r)

trigger:
  branches:
    include:
      - main
      - release/*
  paths:
    include:
      - backend/src/*
      - .azuredevops/Common/*
      - .azuredevops/myconfig/apps/my-api/*

pool:
  vmImage: 'ubuntu-latest'

extends:
  template: ../../../Common/pipelines/app-ci-code.pipeline.yml
  parameters:
    resourceName: 'my-api'
    configName: 'myconfig'
    runtimeStack: 'DOTNETCORE'
    runtimeVersion: '9.0'
    sourcePath: 'backend/src'
    buildCommand: 'dotnet publish -c Release -o published'
    # ── Pipeline Options ────────────────────────────────
    runUnitTests: true
    testCommand: 'dotnet test --logger "trx;LogFileName=results.trx" --collect:"XPlat Code Coverage" --results-directory $(Agent.TempDirectory)/TestResults'
    testResultsFormat: 'VSTest'
    testResultsPath: '$(Agent.TempDirectory)/TestResults/**/*.trx'
    publishTestResults: true
    publishCodeCoverage: true
    coverageTool: 'Cobertura'
    coverageReportPath: '$(Agent.TempDirectory)/TestResults/**/coverage.cobertura.xml'
    runSonarAnalysis: true
    sonarProjectKey: 'myorg_my-api'
    sonarOrganization: 'myorg'
    sonarServiceConnection: 'SonarCloud-MyOrg'
    runLinting: true
    lintCommand: 'dotnet format --verify-no-changes --verbosity diagnostic'
    runDependencyScan: false
    enableDependencyCache: true
    runBuildValidation: true
```

### 5.2 Pipeline CI applicative — Node.js avec Jest

```yaml
# CI Application Pipeline for my-frontend — Auto-generated by InfraFlowSculptor
name: $(Date:yyyyMMdd).$(Rev:r)

trigger:
  branches:
    include:
      - main
      - release/*
  paths:
    include:
      - frontend/*
      - .azuredevops/Common/*
      - .azuredevops/myconfig/apps/my-frontend/*

pool:
  vmImage: 'ubuntu-latest'

extends:
  template: ../../../Common/pipelines/app-ci-code.pipeline.yml
  parameters:
    resourceName: 'my-frontend'
    configName: 'myconfig'
    runtimeStack: 'NODE'
    runtimeVersion: '22'
    sourcePath: 'frontend'
    buildCommand: 'npm run build'
    # ── Pipeline Options ────────────────────────────────
    runUnitTests: true
    testCommand: 'npx jest --ci --reporters=default --reporters=jest-junit --coverage --coverageReporters=cobertura'
    testResultsFormat: 'JUnit'
    testResultsPath: 'frontend/junit.xml'
    publishTestResults: true
    publishCodeCoverage: true
    coverageTool: 'Cobertura'
    coverageReportPath: 'frontend/coverage/cobertura-coverage.xml'
    runSonarAnalysis: false
    runLinting: true
    lintCommand: 'npx eslint . --max-warnings 0'
    runDependencyScan: true
    dependencyScanTool: 'NpmAudit'
    enableDependencyCache: true
    runBuildValidation: true
```

### 5.3 Pipeline CI applicative — Container mode avec scans de sécurité

```yaml
# CI Application Pipeline for my-worker — Auto-generated by InfraFlowSculptor
name: $(Date:yyyyMMdd).$(Rev:r)

trigger:
  branches:
    include:
      - main
      - release/*
  paths:
    include:
      - services/worker/*
      - .azuredevops/Common/*
      - .azuredevops/myconfig/apps/my-worker/*

pool:
  vmImage: 'ubuntu-latest'

extends:
  template: ../../../Common/pipelines/app-ci-container.pipeline.yml
  parameters:
    resourceName: 'my-worker'
    configName: 'myconfig'
    imageRepository: 'myapp/worker'
    dockerfilePath: 'services/worker/Dockerfile'
    enableSecurityScans: true
    acrAuthMode: 'ServiceConnection'
    # ── Pipeline Options ────────────────────────────────
    runUnitTests: true
    testCommand: 'dotnet test --logger "trx;LogFileName=results.trx" --collect:"XPlat Code Coverage"'
    testResultsFormat: 'VSTest'
    publishTestResults: true
    publishCodeCoverage: true
    coverageTool: 'Cobertura'
    runSonarAnalysis: true
    sonarProjectKey: 'myorg_worker'
    sonarServiceConnection: 'SonarCloud-MyOrg'
    runLinting: false
    runDependencyScan: false
    enableDependencyCache: false
```

### 5.4 Template partagé : `app-publish-test-results.step.yml`

```yaml
# Publish Test Results — Auto-generated by InfraFlowSculptor
parameters:
  - name: testResultsFormat
    type: string
    default: 'VSTest'
    values:
      - VSTest
      - JUnit
  - name: testResultsPath
    type: string
    default: '**/*.trx'
  - name: testRunTitle
    type: string
    default: 'Unit Tests'

steps:
  - task: PublishTestResults@2
    displayName: 'Publish test results'
    inputs:
      testResultsFormat: ${{ parameters.testResultsFormat }}
      testResultsFiles: ${{ parameters.testResultsPath }}
      testRunTitle: ${{ parameters.testRunTitle }}
      mergeTestResults: true
      failTaskOnFailedTests: true
    condition: succeededOrFailed()
```

### 5.5 Template partagé : `app-publish-code-coverage.step.yml`

```yaml
# Publish Code Coverage — Auto-generated by InfraFlowSculptor
parameters:
  - name: coverageTool
    type: string
    default: 'Cobertura'
    values:
      - Cobertura
      - JaCoCo
  - name: coverageReportPath
    type: string
    default: '**/coverage.cobertura.xml'
  - name: summaryFileLocation
    type: string
    default: ''

steps:
  - task: PublishCodeCoverageResults@2
    displayName: 'Publish code coverage'
    inputs:
      codeCoverageTool: ${{ parameters.coverageTool }}
      summaryFileLocation: ${{ coalesce(parameters.summaryFileLocation, parameters.coverageReportPath) }}
    condition: succeededOrFailed()
```

### 5.6 Template partagé : `app-sonar-prepare.step.yml`

```yaml
# SonarQube/SonarCloud Prepare — Auto-generated by InfraFlowSculptor
parameters:
  - name: sonarProjectKey
    type: string
  - name: sonarOrganization
    type: string
    default: ''
  - name: sonarServiceConnection
    type: string
  - name: sonarExtraProperties
    type: string
    default: ''

steps:
  - ${{ if ne(parameters.sonarOrganization, '') }}:
    # SonarCloud
    - task: SonarCloudPrepare@3
      displayName: 'Prepare SonarCloud analysis'
      inputs:
        SonarCloud: ${{ parameters.sonarServiceConnection }}
        organization: ${{ parameters.sonarOrganization }}
        scannerMode: 'dotnet'
        projectKey: ${{ parameters.sonarProjectKey }}
        extraProperties: ${{ parameters.sonarExtraProperties }}

  - ${{ if eq(parameters.sonarOrganization, '') }}:
    # SonarQube Server
    - task: SonarQubePrepare@7
      displayName: 'Prepare SonarQube analysis'
      inputs:
        SonarQube: ${{ parameters.sonarServiceConnection }}
        scannerMode: 'dotnet'
        projectKey: ${{ parameters.sonarProjectKey }}
        extraProperties: ${{ parameters.sonarExtraProperties }}
```

### 5.7 Template partagé : `app-cache-dependencies.step.yml`

```yaml
# Cache Dependencies — Auto-generated by InfraFlowSculptor
parameters:
  - name: runtimeStack
    type: string

steps:
  - ${{ if eq(parameters.runtimeStack, 'DOTNETCORE') }}:
    - task: Cache@2
      displayName: 'Cache NuGet packages'
      inputs:
        key: 'nuget | "$(Agent.OS)" | **/packages.lock.json'
        restoreKeys: |
          nuget | "$(Agent.OS)"
        path: $(NUGET_PACKAGES)

  - ${{ if eq(parameters.runtimeStack, 'NODE') }}:
    - task: Cache@2
      displayName: 'Cache npm packages'
      inputs:
        key: 'npm | "$(Agent.OS)" | **/package-lock.json'
        restoreKeys: |
          npm | "$(Agent.OS)"
        path: $(npm_config_cache)

  - ${{ if eq(parameters.runtimeStack, 'PYTHON') }}:
    - task: Cache@2
      displayName: 'Cache pip packages'
      inputs:
        key: 'pip | "$(Agent.OS)" | **/requirements.txt'
        restoreKeys: |
          pip | "$(Agent.OS)"
        path: $(PIP_CACHE_DIR)

  - ${{ if eq(parameters.runtimeStack, 'JAVA') }}:
    - task: Cache@2
      displayName: 'Cache Maven packages'
      inputs:
        key: 'maven | "$(Agent.OS)" | **/pom.xml'
        restoreKeys: |
          maven | "$(Agent.OS)"
        path: $(MAVEN_REPO_LOCAL)
```

---

## 6. Analyse d'impact

### 6.1 Fichiers impactés — Backend

| Couche | Fichier | Type de modification |
|--------|---------|---------------------|
| **Domain** | `Common/ValueObjects/AppPipelineStepOptions.cs` | **Nouveau** |
| **Domain** | `WebApp.cs`, `FunctionApp.cs`, `ContainerApp.cs` | Ajout propriété + méthode `SetPipelineStepOptions` |
| **Application** | `AppPipelineRequestFactory.cs` | Mapper `PipelineStepOptions` → `AppPipelineGenerationRequest` |
| **Application** | `CreateWebAppCommandHandler.cs`, `UpdateWebAppCommandHandler.cs` | Appeler `SetPipelineStepOptions` |
| **Application** | Idem pour FunctionApp et ContainerApp | Idem |
| **Application** | `DetectPipelineOptionsQueryHandler.cs` | **Nouveau** — auto-détection |
| **Infrastructure** | `WebAppConfiguration.cs`, `FunctionAppConfiguration.cs`, `ContainerAppConfiguration.cs` | `OwnsOne(PipelineStepOptions)` |
| **Infrastructure** | `PipelineOptionDetectionService.cs` | **Nouveau** — analyse du repo Git |
| **Infrastructure** | Migration EF Core | **Nouvelle** |
| **Contracts** | `PipelineStepOptionsRequest.cs`, `PipelineStepOptionsResponse.cs` | **Nouveaux** |
| **Contracts** | DTOs Create/Update WebApp, FunctionApp, ContainerApp | Ajout champ `PipelineStepOptions?` |
| **Contracts** | `DetectedPipelineOptionsResponse.cs` | **Nouveau** |
| **API** | `ResourceController.cs` ou endpoint dédié | **Nouveau** endpoint détection |
| **GenerationCore** | `AppPipelineGenerationRequest.cs` | Ajout propriétés pipeline options |
| **PipelineGeneration** | `AppPipelineTemplatesGenerator.cs` | Nouveaux templates conditionnels |
| **PipelineGeneration** | `AppCiPipelineBuilder.cs` | Passer les nouveaux paramètres |
| **PipelineGeneration** | `AppBuildStepEmitter.cs` | Découpler test du build |
| **PipelineGeneration** | `AppReleasePipelineBuilder.cs` | Ajouter smoke tests post-deploy |

### 6.2 Fichiers impactés — Frontend

| Composant | Type de modification |
|-----------|---------------------|
| `shared/components/pipeline-options/pipeline-options.component.*` | **Nouveau** composant |
| `shared/interfaces/pipeline-step-options.interface.ts` | **Nouvelle** interface TS |
| `features/resource-edit/resource-edit.component.html` | Intégration du panel options |
| `features/resource-edit/resource-edit.component.ts` | Signal/state pour pipeline options |
| `shared/services/resource.service.ts` (ou nouveau service) | Appel API détection |
| `shared/interfaces/web-app.interface.ts` | Ajout `pipelineStepOptions` |
| `shared/interfaces/function-app.interface.ts` | Ajout `pipelineStepOptions` |
| `shared/interfaces/container-app.interface.ts` | Ajout `pipelineStepOptions` |
| `assets/i18n/en.json`, `fr.json` | Nouvelles clés de traduction |

### 6.3 Fichiers impactés — Tests

| Projet de test | Portée |
|----------------|--------|
| `InfraFlowSculptor.Domain.Tests` | Tests pour `AppPipelineStepOptions`, méthodes `SetPipelineStepOptions` |
| `InfraFlowSculptor.Application.Tests` | Tests handlers Create/Update avec pipeline options |
| `InfraFlowSculptor.PipelineGeneration.Tests` | Tests de chaque template généré, parité CI code/container |
| `InfraFlowSculptor.Contracts.Tests` | Validation des DTOs |
| `InfraFlowSculptor.Infrastructure.Tests` | Tests EF Core owned entity mapping |

### 6.4 Impact sur les pipelines existantes

- **Rétrocompatibilité assurée** : toutes les options booléennes sont `false` par défaut → les pipelines existantes ne changent pas
- Les templates existants (`app-build-code.step.yml`) doivent être modifiés pour séparer le step test du step build, mais les paramètres restent optionnels
- Le `TestCommand` déjà présent dans `AppPipelineGenerationRequest` sera branché au lieu d'être ignoré

### 6.5 Impact sur le MCP Server

Le serveur MCP (`src/Mcp/`) expose des tools pour la création de projets. Le tool `draft_project_from_prompt` devrait être mis à jour pour :
- Suggérer des pipeline options lors de la création
- Utiliser la détection automatique de framework

---

## 7. Plan d'implémentation par phases

### Phase 1 — Foundation (P0)

> Objectif : brancher le `TestCommand` existant + ajouter tests + couverture

| # | Tâche | Couche | Effort estimé |
|---|-------|--------|---------------|
| 1.1 | Créer `AppPipelineStepOptions` value object dans Domain | Domain | S |
| 1.2 | Ajouter `PipelineStepOptions` aux 3 aggregates compute | Domain | S |
| 1.3 | Configurer EF Core `OwnsOne` pour les 3 compute tables | Infrastructure | S |
| 1.4 | Créer la migration EF Core | Infrastructure | S |
| 1.5 | Créer `PipelineStepOptionsRequest/Response` DTOs | Contracts | S |
| 1.6 | Modifier les Create/Update handlers des 3 compute types | Application | M |
| 1.7 | Modifier `AppPipelineRequestFactory` pour mapper les options | Application | M |
| 1.8 | Ajouter les propriétés dans `AppPipelineGenerationRequest` | GenerationCore | S |
| 1.9 | Modifier `AppPipelineTemplatesGenerator` : nouveaux templates `test-results`, `code-coverage` | PipelineGeneration | M |
| 1.10 | Modifier `AppCiPipelineBuilder` pour passer les paramètres | PipelineGeneration | M |
| 1.11 | Créer le composant Angular `pipeline-options` (tests + coverage only) | Frontend | M |
| 1.12 | Intégrer le composant dans `resource-edit` tab Pipeline | Frontend | S |
| 1.13 | Tests unitaires domain + application + generation | Tests | L |

### Phase 2 — Code Quality (P1)

> Objectif : SonarQube/SonarCloud + Linting + Build validation

| # | Tâche | Couche | Effort estimé |
|---|-------|--------|---------------|
| 2.1 | Ajouter propriétés Sonar et Lint dans `AppPipelineStepOptions` | Domain | S |
| 2.2 | Nouveaux templates `sonar-prepare`, `sonar-analyze`, `sonar-publish`, `lint` | PipelineGeneration | M |
| 2.3 | Étendre `AppCiPipelineBuilder` pour Sonar (avant/après build) | PipelineGeneration | M |
| 2.4 | Étendre le composant Angular avec sections Code Quality | Frontend | M |
| 2.5 | Tests unitaires generation + frontend | Tests | M |

### Phase 3 — Auto-detection (P1)

> Objectif : analyser le repo Git pour suggérer framework, commandes, outils

| # | Tâche | Couche | Effort estimé |
|---|-------|--------|---------------|
| 3.1 | Créer `DetectPipelineOptionsQuery` + handler | Application | M |
| 3.2 | Créer `PipelineOptionDetectionService` (analyse `.csproj`, `package.json`, etc.) | Infrastructure | L |
| 3.3 | Nouveau endpoint API `GET /resources/{id}/detect-pipeline-options` | API | S |
| 3.4 | Créer `DetectedPipelineOptionsResponse` DTO | Contracts | S |
| 3.5 | Bouton "Auto-detect" dans le composant Angular | Frontend | M |
| 3.6 | Tests unitaires détection (mocking du service Git) | Tests | L |

### Phase 4 — Security & Performance (P2)

> Objectif : scans de dépendances, cache, smoke tests

| # | Tâche | Couche | Effort estimé |
|---|-------|--------|---------------|
| 4.1 | Ajouter propriétés sécurité/cache/smoke dans `AppPipelineStepOptions` | Domain | S |
| 4.2 | Nouveaux templates `dependency-scan`, `cache-dependencies`, `smoke-test` | PipelineGeneration | M |
| 4.3 | Étendre `AppReleasePipelineBuilder` pour smoke tests post-deploy | PipelineGeneration | M |
| 4.4 | Étendre le composant Angular avec sections Security/Performance/Post-Deploy | Frontend | M |
| 4.5 | Tests unitaires | Tests | M |

### Phase 5 — MCP Integration (P3)

> Objectif : intégrer les pipeline options dans le flux MCP

| # | Tâche | Couche | Effort estimé |
|---|-------|--------|---------------|
| 5.1 | Mettre à jour `draft_project_from_prompt` pour inclure les pipeline options | Mcp | M |
| 5.2 | Ajouter un tool MCP `detect_pipeline_options` | Mcp | M |
| 5.3 | Tests | Tests | M |

---

## 8. Risques et décisions ouvertes

### Risques identifiés

| Risque | Impact | Mitigation |
|--------|--------|------------|
| Explosion du nombre de colonnes EF Core (owned entity → 20+ colonnes) | Complexité migration, taille de la table | Les colonnes sont nullable avec defaults → pas de problème de perf. Alternative : table séparée 1:1 si >30 colonnes |
| Templates YAML complexes avec beaucoup de conditionnels | Lisibilité des templates générés | Découper en step templates atomiques — un template par feature |
| Détection de framework nécessite accès au repo Git | Fonctionnalité dégradée sans PAT | Mode dégradé : détection désactivée, configuration manuelle seule |
| Tâches Azure DevOps (SonarQube, PublishTestResults) requièrent des extensions installées | Pipeline échoue si extension manquante | Documenter les prérequis dans le pipeline généré (commentaires YAML) |
| Compatibilité multi-version des tâches Azure DevOps | Tâches dépréciées dans le temps | Utiliser les versions les plus récentes (`@2`, `@3`) et mettre à jour périodiquement |

### Décisions ouvertes

| # | Question | Options | Recommandation |
|---|----------|---------|----------------|
| D1 | Stocker les options comme owned entity ou table séparée ? | Owned entity (colonnes sur la table compute) vs Table `PipelineStepOptions` avec FK | **Owned entity** — Plus simple, pas de join, conforme au pattern DDD existant |
| D2 | Granularité de la détection : par ressource ou par config ? | Par ressource (repo unique) vs par config (cross-resource) | **Par ressource** — Chaque compute resource a son propre `SourceCodePath` |
| D3 | Supporter SonarCloud ET SonarQube Server ? | Les deux vs SonarCloud seul | **Les deux** — Discriminant par `SonarOrganization` (null = SonarQube Server) |
| D4 | Où placer le step "test" dans le mode container ? | Avant le `docker build` (host) vs dans le Dockerfile (multi-stage) | **Avant le docker build** — Plus contrôlable, résultats publiés dans Azure DevOps |
| D5 | Profil de pipeline pré-configurés (templates) ? | Permettre des profils (ex: ".NET Standard", "Node.js Full") vs tout configuré manuellement | **Phase future** — Commencer par la config manuelle + auto-detect, profils dans une phase ultérieure |
| D6 | Les options doivent-elles être modifiables par environnement ? | Options globales par ressource vs options par environnement | **Globales par ressource** — Le CI est le même quel que soit l'env. Smoke tests pourrait être par env dans une phase future |

---

## Annexe A — Fichiers existants de référence

| Fichier | Rôle |
|---------|------|
| `src/Api/InfraFlowSculptor.PipelineGeneration/PipelineGenerationEngine.cs` | Orchestrateur infra pipeline |
| `src/Api/InfraFlowSculptor.PipelineGeneration/AppPipelineGenerationEngine.cs` | Orchestrateur app pipeline |
| `src/Api/InfraFlowSculptor.PipelineGeneration/Generators/App/AppBuildStepEmitter.cs` | Émetteur de steps de build |
| `src/Api/InfraFlowSculptor.PipelineGeneration/Generators/App/AppCiPipelineBuilder.cs` | Builder CI app pipeline |
| `src/Api/InfraFlowSculptor.PipelineGeneration/Generators/App/AppPipelineTemplatesGenerator.cs` | Générateur de templates partagés |
| `src/Api/InfraFlowSculptor.PipelineGeneration/Generators/App/AppReleasePipelineBuilder.cs` | Builder release app pipeline |
| `src/Api/InfraFlowSculptor.GenerationCore/Models/AppPipelineGenerationRequest.cs` | Modèle de requête de génération |
| `src/Api/InfraFlowSculptor.Domain/WebAppAggregate/WebApp.cs` | Agrégat WebApp (compute) |
| `src/Api/InfraFlowSculptor.Domain/FunctionAppAggregate/FunctionApp.cs` | Agrégat FunctionApp (compute) |
| `src/Api/InfraFlowSculptor.Domain/ContainerAppAggregate/ContainerApp.cs` | Agrégat ContainerApp (compute) |
| `src/Front/src/app/features/resource-edit/resource-edit.component.ts` | Composant d'édition de ressource |

## Annexe B — Diagramme de flux de données

```
┌──────────┐     ┌──────────────┐     ┌──────────────────┐     ┌─────────────────────┐
│ Frontend │────▶│ API Endpoint │────▶│ CQRS Handler     │────▶│ Domain Aggregate    │
│ (Angular)│     │ (Minimal API)│     │ (MediatR)        │     │ (WebApp/FuncApp/CA) │
│          │     │              │     │                  │     │                     │
│ Pipeline │     │ POST/PUT     │     │ SetPipelineStep  │     │ PipelineStepOptions │
│ Options  │     │ /resources   │     │ Options()        │     │ (owned entity)      │
│ Panel    │     │              │     │                  │     │                     │
└──────────┘     └──────────────┘     └──────────────────┘     └──────────┬──────────┘
                                                                          │
                                                                          │ Read via
                                                                          │ ReadRepository
                                                                          ▼
┌──────────┐     ┌──────────────┐     ┌──────────────────┐     ┌─────────────────────┐
│ Pipeline │◀────│ Generator    │◀────│ AppPipelineReq   │◀────│ AppPipelineRequest  │
│ YAML     │     │ Engine       │     │ Factory          │     │ Factory             │
│ Output   │     │              │     │                  │     │                     │
│ ci.yml   │     │ Templates +  │     │ Maps domain      │     │ Maps domain → DTO   │
│ steps/*  │     │ Conditionals │     │ options to        │     │                     │
│          │     │ ${{ if }}    │     │ generation req   │     │                     │
└──────────┘     └──────────────┘     └──────────────────┘     └─────────────────────┘


┌──────────┐     ┌──────────────┐     ┌──────────────────┐
│ Frontend │────▶│ API Endpoint │────▶│ Detection        │
│ (Angular)│     │              │     │ Service          │
│          │     │ GET /detect  │     │                  │
│ [Auto-   │     │ -pipeline-   │     │ Analyse repo Git │
│  detect] │     │ options      │     │ .csproj, pkg.json│
│ button   │     │              │     │ requirements.txt │
└──────────┘     └──────────────┘     └──────────────────┘
```
