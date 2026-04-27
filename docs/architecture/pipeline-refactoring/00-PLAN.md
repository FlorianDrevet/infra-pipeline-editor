# Plan d'implémentation — Refactor PipelineGeneration (alignement Bicep)

> **Statut :** Validé par l'utilisateur le 2026-04-27 (Plan A + toutes recommandations).
> **Contraintes globales :** 0 cassure de signature publique, **parité byte-for-byte** sur la sortie YAML, TDD obligatoire (skill `tdd-workflow` + `xunit-unit-testing`).
> **Décisions utilisateur :**
> 1. Parité strictement byte-for-byte (miroir Bicep).
> 2. Vague 2 (IR YAML) **différée** — décision à réévaluer après Vague 1 mergée.
> 3. `AppPipelineYamlHelper` : suppression sèche après vérification `gitnexus_context` en Vague 1.3.1.
> 4. Suppression dépendance Domain : appliquée dès Vague 1.0.
> 5. Fixtures golden files : **synthétiques minimales** (déterministes, pas basées sur snapshot projet).
> 6. `MonoRepoPipelineAssembler` : **hors refactor staged** (orchestrateur cross-config conservé tel quel).

---

## A. Diagnostic & faisabilité

### A.1 Tableau comparatif Bicep avant refactor / Pipeline aujourd'hui

| Dimension | Bicep avant Vague 1 | Pipeline aujourd'hui |
|---|---|---|
| **God Object principal** | `BicepGenerationEngine` (920 LOC) | `PipelineGenerationEngine` (677 LOC) + `BootstrapPipelineGenerationEngine` (387 LOC) + `AppPipelineBuilderCommon` (847 LOC) |
| **Strategy déjà en place** | OK `IResourceTypeBicepGenerator` (18 generators) | Partiel : `IAppPipelineGenerator` (5 generators) ; pas pour `PipelineGenerationEngine` ni Bootstrap |
| **Façade fine** | NON avant Vague 1 | OK pour `AppPipelineGenerationEngine` (150 LOC) ; pas pour les deux autres |
| **Helpers extraits** | NON → OK `TextManipulation/` (6 helpers) | NON, tout inline + statique privée. `AppPipelineYamlHelper` cohabitant avec `AppPipelineBuilderCommon` (duplication suspecte) |
| **Templates `const string`** | NON, généré 100% inline | OK `AppPipelineTemplatesGenerator` (299 LOC raw strings) |
| **IR / Builder** | NON → OK Vague 2 Phase 0 (`Ir/`) | NON aucun |
| **Tests** | OK 175+ tests | 1 seul test (`BootstrapPipelineGenerationEngineTests`) |
| **Domain dependency** | NON pure | OUI csproj référence `InfraFlowSculptor.Domain` (à supprimer) |
| **`InternalsVisibleTo`** | OK | NON (à ajouter) |
| **Magic strings** | OK `AzureResourceTypes.ComputeArmTypes` | `"DOTNETCORE"`, `"NODE"`, `"PYTHON"`, `"JAVA"`, `"Container"`, `"Code"`, `"AcrImport"`, `"ServiceConnection"`, `"AdminCredentials"` éparpillés |

### A.2 Transposabilité des 3 patterns Bicep

**(a) Pipeline staged — APPLICABLE avec adaptation** (par fichier généré pour `PipelineGenerationEngine`, par job pour `BootstrapPipelineGenerationEngine`, pas de re-staging pour `AppPipelineGenerationEngine`).

**(b) Helpers purs extraits — APPLICABLE et URGENT** : découpage proposé sous `YamlEmission/` (7 helpers) et `AppPipelineSteps/` (5 helpers).

**(c) Builder + IR (Vague 2) — APPLICABLE mais DIFFÉRÉ** (décision validée par l'utilisateur).

À NE PAS faire : YamlDotNet (casse parité), modéliser TOUS les artefacts en une passe, couvrir les templates raw `const string` en IR.

### A.3 Décision csproj retenue

**Option (a)** : Sortir `Domain` de `PipelineGeneration.csproj` en remplaçant par des constantes string dans `GenerationCore`. Détails :
- Créer `GenerationCore/Models/DeploymentModes.cs` (`Code`, `Container` + `IReadOnlySet<string> All`).
- Créer `GenerationCore/Models/AcrAuthModes.cs` (`ServiceConnection`, `AdminCredentials`, `ManagedIdentity`).
- Retirer `<ProjectReference Include="..\InfraFlowSculptor.Domain\..." />` de `PipelineGeneration.csproj`.
- Ajouter `<InternalsVisibleTo Include="InfraFlowSculptor.PipelineGeneration.Tests" />`.

### A.4 Risques identifiés

| # | Risque | Mitigation |
|---|---|---|
| R1 | Casse parité byte-for-byte | Golden files capturés AVANT tout refactor |
| R2 | `AppPipelineFileClassifier.AppSharedTemplatePaths` set figé | Aucun changement de chemins shared en Vague 1. Test verrou : 20 entrées exactes |
| R3 | Méthode statique `GenerateSharedTemplates` consommée par 3 sites | Garder le contrat statique inchangé, déléguer en interne |
| R4 | `AppendPool` statique cross-fichier consommé via `using static` | Garder un proxy `internal static` pendant la transition |
| R5 | Mode `ApplicationOnly` vs `FullOwner` Bootstrap | Tests de régression : 4 cas (FullOwner vide/complet, ApplicationOnly vide/complet) |
| R6 | Test existant `BootstrapPipelineGenerationEngineTests` doit rester vert | Ne pas modifier sa logique en Vague 1.0 |
| R7 | Suppression dépendance Domain | Audit grep préalable (étape 1.1) |

---

## B. Plan d'implémentation par vagues

### Vague 1.0 — Préparation parité (golden files + Domain decoupling)

**Objectif** : sécuriser le refactor en capturant la sortie actuelle et en supprimant la dépendance Domain.
**Public surface préservée** : 100 % (changements internes uniquement).

#### Étape 1.0.1 — Audit Domain dependency

Action : `gitnexus_query("DeploymentMode AcrAuthMode usage")` + grep_search dans `src/Api/InfraFlowSculptor.Application/**/*.cs`.

#### Étape 1.0.2 — Constantes neutres + suppression Domain ref

Fichiers à créer :
- `src/Api/InfraFlowSculptor.GenerationCore/Models/DeploymentModes.cs`
- `src/Api/InfraFlowSculptor.GenerationCore/Models/AcrAuthModes.cs`

Fichiers à modifier :
- `src/Api/InfraFlowSculptor.PipelineGeneration/AppPipelineGenerationEngine.cs` (remplacer `Enum.TryParse<DeploymentMode.DeploymentModeType>`)
- `src/Api/InfraFlowSculptor.PipelineGeneration/Generators/App/AppPipelineBuilderCommon.cs` (remplacer `AcrAuthMode.AcrAuthModeType.AdminCredentials.ToString()`)
- `src/Api/InfraFlowSculptor.PipelineGeneration/Generators/App/AppPipelineYamlHelper.cs` (idem)
- `src/Api/InfraFlowSculptor.PipelineGeneration/InfraFlowSculptor.PipelineGeneration.csproj` (supprimer Domain ref + ajouter `InternalsVisibleTo`)

Tests RED→GREEN :
- `Given_KnownDeploymentMode_When_All_Then_ContainsCodeAndContainer`
- `Given_KnownAcrAuthMode_When_All_Then_ContainsAllThreeModes`
- `Given_InvalidDeploymentMode_When_Generate_Then_ThrowsArgumentException`

#### Étape 1.0.3 — Capture des golden files

**Stratégie de comparaison** : `string.Equals(actualNormalized, expectedNormalized, StringComparison.Ordinal)` après normalisation `\r\n`→`\n`. Pas de Verify.

Helper : `tests/InfraFlowSculptor.PipelineGeneration.Tests/Common/GoldenFileAssertion.cs` exposant `AssertMatches(string actual, string goldenRelativePath)`. Variable d'environnement `IFS_UPDATE_GOLDEN=true` pour réécrire les goldens intentionnellement.

Fichiers à créer :
- `Fixtures/PipelineRequestFixtures.cs` (factories synthétiques minimales)
- `Common/GoldenFileAssertion.cs`
- `GoldenFiles/Engine/{standalone-default,monorepo-default,standalone-with-vargroups}/...`
- `GoldenFiles/Engine/shared-templates/...`
- `GoldenFiles/Bootstrap/{full-owner-complete,full-owner-pipelines-only,application-only-complete,application-only-pipelines-only,empty-noop}/bootstrap.pipeline.yml`
- `GoldenFiles/AppPipeline/{container-app-isolated,web-app-code-combined,web-app-container-admin-credentials,function-app-code,function-app-container,shared-templates}/...`
- `GoldenFiles/MonoRepo/two-configs/{CommonFiles,ConfigFiles}/...`
- `GoldenFileTests/PipelineGenerationEngineGoldenTests.cs`
- `GoldenFileTests/BootstrapPipelineGenerationEngineGoldenTests.cs`
- `GoldenFileTests/AppPipelineGenerationEngineGoldenTests.cs`
- `GoldenFileTests/MonoRepoPipelineAssemblerGoldenTests.cs`
- `GoldenFileTests/AppPipelineSharedTemplatesStabilityTests.cs` (verrou R2 : 20 chemins exacts)

Tests `Given_When_Then` (≈ 30) : voir prompt original.

#### Étape 1.0.4 — Test debt baseline

Mettre à jour `.github/test-debt.md` avec coverage baseline (`dotnet test --collect:"XPlat Code Coverage"`).

---

### Vague 1.1 — Décomposition `BootstrapPipelineGenerationEngine`

**Objectif** : façade ~40 LOC + `BootstrapPipeline` orchestrant 6 stages.

Stages (Order 100/200/300/400/500/999) :
- `HeaderEmissionStage`
- `ValidateSharedResourcesJobStage` (conditionnel `Mode==ApplicationOnly` + counts > 0)
- `PipelineProvisionJobStage` (injecte `dependsOn` quand `ApplicationOnly` + validation présente)
- `EnvironmentProvisionJobStage` (conditionnel `Mode==FullOwner`)
- `VariableGroupProvisionJobStage` (conditionnel `Mode==FullOwner`)
- `NoOpFallbackStage`

Helpers : `Bootstrap/Steps/{BootstrapAzCliConfigureStep,BootstrapPipelineCreationStep,BootstrapEnvironmentCreationStep,BootstrapVariableGroupCreationStep,BootstrapValidationStep,BootstrapNoOpStep}.cs`.

DI : enregistrer chaque stage en `AddSingleton<IBootstrapPipelineStage, XxxStage>()` puis `AddSingleton<BootstrapPipeline>()`.

≈ 25 nouveaux tests RED→GREEN par stage. Golden files Vague 1.0 doivent rester verts.

---

### Vague 1.2 — Décomposition `PipelineGenerationEngine`

**Objectif** : `Generate(...)` ~25 LOC, 5 stages + `PipelineGenerationPipeline`, helpers YAML purs sous `YamlEmission/`.

#### Étape 1.2.1 — Extraction helpers YAML

`src/Api/InfraFlowSculptor.PipelineGeneration/YamlEmission/` :
- `YamlIndent.cs`, `YamlScalar.cs`, `YamlPoolEmitter.cs`, `YamlPathBuilder.cs`, `YamlTriggerEmitter.cs`, `YamlVariablesEmitter.cs`, `YamlOverrideParametersBuilder.cs`

#### Étape 1.2.2 — Pipeline staged + contexte

Stages :
- `CiPipelineStage` (Order 100)
- `PrPipelineStage` (Order 200)
- `ReleasePipelineStage` (Order 300)
- `ConfigVarsStage` (Order 400, conditionnel `!IsMonoRepo`)
- `EnvironmentVarsStage` (Order 500, conditionnel `!IsMonoRepo`)

#### Étape 1.2.3 — Refactor `GenerateSharedTemplates`

Garder signature publique inchangée, déléguer à `SharedTemplatesAssembler` interne.

---

### Vague 1.3 — Décomposition `AppPipelineBuilderCommon`

**Objectif** : éclater 847 LOC en helpers focalisés sous `AppPipelineSteps/`.

#### Étape 1.3.1 — Audit duplication

`gitnexus_context("AppPipelineYamlHelper")` pour confirmer dead code → suppression sèche.

#### Étape 1.3.2 — Extraction `AppPipelineSteps/`

Fichiers à créer :
- `AppHeaderEmitter.cs`, `SdkSetupStep.cs`, `DotNetBuildSteps.cs`, `GenericCodeBuildSteps.cs`, `DockerBuildSteps.cs`, `AcrAuthSteps.cs`, `AppEnvironmentReferenceEmitter.cs`, `AppOverrideParametersBuilder.cs`, `AppArtifactNamingHelper.cs`

`AppPipelineBuilderCommon.cs` : ≤ 100 LOC ou supprimé. `AppPipelineYamlHelper.cs` : supprimé.

---

### Vague 2 — IR + Builder + Emitter (DIFFÉRÉE)

À réévaluer après Vague 1 mergée. Esquisse minimale :

```csharp
namespace InfraFlowSculptor.PipelineGeneration.Ir;

public sealed record AzdoPipelineSpec(...);
public sealed record AzdoStage(...);
public sealed record AzdoJob(...);
public abstract record AzdoStep(string DisplayName);
// + AzdoPowerShellStep, AzdoTaskStep, AzdoTemplateStep, AzdoEachStep

new AzdoPipelineBuilder()
    .Pipeline().Name("$(Date:yyyyMMdd)").Trigger(t => t.Branches("main", "release/*"))
    .Pool(p => p.SelfHosted(agentPoolName))
    .Stage("Build", s => s.Job("BuildBicep", j => j.Step(...)))
    .Build();

public static class AzdoYamlEmitter { public static string Emit(AzdoPipelineSpec spec); }
public static class LegacyYamlAdapter { public static string EmitOrFallback(AzdoPipelineSpec? spec, string legacyContent); }
```

---

## C. Stratégie de parité

### C.1 Mise en place
- **Quand** : Vague 1.0 étape 1.0.3, AVANT tout refactor de production.
- **Où** : `tests/InfraFlowSculptor.PipelineGeneration.Tests/GoldenFiles/`.
- **Comment embarquer** : `<None Include="GoldenFiles\**\*" CopyToOutputDirectory="PreserveNewest" />`.

### C.2 Stratégie de comparaison
- `string.Equals(...,StringComparison.Ordinal)` après `\r\n`→`\n`.
- Pas de Verify dans Vague 1.
- Helper `GoldenFileAssertion.AssertMatches` qui dump l'`actual` dans `bin/.../GoldenFiles/__actual__/` en cas d'échec + `actual.Should().Be(expected)`.
- Variable `IFS_UPDATE_GOLDEN=true` pour réécriture intentionnelle.

### C.3 Couverture minimale (golden matrix)

| Engine | Combinaisons |
|---|---|
| `PipelineGenerationEngine` | (standalone × default) + (standalone × variableGroups+secureParams) + (monorepo × default) + sharedTemplates |
| `BootstrapPipelineGenerationEngine` | FullOwner complet/pipelinesOnly + ApplicationOnly complet/pipelinesOnly + empty |
| `AppPipelineGenerationEngine` | 5 generators × isolated + combined ; container × ServiceConnection + AdminCredentials ; sharedTemplates (20 fichiers exacts) |
| `MonoRepoPipelineAssembler` | 2 configs + 3 envs |

### C.4 Règle d'or

Aucun PR de Vague 1.x ne peut être mergé si un seul golden casse. Un golden modifié intentionnellement = PR séparée `chore(tests): update golden X for reason Y` validée explicitement.

---

## D. Synthèse pour MEMORY.md (à ajouter après merge de chaque vague)

- **Après Vague 1.0** : `InfraFlowSculptor.PipelineGeneration.csproj` ne référence plus `Domain` ; `DeploymentModes` + `AcrAuthModes` ajoutés à `GenerationCore.Models` ; golden files de référence sous `tests/InfraFlowSculptor.PipelineGeneration.Tests/GoldenFiles/`.
- **Après Vague 1.1** : `BootstrapPipelineGenerationEngine` est une façade ~40 LOC sur `BootstrapPipeline` ; 6 stages registrés en DI (Order 100/200/300/400/500/999).
- **Après Vague 1.2** : `PipelineGenerationEngine.Generate` ~25 LOC ; 5 stages + `PipelineGenerationPipeline` ; helpers YAML purs sous `YamlEmission/`.
- **Après Vague 1.3** : `AppPipelineBuilderCommon` ≤ 100 LOC ou supprimé ; `AppPipelineYamlHelper` supprimé ; helpers focalisés sous `AppPipelineSteps/`.
