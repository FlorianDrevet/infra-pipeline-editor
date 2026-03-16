# Azure DevOps — Backlog Initial — InfraFlowSculptor

> Généré automatiquement depuis la mémoire du projet (MEMORY.md) le 2026-03-16.  
> Ce fichier sert de référence pour initialiser le board Azure DevOps.  
> Statut : ✅ Complété | 🔄 En cours | 📋 À faire

---

## EPIC 1 — Architecture Core (Clean Architecture + CQRS + DDD) ✅

**Description :** Mettre en place les fondations architecturales du projet : structure en couches, CQRS avec MediatR, DDD avec agrégats/value objects, EF Core avec PostgreSQL.

---

### US 1.1 — Initialiser la structure du projet ✅

**En tant que** développeur,  
**Je veux** avoir une structure de projet Clean Architecture avec les couches Api/Application/Domain/Infrastructure/Contracts,  
**Afin de** pouvoir développer de façon organisée et maintenable.

**Acceptance Criteria:**
- [x] Projet Api (Minimal API) créé
- [x] Projet Application (CQRS) créé
- [x] Projet Domain (DDD) créé
- [x] Projet Infrastructure (EF Core) créé
- [x] Projet Contracts (DTOs) créé
- [x] Projet Shared (cross-cutting) créé

**Story Points:** 5

#### Tasks :
- [x] Créer la solution `.slnx` avec tous les projets
- [x] Configurer `Directory.Packages.props` (Central Package Management)
- [x] Configurer `global.json` (.NET 10, SDK 10.0.100)
- [x] Ajouter les dépendances MediatR, FluentValidation, Mapster, EF Core, ErrorOr

---

### TS 1.2 — Configurer les classes de base DDD (Shared.Domain) ✅

**Description :** Créer les classes de base réutilisables pour le DDD : `AggregateRoot<T>`, `Entity<T>`, `ValueObject`, `Id<T>`, `SingleValueObject<T>`, `EnumValueObject<T>`.

**Acceptance Criteria:**
- [x] `AggregateRoot<T>` avec domaine events
- [x] `Entity<T>` avec égalité structurelle
- [x] `Id<T>` supportant `new TId(Guid)` et `TId.Create(Guid)`
- [x] `SingleValueObject<T>` et `EnumValueObject<T>`

**Story Points:** 3

---

### TS 1.3 — Configurer EF Core + PostgreSQL + Migrations ✅

**Description :** Configurer le DbContext, les converters EF Core, et les migrations de base.

**Acceptance Criteria:**
- [x] `ProjectDbContext` configuré avec PostgreSQL
- [x] `IdValueConverter<TId, TKey>` implémenté
- [x] `SingleValueConverter<T, TPrimitive>` implémenté
- [x] `EnumValueConverter<TEnumValueObject, TEnum>` implémenté
- [x] 14 migrations créées et appliquées

**Story Points:** 5

---

### TS 1.4 — Configurer CQRS avec MediatR + FluentValidation ✅

**Description :** Mettre en place le pipeline CQRS : `ValidationBehavior`, scan d'assemblies, injection de dépendances.

**Acceptance Criteria:**
- [x] `ValidationBehavior<T>` pipeline behavior configuré
- [x] Scan automatique des handlers et validators
- [x] Pattern `ErrorOr<T>` pour les retours des handlers

**Story Points:** 3

---

### TS 1.5 — Configurer Mapster pour les mappings ✅

**Description :** Configurer Mapster avec des `IRegister` configs par feature, auto-découverte par assembly.

**Acceptance Criteria:**
- [x] `TypeAdapterConfig.GlobalSettings.Scan(Assembly)` configuré
- [x] Convention : value objects → primitifs via `.MapWith(src => src.Value)`

**Story Points:** 2

---

### TS 1.6 — Configurer .NET Aspire (orchestration locale) ✅

**Description :** Configurer Aspire pour orchestrer PostgreSQL, DbGate, l'API principale, et le BicepGenerator API.

**Acceptance Criteria:**
- [x] AppHost orchestre tous les services
- [x] PostgreSQL provisionné via Aspire
- [x] Azure Storage emulator configuré
- [x] `ServiceDefaults` partagés configurés

**Story Points:** 3

---

## EPIC 2 — Gestion de la Configuration Infra (InfrastructureConfig) ✅

**Description :** Aggregate root principal du domaine : représente une configuration d'infrastructure complète avec ses membres, environnements, et paramètres.

---

### US 2.1 — Créer et lister les InfrastructureConfigs ✅

**En tant que** utilisateur authentifié,  
**Je veux** créer une configuration d'infrastructure et lister les miennes,  
**Afin de** gérer mes projets d'infrastructure cloud.

**Acceptance Criteria:**
- [x] `POST /infra-config` crée une nouvelle config
- [x] `GET /infra-config` liste les configs où je suis membre
- [x] `GET /infra-config/{id}` retourne les détails

**Story Points:** 8

#### Tasks :
- [x] Aggregate `InfrastructureConfig` + value objects
- [x] `CreateInfrastructureConfigCommand` + handler + validator
- [x] `GetInfrastructureConfigQuery` + handler
- [x] `ListMyInfrastructureConfigsQuery` + handler
- [x] Endpoint Minimal API + Mapster config
- [x] EF Core configuration + migration

---

### US 2.2 — Gérer les membres d'une InfrastructureConfig ✅

**En tant que** Owner d'une InfrastructureConfig,  
**Je veux** ajouter/modifier/supprimer des membres avec des rôles (Owner/Contributor/Reader),  
**Afin de** contrôler l'accès à ma configuration.

**Acceptance Criteria:**
- [x] `POST /infra-config/{id}/members` ajoute un membre
- [x] `PUT /infra-config/{id}/members/{userId}` modifie le rôle
- [x] `DELETE /infra-config/{id}/members/{userId}` retire un membre
- [x] Seul l'Owner peut modifier les membres
- [x] L'Owner ne peut pas se retirer lui-même

**Story Points:** 8

---

### US 2.3 — Gérer les définitions d'environnements et paramètres ✅

**En tant que** Owner ou Contributor,  
**Je veux** définir des environnements (dev/staging/prod) et des paramètres configurables,  
**Afin de** générer des configurations spécifiques à chaque environnement.

**Acceptance Criteria:**
- [x] `EnvironmentDefinition` entité avec valeurs par environnement
- [x] `ParameterDefinition` value object (type, prefix, suffix, isSecret)
- [x] `EnvironmentParameterValue` par environnement
- [x] `ResourceParameterUsage` link ressource↔paramètre

**Story Points:** 13

---

## EPIC 3 — Gestion des Ressources Azure ✅

**Description :** CRUD complet pour toutes les ressources Azure supportées, avec contrôle d'accès (Owner/Contributor pour écriture, tout membre pour lecture).

---

### US 3.1 — Gérer les Resource Groups ✅

**En tant que** Owner ou Contributor,  
**Je veux** créer et gérer des Resource Groups dans ma configuration,  
**Afin de** organiser mes ressources Azure.

**Acceptance Criteria:**
- [x] `POST /resource-group` crée un RG
- [x] `GET /resource-group/{id}` retourne les détails
- [x] Contrôle d'accès : lecture (tout membre), écriture (Owner/Contributor)

**Story Points:** 5

---

### US 3.2 — Gérer les Key Vaults ✅

**En tant que** Owner ou Contributor,  
**Je veux** créer et gérer des Key Vaults dans mes Resource Groups,  
**Afin de** stocker les secrets de mon infrastructure.

**Acceptance Criteria:**
- [x] `POST /keyvault` crée un KV
- [x] `GET /keyvault/{id}` retourne les détails
- [x] `PUT /keyvault/{id}` met à jour
- [x] `DELETE /keyvault/{id}` supprime
- [x] Héritage TPT (`AzureResource`) configuré en EF Core
- [x] SKU (Premium/Standard) configuré

**Story Points:** 8

---

### US 3.3 — Gérer les Redis Caches ✅

**En tant que** Owner ou Contributor,  
**Je veux** créer et gérer des instances Redis Cache,  
**Afin de** configurer le cache de mon infrastructure.

**Acceptance Criteria:**
- [x] `POST /redis-cache` crée un Redis
- [x] `GET /redis-cache/{id}` retourne les détails
- [x] `PUT /redis-cache/{id}` met à jour
- [x] `DELETE /redis-cache/{id}` supprime
- [x] Value objects : `RedisCacheSku`, `TlsVersion`, `MaxMemoryPolicy`, `RedisCacheSettings`

**Story Points:** 8

---

### US 3.4 — Gérer les Storage Accounts ✅

**En tant que** Owner ou Contributor,  
**Je veux** créer et gérer des Storage Accounts avec leurs sous-ressources (BlobContainer, Queue, Table),  
**Afin de** configurer le stockage de mon infrastructure.

**Acceptance Criteria:**
- [x] CRUD Storage Account
- [x] CRUD sous-ressources : BlobContainer, StorageQueue, StorageTable
- [x] Pattern TPT + pattern agrégat avec sous-entités
- [x] Azure Blob Storage émulé via Aspire

**Story Points:** 13

---

### US 3.5 — Gérer les Role Assignments sur les ressources Azure ✅

**En tant que** Owner ou Contributor,  
**Je veux** assigner des rôles Azure sur mes ressources,  
**Afin de** configurer les permissions RBAC d'Azure dans mon Bicep.

**Acceptance Criteria:**
- [x] `GET /azure-resources/{id}/role-assignments`
- [x] `POST /azure-resources/{id}/role-assignments`
- [x] `DELETE /azure-resources/{id}/role-assignments`
- [x] `RoleAssignment` entité owned par `AzureResource`
- [x] `IAzureResourceRepository` cross-type

**Story Points:** 8

---

## EPIC 4 — Service de Génération Bicep ✅

**Description :** API secondaire qui lit la configuration depuis la DB et génère les fichiers Azure Bicep par environnement, puis les stocke dans Azure Blob Storage.

---

### US 4.1 — Déclencher la génération Bicep ✅

**En tant que** Owner d'une InfrastructureConfig,  
**Je veux** déclencher la génération des fichiers Bicep pour tous les environnements,  
**Afin d'** obtenir les fichiers IaC prêts à déployer.

**Acceptance Criteria:**
- [x] `POST /infra-config/generate-bicep` déclenche la génération
- [x] Appel via Refit client depuis l'API principale vers BicepGenerator
- [x] Fichiers générés et stockés dans Azure Blob Storage

**Story Points:** 8

---

### TS 4.2 — Implémenter le Strategy Pattern pour la génération Bicep ✅

**Description :** Chaque type de ressource Azure a son propre `IResourceTypeBicepGenerator`. Le service principal orchestre la génération en appelant le bon générateur.

**Acceptance Criteria:**
- [x] Interface `IResourceTypeBicepGenerator` définie
- [x] Implémentation pour KeyVault
- [x] Implémentation pour RedisCache
- [x] Implémentation pour StorageAccount
- [x] Enregistrement DI dans `BicepGenerator.Application`

**Story Points:** 13

---

## EPIC 5 — Authentification & Sécurité ✅

**Description :** Sécurisation de l'API avec Azure AD (Entra ID), gestion des rôles (Owner/Contributor/Reader), et contrôle d'accès aux ressources.

---

### US 5.1 — Configurer l'authentification Azure AD ✅

**En tant que** utilisateur,  
**Je veux** m'authentifier via Azure AD (Entra ID),  
**Afin d'** accéder à mes configurations sécurisées.

**Acceptance Criteria:**
- [x] JWT Bearer configuré avec Azure AD
- [x] Fallback policy : authenticated users only
- [x] Politique `IsAdmin` configurée
- [x] `ICurrentUser` / `CurrentUser` service implémenté

**Story Points:** 5

---

### US 5.2 — Implémenter le contrôle d'accès basé sur les rôles ✅

**En tant que** système,  
**Je veux** vérifier les rôles des membres avant chaque opération,  
**Afin de** respecter les niveaux d'accès (Owner/Contributor = écriture, Reader = lecture seule).

**Acceptance Criteria:**
- [x] `InfraConfigAccessHelper.VerifyReadAccessAsync` (tout membre)
- [x] `InfraConfigAccessHelper.VerifyWriteAccessAsync` (Owner/Contributor seulement)
- [x] Helpers spécialisés : `StorageAccountAccessHelper`, `KeyVaultAccessHelper`, `RedisCacheAccessHelper`
- [x] No information leak : retourne NotFound pour les non-membres

**Story Points:** 8

---

## EPIC 6 — Qualité & DevOps ✅

**Description :** Assurer la qualité du code, la CI/CD, et les outils de développement.

---

### TS 6.1 — Configurer les outils de qualité de code ✅

**Description :** SonarQube, linting, règles de qualité.

**Acceptance Criteria:**
- [x] Règles SonarQube documentées (S1192, S125, S3218)
- [x] Seuil duplication < 3%
- [x] Pattern `MemberCommandHelper` pour éviter la duplication

**Story Points:** 3

---

## EPIC 7 — Fonctionnalités à venir 📋

**Description :** Prochaines features planifiées.

---

### US 7.1 — Gérer les Azure Functions 📋

**En tant que** Owner ou Contributor,  
**Je veux** créer et gérer des Azure Functions dans mes Resource Groups,  
**Afin de** inclure des serverless dans mon infrastructure.

**Story Points:** 8

---

### US 7.2 — Générer les pipelines Azure DevOps ✅ (partiel)

**En tant que** Owner,  
**Je veux** générer automatiquement les pipelines Azure DevOps YAML depuis ma configuration,  
**Afin d'** automatiser le déploiement de mon infrastructure.

**Story Points:** 13

---

### US 7.3 — Interface utilisateur (Frontend) 📋

**En tant que** DevOps non-technique,  
**Je veux** une interface graphique pour gérer ma configuration infrastructure,  
**Afin de** ne pas avoir à appeler les APIs manuellement.

**Story Points:** 21

---

## Récapitulatif — Vélocité Estimée

| Epic | Points | Statut |
|------|--------|--------|
| Epic 1 — Architecture Core | 21 | ✅ Complété |
| Epic 2 — InfrastructureConfig | 29 | ✅ Complété |
| Epic 3 — Azure Resources | 42 | ✅ Complété |
| Epic 4 — BicepGenerator | 21 | ✅ Complété |
| Epic 5 — Auth & Sécurité | 13 | ✅ Complété |
| Epic 6 — Qualité & DevOps | 3 | ✅ Complété |
| **Total réalisé** | **129 pts** | ✅ |
| Epic 7 — Features à venir | 42+ | 📋 À faire |
