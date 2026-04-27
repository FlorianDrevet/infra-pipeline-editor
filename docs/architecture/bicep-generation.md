# Génération Bicep — guide complet d’architecture, de lecture et d’apprentissage

## Pourquoi ce guide existe

Si tu arrives sur le projet aujourd’hui, la génération Bicep peut sembler imposante pour trois raisons :

1. elle traverse plusieurs couches de la solution ;
2. elle a récemment été refactorée en profondeur ;
3. elle mélange volontairement plusieurs patterns .NET utiles à apprendre : orchestration applicative, stratégie, pipeline, builder, IR immuable, adapter, helpers purs et assemblage de fichiers.

L’objectif de ce document est donc double :

- **t’expliquer comment la génération Bicep fonctionne de bout en bout** ;
- **te donner un chemin de lecture pas à pas** pour apprendre le code et les patterns du projet sans te perdre.

---

## Ce que la génération Bicep fait réellement

Le projet ne génère pas du Bicep à partir de templates statiques branchés directement sur l’UI.

Il suit ce chemin :

1. le domaine décrit une infrastructure Azure ;
2. la couche Application transforme ce modèle métier en **modèles de génération** ;
3. le projet `InfraFlowSculptor.BicepGeneration` convertit ces modèles en un **arbre abstrait de fichiers Bicep** ;
4. la couche Application publie ensuite ces fichiers dans le stockage de blobs pour prévisualisation, téléchargement ou push Git.

Autrement dit :

- **le domaine décide ce qui existe** ;
- **l’Application décide quoi envoyer au moteur** ;
- **le moteur Bicep décide comment émettre les fichiers** ;
- **les handlers applicatifs décident où les fichiers vont**.

Cette séparation est un choix important du projet.

---

## Le bon modèle mental

Aujourd’hui, il faut penser la génération Bicep en **4 niveaux** :

1. **Entrée applicative**  
   `GenerateBicepCommandHandler` et `GenerateProjectBicepCommandHandler` construisent un `GenerationRequest`.

2. **Orchestration du moteur**  
   `BicepGenerationEngine` lance un pipeline ordonné de stages.

3. **Production des modules par ressource**  
   chaque générateur produit une représentation de module Bicep pour un type ARM donné.

4. **Assemblage final**  
   `BicepAssembler` et `MonoRepoBicepAssembler` construisent les fichiers finaux : `main.bicep`, `types.bicep`, `functions.bicep`, `constants.bicep`, `modules/...`, `parameters/...`.

---

## Pourquoi un projet dédié `InfraFlowSculptor.BicepGeneration`

Le moteur vit dans :

`src/Api/InfraFlowSculptor.BicepGeneration/`

Ce découpage a été fait pour de bonnes raisons :

- éviter de mélanger le code de génération avec les agrégats DDD ;
- rendre le moteur **pur et réutilisable** ;
- isoler les patterns de génération ;
- rendre les tests plus ciblés ;
- permettre à la couche Application de rester responsable des cas d’usage, pas du détail de l’émission Bicep.

Le projet de génération **ne dépend pas du domaine**. Il dépend seulement de :

- `InfraFlowSculptor.GenerationCore` pour les constantes et modèles transverses de génération ;
- des modèles de génération (`GenerationRequest`, `ResourceDefinition`, etc.) ;
- de ses propres assemblers, stages, générateurs et IR.

---

## Lire le `.csproj` pour comprendre le périmètre du projet

Point d’entrée :

`src/Api/InfraFlowSculptor.BicepGeneration/InfraFlowSculptor.BicepGeneration.csproj`

Ce `.csproj` est volontairement minimal.

### Ce qu’il t’apprend

#### 1. Le projet cible .NET 10

- `TargetFramework`: `net10.0`
- `Nullable`: activé
- `ImplicitUsings`: activé

Donc tout le code est écrit avec les conventions modernes du projet.

#### 2. Il expose ses `internal` aux tests

- `InternalsVisibleTo Include="InfraFlowSculptor.BicepGeneration.Tests"`

Choix important : les helpers et briques internes restent cachés au reste de la solution, **mais sont testables**.

#### 3. Il n’a qu’une dépendance projet

- `..\InfraFlowSculptor.GenerationCore\InfraFlowSculptor.GenerationCore.csproj`

Cela confirme que le moteur est **isolé du domaine, de l’infrastructure et de l’API**.

#### 4. Les versions de packages ne sont pas dans le `.csproj`

Le repo utilise la **Central Package Management** via :

- `Directory.Packages.props`

Donc quand tu lis un `.csproj` dans ce projet :

- les `<PackageReference Include="..." />` donnent le package ;
- les versions se lisent dans `Directory.Packages.props`.

### Comment naviguer intelligemment à partir du `.csproj`

Quand tu ouvres ce projet pour apprendre :

1. lis le `.csproj` ;
2. ouvre le `ProjectReference` vers `GenerationCore` si tu veux comprendre les types partagés ;
3. ouvre ensuite le projet de tests :
   `tests/InfraFlowSculptor.BicepGeneration.Tests/InfraFlowSculptor.BicepGeneration.Tests.csproj` ;
4. vérifie dans `Directory.Packages.props` les dépendances de test (`xUnit`, `FluentAssertions`, `NSubstitute`) ;
5. reviens au projet principal pour lire le code dans l’ordre décrit plus bas.

---

## La structure du projet BicepGeneration

Voici la carte mentale utile :

- `BicepGenerationEngine.cs`  
  façade haut niveau ;
- `Pipeline/`  
  orchestration par étapes ;
- `Generators/`  
  un générateur par type de ressource ;
- `Ir/`  
  modèle intermédiaire typé, builder, transformations, emitter ;
- `Assemblers/`  
  génération des fichiers finaux ;
- `Helpers/`  
  fonctions pures de support ;
- `TextManipulation/`  
  primitives legacy conservées pour la compatibilité historique ;
- `Models/`  
  DTOs de génération ;
- `MonoRepoBicepAssembler.cs`  
  variante d’assemblage multi-config.

Le refactoring récent a justement clarifié cette organisation.

---

## Le flux complet de génération, pas à pas

## Étape 1 — Le handler Application construit `GenerationRequest`

Commence toujours par :

- `src/Api/InfraFlowSculptor.Application/InfrastructureConfig/Commands/GenerateBicep/GenerateBicepCommandHandler.cs`
- `src/Api/InfraFlowSculptor.Application/Projects/Commands/GenerateProjectBicep/GenerateProjectBicepCommandHandler.cs`

Ce sont les meilleurs points d’entrée pour comprendre **ce que le moteur reçoit vraiment**.

### Ce qu’ils font

- chargent la configuration depuis les repositories ;
- vérifient les droits d’accès ;
- convertissent les read models/domain models vers :
  - `ResourceDefinition`
  - `ResourceGroupDefinition`
  - `EnvironmentDefinition`
  - `RoleAssignmentDefinition`
  - `AppSettingDefinition`
  - `ExistingResourceReference`
- construisent un `GenerationRequest` ;
- appellent `BicepGenerationEngine` ;
- uploadent ensuite les fichiers générés dans le blob storage.

### Pourquoi ce choix est bon

Le moteur Bicep n’a pas à connaître :

- EF Core ;
- les agrégats ;
- les permissions ;
- le blob storage ;
- les repos Git.

Il reçoit uniquement un modèle de génération déjà propre.

**Pattern .NET à retenir :** isoler le use case dans l’Application et envoyer au moteur un contrat simple, stable et testable.

---

## Étape 2 — La DI déclare la composition du moteur

Lis ensuite :

`src/Api/InfraFlowSculptor.Application/DependencyInjection.cs`

C’est ici que tu comprends la vraie composition du moteur.

### Ce qu’il faut observer

- les **18 générateurs** sont enregistrés comme `IResourceTypeBicepSpecGenerator` ;
- les **stages** du pipeline sont enregistrés comme `IBicepGenerationStage` ;
- `BicepGenerationPipeline` et `BicepGenerationEngine` sont ensuite déclarés.

### Pourquoi c’est important

Le projet ne dépend pas d’un gros orchestrateur codé “en dur”.

Il dépend d’une **composition par DI**, ce qui permet :

- d’ajouter un nouveau générateur sans toucher l’engine ;
- d’ajouter un nouveau stage sans casser le reste ;
- de tester chaque brique isolément ;
- de garder un ordre de traitement explicite.

### Détail clé

L’ordre d’exécution des stages **ne dépend pas de l’ordre d’enregistrement** :

- `BicepGenerationPipeline` trie les stages par `Order`.

C’est un très bon pattern .NET pour éviter les dépendances implicites et fragiles.

---

## Étape 3 — `BicepGenerationEngine` reste une façade mince

Lis :

`src/Api/InfraFlowSculptor.BicepGeneration/BicepGenerationEngine.cs`

Le refactoring a volontairement réduit cette classe à un rôle simple :

- construire le `BicepGenerationContext` ;
- lancer le pipeline ;
- récupérer `GenerationResult` ;
- faire le **pruning final des outputs** ;
- gérer le mode mono-repo.

### Pourquoi ce choix a été fait

Avant le refactoring, l’engine était un gros objet central.

Maintenant :

- l’engine orchestre ;
- les stages transforment ;
- les assemblers composent.

**Pattern à retenir :** une façade de haut niveau doit rester lisible, stable et peu intelligente.

### Point important

Le pruning des outputs reste **dans l’engine**, pas dans un stage.

Pourquoi :

- le pruning single-config et mono-repo n’ont pas exactement la même logique ;
- le mono-repo a besoin d’une vision globale de plusieurs `main.bicep`.

Donc le projet a explicitement gardé cette responsabilité au plus haut niveau.

---

## Étape 4 — Le pipeline porte la vraie orchestration

Lis :

- `Pipeline/BicepGenerationPipeline.cs`
- `Pipeline/BicepGenerationContext.cs`
- `Pipeline/Stages/`

### Le rôle du pipeline

Le pipeline exécute les stages dans l’ordre croissant de `Order`.

Le contexte transporte l’état mutable de la génération :

- la requête d’origine ;
- les `WorkItems` ;
- les résultats d’analyse d’identité ;
- les résultats d’analyse d’app settings ;
- le mapping ressource → info de type ;
- le résultat final assemblé.

### Pourquoi ce pattern est utilisé

Parce qu’il rend visibles :

- l’ordre des transformations ;
- les préconditions de chaque étape ;
- les dépendances entre étapes ;
- les responsabilités exactes.

**Pattern à retenir :** quand un traitement suit des phases successives bien identifiées, un pipeline explicite est souvent meilleur qu’une seule méthode géante.

---

## Les stages, dans l’ordre exact

### 100 — `IdentityAnalysisStage`

Responsabilité :

- détecter quelles ressources ont besoin d’identités ;
- distinguer system-assigned et user-assigned ;
- calculer les cas “mixed identity” par type ARM.

Pourquoi cette phase existe :

- l’injection d’identité ne doit pas être pilotée par chaque générateur ;
- elle dépend d’informations globales, pas seulement d’une ressource isolée.

### 200 — `AppSettingsAnalysisStage`

Responsabilité :

- identifier les outputs nécessaires aux app settings ;
- repérer les types compute qui ont besoin d’un paramètre `appSettings` ou `envVars`.

Pourquoi :

- certaines injections dépendent des relations entre ressources ;
- il faut savoir à l’avance quels modules devront exposer des outputs.

### 300 — `ModuleBuildStage`

Responsabilité :

- sélectionner le bon générateur par type ARM ;
- appeler `GenerateSpec(resource)` ;
- récupérer aussi `Generate(resource)` pour les métadonnées legacy encore utiles ;
- créer un `ModuleWorkItem`.

C’est l’étape charnière entre :

- la stratégie par type ;
- le pipeline global.

### 400 — `IdentityInjectionStage`

Responsabilité :

- appliquer l’identité au module.

Aujourd’hui, cette étape agit au niveau IR via des transformations typées.

### 500 — `OutputInjectionStage`

Responsabilité :

- ajouter les outputs requis par les références d’app settings ;
- gérer aussi les outputs sécurisés.

### 600 — `AppSettingsInjectionStage`

Responsabilité :

- injecter `appSettings` pour Web App / Function App ;
- injecter `envVars` pour Container App.

### 700 — `TagsInjectionStage`

Responsabilité :

- injecter le support des tags dans les modules.

### 800 — `ParentReferenceResolutionStage`

Responsabilité :

- résoudre les références inter-ressources ;
- renseigner les dépendances de type :
  - `appServicePlanId`
  - `containerAppEnvironmentId`
  - `logAnalyticsWorkspaceId`
  - `sqlServerId`
- gérer aussi les fallbacks vers les existing resources cross-config.

Cette étape est essentielle pour comprendre les références dans `main.bicep`.

### 850 — `SpecEmissionStage`

Responsabilité :

- convertir le `BicepModuleSpec` en texte via `BicepEmitter` ;
- alimenter `GeneratedTypeModule.ModuleBicepContent` et `ModuleTypesBicepContent`.

Cette étape matérialise le passage :

- du monde structuré et typé ;
- au texte Bicep final.

### 900 — `AssemblyStage`

Responsabilité :

- déléguer à `BicepAssembler.Assemble`.

---

## Ce qu’un `ModuleWorkItem` représente

Le type `ModuleWorkItem` dans `BicepGenerationContext.cs` est central.

Il contient à la fois :

- la ressource source ;
- le `GeneratedTypeModule` attendu par l’assembleur historique ;
- le `BicepModuleSpec` structuré utilisé par la V2 ;
- les métadonnées d’identité et de références parent.

Ce design est important car il permet la coexistence entre :

- **le contrat legacy d’assemblage** ;
- **le nouveau pipeline IR**.

**Choix assumé :** ne pas casser tout le moteur d’un coup, mais migrer en gardant un pont de compatibilité.

---

## Le double contrat `Generate()` / `GenerateSpec()`

Lis :

- `Generators/IResourceTypeBicepGenerator.cs`
- `Generators/IResourceTypeBicepSpecGenerator.cs`

### Pourquoi les deux existent encore

Le projet est en phase post-refacto où :

- l’IR est devenue la voie principale ;
- le contrat legacy n’a pas été supprimé brutalement ;
- certaines métadonnées historiques restent encore lues depuis `Generate()`.

### Ce que ça apporte

- migration progressive ;
- moindre risque de régression ;
- compatibilité avec l’assembleur existant ;
- possibilité de comparer l’ancien et le nouveau mode.

**Pattern à retenir :** en refactoring profond, un adaptateur temporaire vaut souvent mieux qu’un big bang.

---

## Étape 5 — Les générateurs portent la connaissance métier du type Azure

Lis ensuite :

`src/Api/InfraFlowSculptor.BicepGeneration/Generators/`

Un fichier par ressource Azure.

### Ce qu’un générateur doit faire

- déclarer le type ARM supporté ;
- déclarer le nom logique du type ;
- construire le module Bicep ;
- exposer les paramètres, outputs, types exportés et éventuels companions ;
- éventuellement fournir des métadonnées de compatibilité legacy.

### Le meilleur générateur pour apprendre

Commence par :

`Generators/KeyVaultTypeBicepGenerator.cs`

Pourquoi :

- il est assez simple ;
- il montre bien le pattern Builder + IR ;
- il garde encore le `Generate()` legacy, donc tu vois la différence entre l’ancien et le nouveau monde.

Quand tu l’ouvres, regarde dans cet ordre :

1. `ResourceType`
2. `ResourceTypeName`
3. `GenerateSpec()`
4. `Generate()`
5. les types exportés
6. la lecture de `resource.Properties`

### Les générateurs plus avancés à lire ensuite

- `StorageAccountTypeBicepGenerator.cs`  
  pour les companions et les cas multi-modules ;
- `ContainerAppTypeBicepGenerator.cs`  
  pour la complexité compute ;
- `WebAppTypeBicepGenerator.cs` et `FunctionAppTypeBicepGenerator.cs`  
  pour les variantes de modules ;
- `SqlDatabaseTypeBicepGenerator.cs`  
  pour les existing resources et le parent ;
- `ContainerAppEnvironmentTypeBicepGenerator.cs`  
  pour les additional resources et les conditions.

---

## Pourquoi le Builder + IR a été introduit

Le point clé de la refacto est là :

- l’ancien moteur produisait très tôt une **string Bicep** ;
- toutes les mutations suivantes devaient réécrire ce texte ;
- cela amenait des regex, de la fragilité et une lecture difficile.

La nouvelle approche produit d’abord une **représentation intermédiaire typée** :

- `BicepModuleSpec`
- `BicepResourceDeclaration`
- `BicepParam`
- `BicepOutput`
- `BicepExpression`
- etc.

Puis cette IR est transformée avant émission.

### Pourquoi c’est un meilleur design

- les transformations deviennent structurées ;
- les tests deviennent plus simples ;
- l’intention métier est plus lisible ;
- on réduit la dépendance à la forme exacte du texte Bicep.

**Pattern à retenir :** quand on génère un langage structuré, il est souvent plus sain de manipuler un modèle objet intermédiaire qu’une string brute.

---

## Comment lire l’IR sans se noyer

Lis ce dossier dans cet ordre :

1. `Ir/BicepModuleSpec.cs`
2. `Ir/BicepResourceDeclaration.cs`
3. `Ir/BicepExpression.cs`
4. `Ir/BicepType.cs`
5. `Ir/Builder/BicepModuleBuilder.cs`
6. `Ir/Builder/BicepObjectBuilder.cs`
7. `Ir/Transformations/`
8. `Ir/Emit/BicepEmitter.cs`
9. `Ir/LegacyTextModuleAdapter.cs`

### Ce que tu dois comprendre à chaque étape

#### `BicepModuleSpec`

Le conteneur principal d’un module :

- imports ;
- paramètres ;
- variables ;
- existing resources ;
- ressource principale ;
- ressources additionnelles ;
- outputs ;
- types exportés ;
- companions.

#### `BicepExpression`

Le cœur de l’IR.

Tu y vois les différentes formes possibles d’expression :

- littéraux ;
- références ;
- raw expressions ;
- interpolations ;
- fonctions ;
- ternaires ;
- objets ;
- tableaux.

#### `BicepModuleBuilder`

Le builder fluide utilisé par les générateurs pour écrire un module de façon déclarative.

Ce builder améliore :

- la lisibilité ;
- la cohérence ;
- la factorisation ;
- la validation minimale au `Build()`.

#### `Transformations`

Tu vois ici comment des concerns transverses sont appliqués **sans réécrire chaque générateur** :

- identité ;
- outputs ;
- tags ;
- app settings.

#### `BicepEmitter`

Le seul endroit qui décide :

- de l’ordre final du texte ;
- du format ;
- des sauts de ligne ;
- de l’indentation ;
- de l’émission des tableaux, objets, conditions, loops, existing resources.

**Choix très sain :** un seul composant décide du rendu final.

---

## Le rôle de `LegacyTextModuleAdapter`

Lis :

`Ir/LegacyTextModuleAdapter.cs`

Cette classe est capitale pour comprendre la stratégie de migration.

Elle sert à :

- créer un `GeneratedTypeModule` compatible avec l’existant ;
- injecter ensuite le texte émis depuis l’IR dans ce contrat historique.

### Pourquoi ce choix a été fait

Parce que l’assembleur et plusieurs conventions historiques travaillaient déjà avec `GeneratedTypeModule`.

Au lieu de tout casser :

- l’IR pilote la construction ;
- l’adapter nourrit le contrat historique ;
- le reste du moteur continue de vivre pendant la migration.

**Pattern à retenir :** un adapter de transition est souvent le bon outil lors d’un refactoring incrémental.

---

## Étape 6 — L’assembleur transforme des modules en arborescence de fichiers

Lis ensuite :

- `BicepAssembler.cs`
- `MonoRepoBicepAssembler.cs`
- le dossier `Assemblers/`

### Ce que fait `BicepAssembler`

Il ne connaît pas le domaine métier.  
Il reçoit déjà :

- les modules générés ;
- les environnements ;
- les role assignments ;
- les app settings ;
- le naming context ;
- les existing resource references ;
- les tags.

Puis il génère :

- `types.bicep`
- `functions.bicep`
- `constants.bicep`
- `main.bicep`
- `parameters/*.bicepparam`
- `modules/**`

### Ce qu’il faut remarquer

#### 1. Les responsabilités sont spécialisées

Le gros assembleur a été découpé en sous-assembleurs :

- `TypesBicepAssembler`
- `FunctionsBicepAssembler`
- `ConstantsBicepAssembler`
- `MainBicepAssembler`
- `ParameterFileAssembler`
- `KvSecretsModuleAssembler`
- `RoleAssignmentAssembler`

Très bon exemple de **Single Responsibility Principle**.

#### 2. Les modules peuvent être dédupliqués

`NormalizePrimaryModuleFileNames()` gère le cas où plusieurs ressources aboutissent au même chemin mais pas au même contenu.

Le choix fait ici :

- garder le nom partagé si le contenu est identique ;
- ajouter un hash si le contenu diffère.

Cela évite les collisions silencieuses.

#### 3. Le moteur reste repo-agnostique

L’assembleur produit une structure abstraite `path -> content`.

Ce n’est pas lui qui décide :

- si le fichier va dans un repo infra ou app ;
- s’il faut push sur Git ;
- s’il faut l’uploader ailleurs.

Ce choix reste dans l’Application.

---

## Étape 7 — Le mono-repo n’est pas un mode “à part”, c’est un autre assemblage

Lis :

`MonoRepoBicepAssembler.cs`

Le moteur ne réécrit pas tous les générateurs pour le mono-repo.

Il :

1. génère chaque configuration séparément ;
2. collecte les résultats ;
3. déduplique les fichiers communs ;
4. produit `Common/` ;
5. réécrit les `main.bicep` des configs pour pointer vers les chemins partagés.

### Pourquoi c’est un bon design

- le moteur garde une logique simple de génération par configuration ;
- la mutualisation est repoussée à l’étape d’assemblage ;
- cela évite de polluer les générateurs avec des considérations de topologie Git.

**Pattern à retenir :** faire varier l’assemblage final plutôt que les briques élémentaires quand la topologie change.

---

## Les grands choix d’architecture et leur justification

## 1. Garder un moteur de génération séparé du domaine

**Pourquoi :**

- le domaine décrit l’infrastructure ;
- il ne doit pas connaître le langage Bicep.

**Bénéfice :**

- domaine plus stable ;
- génération remplaçable ;
- meilleure testabilité.

## 2. Un générateur par type de ressource

**Pourquoi :**

- chaque ressource Azure a ses propres paramètres, invariants et output patterns.

**Bénéfice :**

- faible couplage ;
- ajout de nouveau type plus simple ;
- lecture par ressource beaucoup plus claire.

## 3. Un pipeline ordonné

**Pourquoi :**

- les transformations globales arrivent après la construction initiale ;
- leur ordre compte.

**Bénéfice :**

- orchestration explicite ;
- meilleure maintenabilité ;
- stages testables isolément.

## 4. Une IR immuable + transformations ciblées

**Pourquoi :**

- manipuler du texte brut est fragile ;
- les concerns transverses ne doivent pas vivre dans tous les générateurs.

**Bénéfice :**

- structure claire ;
- tests plus robustes ;
- transformations plus lisibles.

## 5. Un adapter de compatibilité

**Pourquoi :**

- la migration complète d’un moteur existant ne se fait pas en un seul changement sûr.

**Bénéfice :**

- migration progressive ;
- parité fonctionnelle ;
- réduction du risque.

## 6. L’engine garde le pruning

**Pourquoi :**

- le mono-repo a besoin d’une vision globale de tous les `main.bicep`.

**Bénéfice :**

- responsabilité au bon niveau ;
- moins de couplage entre stage et topologie finale.

## 7. Les versions de packages sont centralisées

**Pourquoi :**

- homogénéiser tous les projets ;
- éviter les divergences de versions.

**Bénéfice :**

- maintenance plus simple ;
- lecture plus cohérente de la solution.

---

## Guide de lecture pas à pas pour un nouveau développeur

Voici l’ordre recommandé.

## Parcours 1 — Comprendre le flux complet sans entrer dans tous les détails

1. `docs/architecture/overview.md`
2. `docs/architecture/cqrs-patterns.md`
3. `src/Api/InfraFlowSculptor.Application/InfrastructureConfig/Commands/GenerateBicep/GenerateBicepCommandHandler.cs`
4. `src/Api/InfraFlowSculptor.Application/DependencyInjection.cs`
5. `src/Api/InfraFlowSculptor.BicepGeneration/BicepGenerationEngine.cs`
6. `src/Api/InfraFlowSculptor.BicepGeneration/Pipeline/BicepGenerationPipeline.cs`
7. `src/Api/InfraFlowSculptor.BicepGeneration/BicepAssembler.cs`
8. `src/Api/InfraFlowSculptor.BicepGeneration/MonoRepoBicepAssembler.cs`

À ce stade, tu comprends déjà le flux macro.

## Parcours 2 — Comprendre la mécanique interne de génération

1. `Pipeline/BicepGenerationContext.cs`
2. `Pipeline/Stages/IdentityAnalysisStage.cs`
3. `Pipeline/Stages/ModuleBuildStage.cs`
4. `Pipeline/Stages/ParentReferenceResolutionStage.cs`
5. `Pipeline/Stages/SpecEmissionStage.cs`
6. `Generators/KeyVaultTypeBicepGenerator.cs`
7. `Ir/BicepModuleSpec.cs`
8. `Ir/Builder/BicepModuleBuilder.cs`
9. `Ir/Emit/BicepEmitter.cs`
10. `Ir/Transformations/*`

À ce stade, tu comprends comment un module naît, se transforme et est émis.

## Parcours 3 — Comprendre les cas complexes

1. `Generators/StorageAccountTypeBicepGenerator.cs`
2. `Generators/WebAppTypeBicepGenerator.cs`
3. `Generators/FunctionAppTypeBicepGenerator.cs`
4. `Generators/ContainerAppTypeBicepGenerator.cs`
5. `Generators/ContainerAppEnvironmentTypeBicepGenerator.cs`
6. `Assemblers/MainBicepAssembler.cs`
7. `Assemblers/ParameterFileAssembler.cs`

Là, tu comprends les variantes, companions, paramètres complexes et références croisées.

## Parcours 4 — Comprendre la qualité et la stratégie de tests

1. `tests/InfraFlowSculptor.BicepGeneration.Tests/InfraFlowSculptor.BicepGeneration.Tests.csproj`
2. les tests de `Pipeline/Stages/`
3. les tests de `Ir/`
4. les tests des générateurs

But :

- voir comment le projet teste les briques fines ;
- apprendre le style de test attendu.

---

## Comment apprendre les bons patterns .NET à travers ce sous-système

La génération Bicep est un très bon terrain d’apprentissage parce qu’on y voit plusieurs patterns concrets.

## 1. DI + composition

À observer dans `DependencyInjection.cs`.

À retenir :

- enregistrer des implémentations multiples derrière une interface ;
- laisser l’orchestrateur consommer un `IEnumerable<T>` ;
- appliquer l’ordre explicitement dans le pipeline.

## 2. Façade

À observer dans `BicepGenerationEngine`.

À retenir :

- exposer une API simple ;
- pousser la complexité dans des composants internes spécialisés.

## 3. Strategy

À observer dans `Generators/`.

À retenir :

- un type ARM = une stratégie ;
- le moteur central ne doit pas contenir un `switch` géant sur tous les types.

## 4. Pipeline

À observer dans `Pipeline/`.

À retenir :

- séquencer des transformations dépendantes ;
- documenter les préconditions et postconditions ;
- rendre chaque étape testable.

## 5. Builder

À observer dans `Ir/Builder/`.

À retenir :

- rendre la construction déclarative ;
- éviter les constructeurs énormes ;
- guider le développeur vers une forme cohérente.

## 6. Immutable records + transformation functions

À observer dans `Ir/` et `Transformations/`.

À retenir :

- transformer des objets plutôt que muter des strings ;
- renvoyer un nouvel état plutôt qu’éparpiller les effets de bord.

## 7. Adapter

À observer dans `LegacyTextModuleAdapter`.

À retenir :

- très utile lors d’une migration progressive ;
- permet de conserver des contrats existants pendant un refactoring profond.

## 8. Helpers purs

À observer dans `Helpers/` et `TextManipulation/`.

À retenir :

- isoler les fonctions sans état ;
- éviter de mettre de la logique de formatage partout.

---

## Quand tu dois debugger un problème, où regarder d’abord

## Cas 1 — “Le fichier n’est pas généré”

Commence par :

1. `GenerateBicepCommandHandler`
2. `ModuleBuildStage`
3. le générateur concerné dans `Generators/`

Question clé :

- le type ARM de la ressource est-il bien enregistré en DI ?

## Cas 2 — “Le module existe mais le contenu est incorrect”

Commence par :

1. le générateur concerné ;
2. les transformations IR ;
3. `SpecEmissionStage` ;
4. `BicepEmitter`.

## Cas 3 — “Le `main.bicep` est faux mais les modules semblent bons”

Commence par :

1. `BicepAssembler`
2. `Assemblers/MainBicepAssembler.cs`
3. `ParentReferenceResolutionStage`

## Cas 4 — “Le mono-repo référence mal les chemins”

Commence par :

1. `MonoRepoBicepAssembler`
2. `BuildCommonModulePathMaps`
3. `RewriteMainBicepForMonoRepo`

## Cas 5 — “Un output manque ou est supprimé”

Commence par :

1. `OutputInjectionStage`
2. `BicepOutputPruner`
3. `BicepGenerationEngine`

---

## Si tu veux modifier le moteur, règles de prudence

1. **Ne remets pas de logique métier de domaine dans le projet BicepGeneration.**
2. **N’ajoute pas de magic strings ARM** : utilise `AzureResourceTypes`.
3. **Si la logique est transversale, préfère un stage ou une transformation IR.**
4. **Si la logique est spécifique à une ressource, préfère le générateur dédié.**
5. **Si tu changes le rendu texte, vérifie d’abord si cela relève de `BicepEmitter`.**
6. **Si tu touches l’assemblage final, vérifie les impacts single-config et mono-repo.**
7. **Si tu as besoin de compatibilité pendant une migration, préfère un adapter plutôt qu’un changement brutal.**

---

## Résumé ultra-court à garder en tête

- la couche **Application** prépare le `GenerationRequest` ;
- `BicepGenerationEngine` orchestre ;
- le **pipeline** applique les transformations dans l’ordre ;
- les **générateurs** savent construire un module par type Azure ;
- l’**IR** porte la structure typée ;
- `BicepEmitter` transforme l’IR en texte ;
- `BicepAssembler` et `MonoRepoBicepAssembler` construisent l’arborescence de fichiers finale ;
- la couche **Application** publie ensuite ces fichiers.

Si tu suis l’ordre de lecture proposé, tu comprendras non seulement la génération Bicep du projet, mais aussi plusieurs bons patterns .NET appliqués dans un vrai codebase.

---

## Fichiers à connaître par cœur

### Entrée applicative

- `src/Api/InfraFlowSculptor.Application/InfrastructureConfig/Commands/GenerateBicep/GenerateBicepCommandHandler.cs`
- `src/Api/InfraFlowSculptor.Application/Projects/Commands/GenerateProjectBicep/GenerateProjectBicepCommandHandler.cs`
- `src/Api/InfraFlowSculptor.Application/DependencyInjection.cs`

### Moteur

- `src/Api/InfraFlowSculptor.BicepGeneration/BicepGenerationEngine.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/Pipeline/BicepGenerationPipeline.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/Pipeline/BicepGenerationContext.cs`

### Stages

- `src/Api/InfraFlowSculptor.BicepGeneration/Pipeline/Stages/IdentityAnalysisStage.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/Pipeline/Stages/AppSettingsAnalysisStage.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/Pipeline/Stages/ModuleBuildStage.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/Pipeline/Stages/ParentReferenceResolutionStage.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/Pipeline/Stages/SpecEmissionStage.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/Pipeline/Stages/AssemblyStage.cs`

### Générateurs et IR

- `src/Api/InfraFlowSculptor.BicepGeneration/Generators/KeyVaultTypeBicepGenerator.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/Generators/WebAppTypeBicepGenerator.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/Generators/ContainerAppTypeBicepGenerator.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/Ir/BicepModuleSpec.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/Ir/Builder/BicepModuleBuilder.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/Ir/Emit/BicepEmitter.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/Ir/LegacyTextModuleAdapter.cs`

### Assemblage

- `src/Api/InfraFlowSculptor.BicepGeneration/BicepAssembler.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/MonoRepoBicepAssembler.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/Assemblers/MainBicepAssembler.cs`
- `src/Api/InfraFlowSculptor.BicepGeneration/Assemblers/ParameterFileAssembler.cs`

### Tests

- `tests/InfraFlowSculptor.BicepGeneration.Tests/InfraFlowSculptor.BicepGeneration.Tests.csproj`

---

## Pages connexes

- [Vue d’ensemble de l’architecture](overview.md)
- [CQRS et MediatR](cqrs-patterns.md)
- [Persistance EF Core](persistence.md)
- [Génération de pipelines](pipeline-generation.md)
- [Audit et options de refactoring Bicep](bicep-refactoring/00-AUDIT.md)
- [Pattern Pipeline](bicep-refactoring/01-pattern-pipeline.md)
- [Pattern Builder + IR](bicep-refactoring/02-pattern-builder-ir.md)
