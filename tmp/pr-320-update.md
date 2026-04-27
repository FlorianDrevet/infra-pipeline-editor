# PR #320 — Mise à jour titre + description

## Titre

```
refactor(pipeline-generation): decompose engines into staged pipeline architecture
```

---

## Description (à coller dans le body de la PR)

## 🎯 But principal

Refactoring complet "Vague 1" de la génération de pipelines YAML Azure DevOps : décomposition des engines monolithiques (`BootstrapPipelineGenerationEngine`, `PipelineGenerationEngine`) en architecture staged (pipeline + stages ordonnés), alignée sur le pattern déjà appliqué à la génération Bicep. Suppression du dead code et extraction de helpers focalisés.

---

## 📋 Type de changement

- [ ] `feat` — Nouvelle fonctionnalité
- [ ] `fix` — Correction de bug
- [x] `refactor` — Refactoring sans changement fonctionnel
- [ ] `perf` — Amélioration des performances
- [ ] `docs` — Documentation uniquement
- [x] `test` — Ajout ou modification de tests
- [ ] `chore` — Maintenance, dépendances, CI
- [ ] `ci` — Changements liés aux pipelines CI/CD
- [ ] `style` — Formatage, lint (sans impact fonctionnel)
- [ ] `revert` — Annulation d'un commit précédent

---

## 🗂️ Changements par couche

### GenerationCore (`InfraFlowSculptor.GenerationCore`)

- Ajout `Models/DeploymentModes.cs` — constantes string miroir de `DeploymentMode` (découplage Domain)
- Ajout `Models/AcrAuthModes.cs` — constantes string miroir de `AcrAuthMode` (découplage Domain)
- Ajout `Models/AppPipelineMode.cs` — enum miroir de `Domain.AppPipelineMode` (découplage Domain)

### PipelineGeneration (`InfraFlowSculptor.PipelineGeneration`)

**Vague 1.0 — Domain decoupling**
- `InfraFlowSculptor.PipelineGeneration.csproj` : suppression `ProjectReference` vers `InfraFlowSculptor.Domain` (seule ref restante : `GenerationCore`)
- Ajout `InternalsVisibleTo` pour le projet de tests

**Vague 1.1 — BootstrapPipelineGenerationEngine staged**
- `BootstrapPipelineGenerationEngine.cs` : 387 LOC → 68 LOC facade (délègue à `BootstrapPipeline`)
- Ajout `Bootstrap/IBootstrapPipelineStage.cs` — interface de stage
- Ajout `Bootstrap/BootstrapPipelineContext.cs` — contexte partagé entre stages
- Ajout `Bootstrap/BootstrapPipeline.cs` — orchestrateur exécutant les stages ordonnés
- Ajout `Bootstrap/BootstrapYamlHelpers.cs` — helpers YAML extraits
- Ajout 6 stages sous `Bootstrap/Stages/` :
  - `HeaderEmissionStage` (order 100)
  - `ValidateSharedResourcesJobStage` (order 200)
  - `PipelineProvisionJobStage` (order 300)
  - `EnvironmentProvisionJobStage` (order 400)
  - `VariableGroupProvisionJobStage` (order 500)
  - `NoOpFallbackStage` (order 999)

**Vague 1.2 — PipelineGenerationEngine staged**
- `PipelineGenerationEngine.Generate()` : méthode réduite à ~15 LOC facade (délègue à `InfraPipeline`)
- `GenerateSharedTemplates(...)` et `AppendPool(...)` : inchangés (statiques)
- Ajout `Infra/IInfraPipelineStage.cs` — interface de stage
- Ajout `Infra/InfraPipelineContext.cs` — contexte partagé
- Ajout `Infra/InfraPipeline.cs` — orchestrateur
- Ajout 5 stages sous `Infra/Stages/` :
  - `CiPipelineStage` (order 100)
  - `PrPipelineStage` (order 200)
  - `ReleasePipelineStage` (order 300)
  - `ConfigVarsStage` (order 400)
  - `EnvironmentVarsStage` (order 500)

**Vague 1.3 — Dead code removal + helpers extraction**
- Supprimé `AppPipelineYamlHelper.cs` (453 LOC dead code, 0 appelant)
- `AppPipelineBuilderCommon.cs` (298 LOC) éclaté en 3 helpers :
  - `AppNamingHelper.cs` (133 LOC) — naming, resolution, escaping
  - `AppHeaderEmitter.cs` (105 LOC) — CI/release headers, env var refs, constants
  - `AppBuildStepEmitter.cs` (178 LOC) — SDK setup, code build steps, admin creds
- Supprimé `AppPipelineBuilderCommon.cs`
- Callers mis à jour : `AppCiPipelineBuilder.cs`, `AppReleasePipelineBuilder.cs`
- Net : −335 LOC dead code supprimé

### Application (`InfraFlowSculptor.Application`)

- `DependencyInjection.cs` : enregistrement DI des 11 stages + 2 pipelines en singleton
- `InfrastructureConfig/Commands/GeneratePipeline/` : alias `using AppPipelineMode = GenerationCore.Models.AppPipelineMode` dans les handlers (chirurgical, 0 changement fonctionnel)

### Tests (`tests/InfraFlowSculptor.PipelineGeneration.Tests/`)

- 83 golden files capturés sous `GoldenFiles/` (parité byte-for-byte avec output actuel)
- `Common/GoldenFileAssertion.cs` — helper assertion avec mode capture (`IFS_UPDATE_GOLDEN=true`)
- 3 fixture classes synthétiques déterministes
- 44 golden parity tests (Bootstrap + Engine + AppPipeline + MonoRepo)
- 25 Bootstrap stage unit tests
- 22 Infra stage unit tests
- Verrou R2 : `AppPipelineSharedTemplatesStabilityTests` (20 paths frozen)
- **Total : 91/91 tests verts**

### Docs

- `docs/architecture/pipeline-refactoring/00-PLAN.md` — plan de référence Vague 1

---

## 🗄️ Base de données

- [ ] Cette PR inclut une **migration EF Core**
- [x] Aucune migration nécessaire

---

## ✅ Checklist avant merge

- [x] Build réussi : `dotnet build .\InfraFlowSculptor.slnx`
- [x] Aucun avertissement de compilation introduit
- [x] Conventions DDD respectées (agrégat / entité / value object)
- [x] Handlers retournent `ErrorOr<T>` (pas d'exception pour les erreurs métier)
- [x] Validators FluentValidation créés pour chaque commande
- [x] Repositories utilisés depuis les handlers (pas d'appel EF Core direct)
- [x] Endpoints enregistrés dans `Program.cs`
- [x] Mapping Mapster configuré via `IRegister`
- [ ] `MEMORY.md` mis à jour avec les nouveautés
- [x] Duplication de code < 3% (seuil qualité SonarQube)

---

## 📊 Métriques

| Métrique | Valeur |
|----------|--------|
| Tests totaux | 91/91 verts |
| Golden files | 83 (parité byte-for-byte) |
| LOC supprimées (dead code) | −335 |
| Stages créés | 11 (6 Bootstrap + 5 Infra) |
| Pipelines orchestrateurs | 2 (`BootstrapPipeline`, `InfraPipeline`) |

## 🛡️ Couverture des risques

| Risque | Mitigation |
|--------|-----------|
| R1 — Régression parité | 83 golden files sentinels, 44 parity tests |
| R2 — `AppPipelineFileClassifier` frozen set | `AppPipelineSharedTemplatesStabilityTests` (20 paths exacts) |
| Backward compat | Constructeurs parameterless préservés pour instanciation directe en tests |

---

## 🔗 Issues / tickets liés

<!-- ADO link à ajouter : Fixes AB#<ID_US> / AB#<ID_EPIC> -->
