## 🎯 But principal

<!-- Décrivez en une phrase claire l'objectif de cette PR. -->
<!-- Ex : Ajout de la feature StorageAccount avec CRUD complet -->


---

## 📋 Type de changement

<!-- Cochez les cases correspondantes (une ou plusieurs) -->

- [ ] `feat` — Nouvelle fonctionnalité
- [ ] `fix` — Correction de bug
- [ ] `refactor` — Refactoring sans changement fonctionnel
- [ ] `perf` — Amélioration des performances
- [ ] `docs` — Documentation uniquement
- [ ] `test` — Ajout ou modification de tests
- [ ] `chore` — Maintenance, dépendances, CI
- [ ] `ci` — Changements liés aux pipelines CI/CD
- [ ] `style` — Formatage, lint (sans impact fonctionnel)
- [ ] `revert` — Annulation d'un commit précédent

---

## 🗂️ Changements par couche

### Domaine (`InfraFlowSculptor.Domain`)
<!-- Nouveaux agrégats, entités, value objects, erreurs ajoutés ou modifiés -->
<!-- Ex : - Ajout de `StorageAccountAggregate` avec `StorageAccountId`, `StorageAccountName`, `StorageSku` -->
<!-- Supprimer la section si non concernée -->

- 

### Application (`InfraFlowSculptor.Application`)
<!-- Commandes, queries, handlers, validators, helpers ajoutés ou modifiés -->
<!-- Ex : - `CreateStorageAccountCommand` + handler + validator -->
<!-- Supprimer la section si non concernée -->

- 

### Infrastructure (`InfraFlowSculptor.Infrastructure`)
<!-- Configurations EF Core, repositories, migrations, services Azure ajoutés ou modifiés -->
<!-- Ex : - `StorageAccountConfiguration` EF Core + `StorageAccountRepository` + migration `AddStorageAccountTable` -->
<!-- Supprimer la section si non concernée -->

- 

### Contrats (`InfraFlowSculptor.Contracts`)
<!-- Requests, responses, validation attributes ajoutés ou modifiés -->
<!-- Ex : - `CreateStorageAccountRequest`, `StorageAccountResponse` -->
<!-- Supprimer la section si non concernée -->

- 

### API & Mappings (`InfraFlowSculptor.Api`)
<!-- Endpoints Minimal API, configs Mapster, DI ajoutés ou modifiés -->
<!-- Ex : - Endpoints GET/POST/PUT/DELETE sous `/storage-account` -->
<!-- Supprimer la section si non concernée -->

- 

### Frontend (`src/Front`)
<!-- Components, routes, services Axios, facades, guards, environnements, theming -->
<!-- Ex : - Alignement des interfaces frontend avec les contrats backend -->
<!-- Supprimer la section si non concernée -->

- 

### BicepGenerators (`BicepGenerator.*`)
<!-- Générateurs, stratégies Bicep, blob storage ajoutés ou modifiés -->
<!-- Supprimer la section si non concernée -->

- 

### Partagé (`Shared.*`)
<!-- Base classes, helpers, middlewares, converters partagés ajoutés ou modifiés -->
<!-- Supprimer la section si non concernée -->

- 

### Aspire / Configuration / CI
<!-- AppHost, appsettings, Dockerfile, workflows GitHub Actions ajoutés ou modifiés -->
<!-- Supprimer la section si non concernée -->

- 

---

## 🗄️ Base de données

- [ ] Cette PR inclut une **migration EF Core** (`dotnet ef migrations add <Nom>`)
- [ ] Aucune migration nécessaire

<!-- Si migration : précisez le nom -->
Nom de la migration : 

---

## ✅ Checklist avant merge

- [ ] Build réussi : `dotnet build .\InfraFlowSculptor.slnx`
- [ ] Aucun avertissement de compilation introduit
- [ ] Conventions DDD respectées (agrégat / entité / value object)
- [ ] Handlers retournent `ErrorOr<T>` (pas d'exception pour les erreurs métier)
- [ ] Validators FluentValidation créés pour chaque commande
- [ ] Repositories utilisés depuis les handlers (pas d'appel EF Core direct)
- [ ] Endpoints enregistrés dans `Program.cs`
- [ ] Mapping Mapster configuré via `IRegister`
- [ ] `MEMORY.md` mis à jour avec les nouveautés
- [ ] Duplication de code < 3% (seuil qualité SonarQube)

---

## 🔗 Issues / tickets liés

<!-- Ex : Closes #42 -->


