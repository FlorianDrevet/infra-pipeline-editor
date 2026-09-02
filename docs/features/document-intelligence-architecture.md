# Azure Document Intelligence — Architecture complète d'intégration

> **Document de conception architecturale** — Staff/Senior engineer level analysis
> Date: 2026-05-27
> Statut: Proposition — En attente de validation

---

## A. Compréhension de la feature

### Ce que la feature permettra

L'utilisateur pourra, depuis InfraFlowSculptor :
1. **Ajouter** une ressource Azure Document Intelligence à une configuration d'infrastructure
2. **Configurer** les propriétés de la ressource (SKU, réseau, identité, sous-domaine personnalisé)
3. **Gérer les paramètres par environnement** (SKU différent dev/prod, réseau public/privé selon l'env)
4. **Générer** le Bicep correspondant, intégré dans le pipeline de génération existant
5. **Référencer les outputs** de cette ressource dans les app settings d'autres ressources (endpoint URL)
6. **Gérer les rôles RBAC** associés (Cognitive Services User, Contributor)
7. **Configurer un Private Endpoint** pour un accès réseau restreint

### Comment elle sera utilisée

- L'utilisateur sélectionne "Document Intelligence" dans le picker de ressources
- Il renseigne nom, localisation, et les propriétés de base (sous-domaine)
- Il configure par environnement : SKU (F0 free pour dev, S0 standard pour prod), accès réseau
- À la génération, le module Bicep est produit et intégré dans `main.bicep`
- Les compute resources (WebApp, ContainerApp, FunctionApp) peuvent référencer l'endpoint comme app setting

### Comment elle s'intègre dans le produit

Document Intelligence est la **24ème ressource Azure** supportée. Elle suit exactement les mêmes patterns que les autres AI/Cognitive Services resources et enrichit le catalogue vers les services AI — un axe stratégique pour le produit (voir le positionnement Azure-first + AI).

### Ce qui changera concrètement

- Nouveau type de ressource disponible dans l'add-resource dialog
- Nouvelle section de configuration resource-edit avec propriétés et env settings
- Nouveau module Bicep généré (`DocumentIntelligence/documentIntelligence.module.bicep`)
- Nouveaux outputs référençables pour les app settings (endpoint)
- Nouveaux rôles RBAC assignables (Cognitive Services User/Contributor)
- Support du Private Endpoint via l'infrastructure existante

---

## B. Architecture cible

### 1. Analyse de la ressource Azure

#### Informations Azure fondamentales

| Propriété | Valeur |
|-----------|--------|
| ARM Type | `Microsoft.CognitiveServices/accounts` |
| Kind | `FormRecognizer` |
| API Version | `2026-03-01` (latest) |
| Abbreviation | `docint` |
| SKUs | `F0` (Free), `S0` (Standard) |
| Régions | ~30 régions, inclut France Central, West Europe |

#### Propriétés à exposer

**Au niveau ressource (global) :**
| Propriété | Type | Obligatoire | Valeur par défaut | Justification |
|-----------|------|-------------|-------------------|---------------|
| `Name` | string | Oui | — | Identifiant unique Azure (2-64 chars, `^[a-zA-Z0-9][a-zA-Z0-9_.-]*$`) |
| `Location` | Location | Oui | — | Région de déploiement |
| `CustomSubDomainName` | string? | Non | null | Sous-domaine pour authentification token-based; requis pour Private Endpoint et Entra ID auth |
| `DisableLocalAuth` | bool | Non | false | Désactiver les clés API (forcer Entra ID uniquement) |

**Au niveau environnement (per-env) :**
| Propriété | Type | Obligatoire | Valeur par défaut | Justification |
|-----------|------|-------------|-------------------|---------------|
| `Sku` | DocumentIntelligenceSku? | Non | null (inherit/F0) | F0 pour dev, S0 pour prod — le principal levier coût |
| `PublicNetworkAccess` | PublicNetworkAccessMode? | Non | null (Enabled) | Désactiver en prod avec PE, garder ouvert en dev |

#### Propriétés intentionnellement MASQUÉES à l'utilisateur

| Propriété Azure | Raison de masquage |
|-----------------|-------------------|
| `apiProperties` | Spécifique à d'autres Cognitive Services (QnA, Metrics Advisor) — non pertinent |
| `encryption` (CMK) | Trop avancé pour V1, ajout futur si demandé |
| `dynamicThrottlingEnabled` | Détail d'implémentation runtime, pas de valeur infra |
| `networkAcls` (détail) | Simplifié via `publicNetworkAccess` + Private Endpoint existant |
| `userOwnedStorage` | Scénario avancé, pas prioritaire |
| `allowedFqdnList` | Trop granulaire pour la cible produit |
| `networkInjections` | Scénario AI Foundry agents uniquement |
| `raiMonitorConfig` | Hors scope infra classique |

#### Outputs exposables (App Settings)

| Output | Description | Expression Bicep |
|--------|-------------|-----------------|
| `endpoint` | Document Intelligence endpoint URL | `docIntel.properties.endpoint` |
| `accountId` | Resource ID | `docIntel.id` |

> **Décision :** Ne pas exposer `listKeys()` directement comme output. Les clés API sont des secrets qui doivent transiter par Key Vault si utilisés. L'approche recommandée est Managed Identity (`disableLocalAuth: true`).

#### Rôles RBAC pertinents

| Rôle | GUID | Usage |
|------|------|-------|
| Cognitive Services User | `a97b65f3-24c7-4388-baec-2e87135dc908` | Lecture/utilisation du service (analyze documents) |
| Cognitive Services Contributor | `25fbc0a9-bd7c-42a3-aa1a-3b75d497ee68` | Gestion + utilisation (manage custom models) |

#### Dépendances et contraintes

- **Pas de dépendance parent** (contrairement à SqlDatabase → SqlServer ou ContainerApp → CAE)
- **Dépendance optionnelle** : Private Endpoint (via l'infra existante `PrivateEndpointConfig`)
- **Dépendance optionnelle** : UserAssignedIdentity (via le mécanisme existant `AssignedUserAssignedIdentityId`)
- **Contrainte de nommage** : 2-64 chars, pattern `^[a-zA-Z0-9][a-zA-Z0-9_.-]*$`
- **Contrainte de région** : disponible dans ~30 régions, mais pas partout — la validation frontend existante par localisation couvre déjà ce cas

#### Différences par environnement

| Aspect | Dev | Prod |
|--------|-----|------|
| SKU | F0 (Free, 500 pages/mois) | S0 (Pay-as-you-go) |
| Public Network Access | Enabled | Disabled (Private Endpoint) |
| Disable Local Auth | false (clés pour debug) | true (Entra ID only) |

---

### 2. Modèle de données — Domain Layer

```
src/Api/InfraFlowSculptor.Domain/DocumentIntelligenceAggregate/
├── DocumentIntelligence.cs                         (Aggregate root)
├── Entities/
│   └── DocumentIntelligenceEnvironmentSettings.cs  (Per-env entity)
└── ValueObjects/
    └── DocumentIntelligenceEnvironmentSettingsId.cs (Typed ID)
    └── DocumentIntelligenceSku.cs                  (EnumValueObject: F0, S0)
    └── PublicNetworkAccessMode.cs                  (EnumValueObject: Enabled, Disabled)
```

**Décisions de conception :**

1. **`DocumentIntelligenceSku` comme `EnumValueObject`** plutôt qu'un simple string — cohérent avec `RedisCacheSku`, `TlsVersion`, etc. Valeurs: `F0`, `S0`.

2. **`PublicNetworkAccessMode` comme `EnumValueObject`** — réutilisable à terme pour d'autres ressources (CosmosDb, SQL, etc.). Valeurs: `Enabled`, `Disabled`. Cependant, si cette enum est déjà définie pour une autre ressource ou pourrait l'être, la placer dans `Domain/Common/ValueObjects/` plutôt que dans l'agrégat.

3. **`CustomSubDomainName` au niveau ressource** (pas per-env) — c'est un identifiant DNS global unique, il ne change pas par environnement. Le name Azure change par env via le naming template, mais le sous-domaine custom est un choix architectural global.

4. **`DisableLocalAuth` au niveau ressource** (pas per-env) — c'est une politique de sécurité appliquée uniformément. En pratique, si on veut le faire varier par env, l'utilisateur crée 2 ressources séparées.

> **Challenge architectural :** Est-ce que `DisableLocalAuth` devrait être per-env ? En théorie oui (dev avec clés, prod sans). Mais dans la pratique Azure, le `kind: FormRecognizer` account est recréé par environnement via le naming template. Chaque env déploie sa propre instance. Donc c'est bien un **setting per-env**. → **Décision révisée : `DisableLocalAuth` migre vers `DocumentIntelligenceEnvironmentSettings`.**

**Aggregate root final :**
```csharp
public sealed class DocumentIntelligence : AzureResource
{
    // Resource-level properties
    public string? CustomSubDomainName { get; private set; }
    
    // Per-env settings
    public IReadOnlyCollection<DocumentIntelligenceEnvironmentSettings> EnvironmentSettings => ...;
    
    // Create, Update, SetEnvironmentSettings, SetAllEnvironmentSettings
}
```

**Environment Settings :**
```csharp
public sealed class DocumentIntelligenceEnvironmentSettings : Entity<DocumentIntelligenceEnvironmentSettingsId>
{
    public AzureResourceId DocumentIntelligenceId { get; private set; }
    public string EnvironmentName { get; private set; }
    public DocumentIntelligenceSku? Sku { get; private set; }           // F0, S0
    public PublicNetworkAccessMode? PublicNetworkAccess { get; private set; } // Enabled, Disabled
    public bool DisableLocalAuth { get; private set; }                   // Per-env security policy
}
```

---

### 3. Génération Bicep

#### Module cible

```bicep
// modules/DocumentIntelligence/documentIntelligence.module.bicep
import { DocumentIntelligenceSkuName, PublicNetworkAccessType } from 'types.bicep'

@description('Name of the Document Intelligence resource.')
param name string

@description('Location for the resource.')
param location string

@description('SKU name for Document Intelligence.')
param skuName DocumentIntelligenceSkuName = 'S0'

@description('Custom subdomain name for token-based authentication.')
param customSubDomainName string = ''

@description('Whether public network access is allowed.')
param publicNetworkAccess PublicNetworkAccessType = 'Enabled'

@description('Disable local (key-based) authentication.')
param disableLocalAuth bool = false

param tags object = {}

resource docIntel 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: name
  location: location
  kind: 'FormRecognizer'
  properties: {
    customSubDomainName: !empty(customSubDomainName) ? customSubDomainName : null
    publicNetworkAccess: publicNetworkAccess
    disableLocalAuth: disableLocalAuth
  }
  sku: {
    name: skuName
  }
  tags: tags
}

output endpoint string = docIntel.properties.endpoint
output id string = docIntel.id
```

#### Types associés

```bicep
// modules/DocumentIntelligence/types.bicep
@export()
type DocumentIntelligenceSkuName = 'F0' | 'S0'

@export()
type PublicNetworkAccessType = 'Enabled' | 'Disabled'
```

#### Intégration dans le moteur de génération

Le générateur implémente `IResourceTypeBicepSpecGenerator` et produit un `BicepModuleSpec` via le `BicepModuleBuilder` :

```csharp
public sealed class DocumentIntelligenceTypeBicepGenerator : IResourceTypeBicepSpecGenerator
{
    public string ResourceType => AzureResourceTypes.ArmTypes.DocumentIntelligenceType;
    public string ResourceTypeName => AzureResourceTypes.DocumentIntelligence;
    
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource) { ... }
}
```

**Stratégie de paramètres :**
- Tous les params per-env alimentent le `.bicepparam` par env
- `customSubDomainName` est un param global (même valeur partout, sauf si vide → non émis)
- Identity injection est gérée par le stage 400 existant (pas de code custom)
- Tags injection par le stage 700 existant
- Private Endpoint via le companion module existant (`PrivateEndpointTypeBicepGenerator`)

**API Version :** `2024-10-01` — stable et courante. Ne pas utiliser la preview `2026-03-01` pour la génération d'infra car le déploiement client doit rester stable.

#### RBAC

Ajout dans `AzureRoleDefinitionCatalog` :
```csharp
["DocumentIntelligence"] = [
    new("CognitiveServicesUser", "a97b65f3-24c7-4388-baec-2e87135dc908"),
    new("CognitiveServicesContributor", "25fbc0a9-bd7c-42a3-aa1a-3b75d497ee68"),
]
```

Ajout dans `RoleAssignmentModuleTemplates` pour la génération du module RBAC.

---

### 4. Architecture Backend (Application + Infrastructure)

#### CQRS Commands/Queries

| Opération | Fichier | Description |
|-----------|---------|-------------|
| Create | `CreateDocumentIntelligenceCommand` | Crée l'agrégat + env settings |
| Update | `UpdateDocumentIntelligenceCommand` | Met à jour nom/location/customSubDomain + env settings |
| Delete | `DeleteDocumentIntelligenceCommand` | Supprime via repository |
| Get | `GetDocumentIntelligenceQuery` | Charge avec Include(EnvironmentSettings) |

#### Validations

**Create/Update Validator :**
- `Name` : required, 2-64 chars, pattern `^[a-zA-Z0-9][a-zA-Z0-9_.-]*$`
- `Location` : required, must be valid Azure region
- `CustomSubDomainName` : optional, quand présent → 2-64 chars, lowercase alphanumeric + hyphens, unique globally (non validable côté back sans appel Azure)
- `EnvironmentSettings[].Sku` : si présent, doit être F0 ou S0
- `EnvironmentSettings[].PublicNetworkAccess` : si présent, doit être Enabled ou Disabled

#### Persistance EF Core

```csharp
// TPT inheritance
builder.HasBaseType<AzureResource>().ToTable("DocumentIntelligences");
builder.Property(x => x.CustomSubDomainName).HasMaxLength(64).IsRequired(false);

// Environment settings
builder.HasMany(x => x.EnvironmentSettings)
    .WithOne()
    .HasForeignKey(es => es.DocumentIntelligenceId)
    .OnDelete(DeleteBehavior.Cascade);
```

Index unique : `(DocumentIntelligenceId, EnvironmentName)` sur la table des env settings.

#### Impacts sur les fichiers existants

| Fichier | Modification |
|---------|-------------|
| `ProjectDbContext.cs` | +2 `DbSet<>` |
| `InfrastructureConfigReadRepository.cs` | +3 endroits (settings loading, MapResource, GetResourceTypeString) |
| `Application/DependencyInjection.cs` | +1 generator singleton |
| `Infrastructure/DependencyInjection.cs` | +1 repository scoped |
| `ResourceAbbreviationCatalog.cs` | +1 entry `["DocumentIntelligence"] = "docint"` |
| `AzureResourceTypes.cs` | +2 constants (friendly + ARM) |
| `ResourceOutputCatalog.cs` | +1 entry (endpoint) |
| `AzureRoleDefinitionCatalog.cs` | +1 entry (2 roles) |
| `RoleAssignmentModuleTemplates.cs` | +1 template metadata |
| `BicepArmTypeCatalog.cs` | +1 API version entry |
| `Program.cs` | +1 controller registration |

---

### 5. Architecture Frontend

#### Composants et fichiers

```
src/Front/src/app/
├── shared/
│   ├── interfaces/document-intelligence.interface.ts
│   └── services/document-intelligence.service.ts
├── features/
│   ├── config-detail/
│   │   ├── enums/resource-type.enum.ts          (MODIFIER)
│   │   └── add-resource-dialog/                  (MODIFIER)
│   └── resource-edit/
│       ├── resource-edit.component.ts            (MODIFIER)
│       └── resource-edit.component.html          (MODIFIER)
└── public/i18n/
    ├── fr.json                                   (MODIFIER)
    └── en.json                                   (MODIFIER)
```

#### UX Design

**Add Resource Dialog :**
- Catégorie : **"AI & Machine Learning"** (nouvelle catégorie, ou rattachement à une existante type "Data & Analytics" — à confirmer selon le regroupement actuel)
- Icône : `psychology` (Material icon, cohérent avec le thème AI)
- Step Environnement : OUI (car SKU et PublicNetworkAccess varient par env)

**Resource Edit — Onglet General :**
```
┌─────────────────────────────────────────────────────┐
│ Document Intelligence                                │
├─────────────────────────────────────────────────────┤
│ Name            [__________________________]         │
│ Location        [__________________________]         │
│ Custom subdomain [_________________________]  (opt.) │
│   ℹ️ Required for Private Endpoint & Entra ID auth  │
└─────────────────────────────────────────────────────┘
```

**Resource Edit — Onglet Environments :**
```
┌─────────────────────────────────────────────────────┐
│ Environment: dev                                     │
├─────────────────────────────────────────────────────┤
│ SKU             [F0 (Free) ▼]                       │
│ Public access   [Enabled ▼]                         │
│ Disable API keys [Toggle OFF]                       │
│                                                     │
│ Environment: prod                                    │
├─────────────────────────────────────────────────────┤
│ SKU             [S0 (Standard) ▼]                   │
│ Public access   [Disabled ▼]                        │
│ Disable API keys [Toggle ON]                        │
│   ⚠️ Requires Managed Identity for authentication   │
└─────────────────────────────────────────────────────┘
```

**Principes UX :**
- Utiliser `app-ds-select` pour SKU et PublicNetworkAccess (options fixes)
- Utiliser `app-ds-toggle` pour DisableLocalAuth avec avertissement contextuel
- Le champ `CustomSubDomainName` est guidé : info-bulle expliquant quand c'est nécessaire
- Masquer la complexité réseau derrière le simple toggle PublicNetworkAccess + Private Endpoint existant

---

### 6. Gestion des environnements

Le modèle d'héritage pour Document Intelligence suit le pattern existant :
- **Valeurs par défaut** : SKU=null (interprété comme F0 par le générateur), PublicNetworkAccess=null (Enabled), DisableLocalAuth=false
- **Surcharge par env** : chaque env peut override indépendamment
- **Génération** : le `.bicepparam` par env émet uniquement les valeurs non-null

**Pas de notion d'héritage global→env** pour cette ressource : chaque environnement est indépendant, car les Cognitive Services accounts sont des ressources ARM distinctes par env (contrairement à un Container App Environment qui pourrait être partagé).

---

### 7. Private Endpoint

Document Intelligence supporte le Private Endpoint via le `groupId: 'account'`. L'infrastructure existante (`PrivateEndpointConfig` sur `AzureResource`) gère déjà ce cas :
- L'utilisateur configure le PE via l'UI existante (onglet réseau dans resource-edit)
- Le companion module `PrivateEndpointTypeBicepGenerator` émet le module PE
- Le `PrivateDnsZone` associé est `privatelink.cognitiveservices.azure.com`

**Aucun développement spécifique n'est nécessaire** pour le Private Endpoint — le mécanisme existant couvre le besoin si `PrivateEndpointGroupIds` expose l'entrée pour `DocumentIntelligence → 'account'`.

---

## C. Plan d'implémentation détaillé

### Phase 1 — Domain + Application + Infrastructure (Backend core)

**Priorité : Haute — Fondation**

| # | Tâche | Fichiers | Agent |
|---|-------|----------|-------|
| 1.1 | Créer l'agrégat DocumentIntelligence | 5 fichiers domain | dotnet-dev |
| 1.2 | Créer les erreurs | 1 fichier | dotnet-dev |
| 1.3 | Ajouter aux catalogues (roles, abbreviations, outputs, ARM types) | 5 fichiers modifiés | dotnet-dev |
| 1.4 | Créer les commands/queries/handlers/validators | 12 fichiers application | dotnet-dev |
| 1.5 | Créer le repository + EF config + migration | 5 fichiers infra | dotnet-dev |
| 1.6 | Modifier InfrastructureConfigReadRepository | 3 points de modification | dotnet-dev |
| 1.7 | Créer les contracts (DTOs) | 4 fichiers | dotnet-dev |
| 1.8 | Créer le controller API + mapping | 3 fichiers | dotnet-dev |

**Livrable** : API fonctionnelle CRUD pour DocumentIntelligence

### Phase 2 — Bicep Generation

**Priorité : Haute — Valeur core**

| # | Tâche | Fichiers | Agent |
|---|-------|----------|-------|
| 2.1 | Créer DocumentIntelligenceTypeBicepGenerator | 1 fichier | dotnet-dev |
| 2.2 | Ajouter dans RoleAssignmentModuleTemplates | 1 modification | dotnet-dev |
| 2.3 | Ajouter dans BicepArmTypeCatalog | 1 modification | dotnet-dev |
| 2.4 | Enregistrer dans DI | 1 modification | dotnet-dev |
| 2.5 | Tests de génération (parity) | 2-3 fichiers tests | dotnet-dev |

**Livrable** : Génération Bicep fonctionnelle et testée

### Phase 3 — Frontend Integration

**Priorité : Moyenne — UX**

| # | Tâche | Fichiers | Agent |
|---|-------|----------|-------|
| 3.1 | Créer les interfaces TypeScript | 1 fichier | angular-front |
| 3.2 | Créer le service Angular | 1 fichier | angular-front |
| 3.3 | Ajouter dans ResourceTypeEnum + metadata | 5 points de modification | angular-front |
| 3.4 | Intégrer dans add-resource-dialog | 2 modifications | angular-front |
| 3.5 | Intégrer dans resource-edit (general + env tabs) | 2 modifications | angular-front |
| 3.6 | i18n FR + EN | 2 modifications | angular-front |

**Livrable** : UI complète, cohérente avec les autres ressources

### Phase 4 — Tests & Validation

**Priorité : Haute — Qualité**

| # | Tâche | Agent |
|---|-------|-------|
| 4.1 | Tests unitaires domain (aggregate, value objects) | dotnet-dev |
| 4.2 | Tests unitaires application (handlers, validators) | dotnet-dev |
| 4.3 | Tests de génération Bicep (snapshot/golden) | dotnet-dev |
| 4.4 | Validation `bicep build` sur artifacts générés | dotnet-dev |
| 4.5 | Frontend typecheck + build | angular-front |

### Stratégie de migration

- **Aucune migration de données nécessaire** : c'est un nouveau type de ressource
- Migration EF Core : `Add-Migration AddDocumentIntelligence` (additive, non-breaking)
- **Compatibilité ascendante** : les projets existants ne sont pas affectés (le type est simplement absent de leurs configs)
- **Import/Export** : le format JSON de sérialisation de projet doit inclure le nouveau type automatiquement via les patterns existants de `InfrastructureConfigReadRepository`

### Quick wins

1. Le mécanisme de Private Endpoint existant couvre déjà le besoin réseau
2. Le mécanisme d'identité (UAI) existant couvre déjà l'assignation d'identité
3. Le mécanisme de rôle RBAC existant couvre déjà l'assignation de rôles
4. Le mécanisme d'app settings existant permet déjà de référencer l'endpoint

### Dette technique identifiée

1. **`PublicNetworkAccessMode`** pourrait être mutualisé avec d'autres ressources (CosmosDb, SQL, Storage) qui ont le même concept. Actuellement chaque ressource gère ça indépendamment → dette de duplication conceptuelle, mais ne pas bloquer l'implémentation pour ça.
2. **Cognitive Services multi-kind** : le ARM type `Microsoft.CognitiveServices/accounts` est partagé avec OpenAI, Speech, Vision, etc. Si d'autres Cognitive Services sont ajoutés plus tard, il faudra un mécanisme de `kind` différenciation dans le moteur de génération. Mais pour une première itération, le générateur est spécifique à `FormRecognizer`.

---

## D. Edge cases et scénarios critiques

### Scénarios fonctionnels

| Scénario | Comportement attendu |
|----------|---------------------|
| Environnement sans settings définis | Génère avec les defaults (F0, public, local auth enabled) |
| Région incompatible | La validation frontend/backend vérifie la disponibilité. Si pas de validation de disponibilité par région dans le projet actuel, le déploiement Azure échouera — acceptable en V1, amélioration future. |
| SKU F0 existant + changement vers S0 | Azure supporte le changement de SKU sans recréation |
| SKU S0 → F0 | Azure le refuse si le quota est dépassé. Pas de validation côté InfraFlowSculptor — erreur runtime Azure acceptable |
| `customSubDomainName` vide | Pas de token-based auth, pas de Private Endpoint possible. Le générateur omet le paramètre. |
| `disableLocalAuth: true` sans identité assignée | Warning UX côté frontend (similaire aux diagnostics ACR existants). Le Bicep est valide mais inutilisable sans identité. |
| Ressource marquée `IsExisting` | Exclue de la génération Bicep, disponible comme `existing` resource reference pour les outputs |
| Suppression de la ressource | Cascade delete des env settings. Les app settings qui la référencent deviennent orphelins (comportement existant pour toutes les ressources). |
| Génération multi-config mono-repo | Le module est dédupliqué si les paramètres sont compatibles (mécanisme existant `NormalizePrimaryModuleFileNames`) |
| Private Endpoint sans `customSubDomainName` | Invalid côté Azure — **validation backend à ajouter** : si PE configuré, customSubDomainName requis |

### Scénarios techniques

| Scénario | Comportement |
|----------|-------------|
| Migration sur base existante | Migration additive (CREATE TABLE), aucun impact sur données existantes |
| Import d'un projet ancien (sans DocumentIntelligence) | Fonctionne — le type est simplement absent des configs importées |
| Rollback de migration | Possible via `Remove-Migration` standard EF Core |
| Génération incrémentale | Pas de concept d'incrémental dans le moteur actuel — full regeneration à chaque fois |
| Conflit de nommage avec une autre Cognitive Services | Le naming template gère ça via l'abbreviation `docint` unique |

### Risques identifiés

| Risque | Impact | Mitigation |
|--------|--------|-----------|
| ARM type partagé avec OpenAI/Vision | Si on ajoute Azure OpenAI plus tard, la logique de `kind` doit être propre | Documenter la décision; le `ResourceType` constant différencie déjà les types |
| Régions limitées | L'utilisateur peut configurer une région non supportée | Acceptable en V1 — Azure renvoie une erreur claire au déploiement |
| SKU F0 limité à 500 pages/mois | L'utilisateur peut ne pas comprendre les limitations | UX: ajouter une info-bulle sur le SKU selector |
| `customSubDomainName` globalement unique | Conflit possible si le nom est pris | Le déploiement Azure échouera — pas de pré-validation côté InfraFlowSculptor |

---

## E. Réflexion UX/UI détaillée

### Picker de ressource (Add Resource Dialog)

- **Catégorie suggérée** : `"AI & Cognitive Services"` (nouvelle catégorie si inexistante, sinon `"Data & AI"`)
- **Label** : "Document Intelligence"
- **Sous-label** : "Extract text and data from documents (formerly Form Recognizer)"
- **Icône** : `psychology` ou `document_scanner`

### Resource Edit — Onglet General

Champs affichés :
1. **Name** — `app-ds-text-field`, validation pattern
2. **Location** — composant location existant
3. **Custom Subdomain** — `app-ds-text-field` optionnel avec info contextuelle
   - Tooltip : "Required for Private Endpoint and Entra ID (managed identity) authentication. Must be globally unique."

### Resource Edit — Onglet Environments

Pour chaque env :
1. **SKU** — `app-ds-select` avec 2 options :
   - `F0 — Free (500 pages/month)`
   - `S0 — Standard (pay-per-use)`
2. **Public Network Access** — `app-ds-select` :
   - `Enabled`
   - `Disabled`
3. **Disable API Keys** — `app-ds-toggle`
   - Quand ON : afficher un warning contextuel jaune "Managed Identity required for authentication"

### Messages d'erreur et validations

| Validation | Message FR | Message EN |
|-----------|-----------|-----------|
| Name requis | "Le nom est requis" | "Name is required" |
| Name pattern | "Le nom doit commencer par une lettre ou un chiffre" | "Name must start with a letter or digit" |
| Subdomain pattern | "Le sous-domaine doit être en minuscules alphanumériques et tirets" | "Subdomain must be lowercase alphanumeric and hyphens" |

### Options avancées

Les fonctionnalités avancées sont accessibles via les mécanismes existants sans UI spécifique :
- **Private Endpoint** → onglet réseau existant sur resource-edit
- **Identity (UAI)** → onglet identité existant
- **Role Assignments** → onglet rôles existant
- **App Settings reference** → UI app settings existante sur les compute resources

### État d'erreur — Diagnostics

Ajouter un diagnostic conditionnel (comme les diagnostics ACR existants) :
- Si `disableLocalAuth: true` ET pas d'identité assignée → warning "L'authentification par clé est désactivée mais aucune identité managée n'est assignée"
- Si Private Endpoint configuré ET `customSubDomainName` vide → error "Un sous-domaine personnalisé est requis pour utiliser un Private Endpoint"

---

## F. Auto-critique

### Limites de cette approche

1. **Cognitive Services multi-kind** : Mon approche crée un générateur spécifique à `FormRecognizer`. Si le produit veut supporter OpenAI, Speech, Vision, chaque service sera un agrégat séparé avec un générateur séparé. C'est la bonne approche pour l'instant (chaque service a des propriétés et SKUs différents), mais ça ne scale pas infiniment sans un pattern d'abstraction "CognitiveServicesAccount" générique. Je recommande de ne PAS le faire maintenant — le besoin n'est pas avéré.

2. **Validation de disponibilité régionale** : Je ne propose pas de validation de région côté InfraFlowSculptor. Azure retournera une erreur claire au déploiement. Ajouter un catalogue de régions par type de ressource serait un travail significatif et une donnée volatile (Microsoft ajoute des régions régulièrement). C'est un compromis acceptable.

3. **Pas de validation d'unicité du `customSubDomainName`** : Ce sous-domaine est globalement unique dans Azure. Sans appel à l'API Azure, on ne peut pas le valider côté InfraFlowSculptor. On accepte l'erreur au déploiement.

4. **`PublicNetworkAccessMode` non mutualisé** : D'autres ressources ont ce concept (CosmosDb, SQL, etc.) mais chacune le gère indépendamment dans le code actuel. Introduire un ValueObject partagé serait du refactoring préventif — je préfère rester cohérent avec l'existant et factoriser quand le pattern se répète 3+ fois.

### Compromis assumés

| Compromis | Raison |
|-----------|--------|
| Pas de CMK (encryption) en V1 | Complexité disproportionnée, <5% des utilisateurs |
| Pas de multi-region routing | Scénario avancé, pas le use case principal |
| Pas de container deployment (disconnected) | Hors scope infra classique |
| Pas de custom model management | Le produit gère l'infra, pas le runtime AI |
| Pas de commitment tiers pricing | Aspect commercial, pas d'impact sur la génération |

### Ce que je ferais différemment en V2

1. **Abstract CognitiveServicesAccount** — si Azure OpenAI, Speech, Vision arrivent, extraire un pattern partagé pour `Microsoft.CognitiveServices/accounts` avec un `kind` discriminator
2. **Network Access Level** — unifier `PublicNetworkAccessMode` comme un ValueObject partagé dans `Domain/Common/ValueObjects/` utilisé par toutes les ressources qui ont ce concept
3. **Region availability validation** — intégrer un data provider qui maintient la matrice régions×types, mis à jour périodiquement depuis l'API Azure resource providers
4. **Managed Identity diagnostics** — un système de diagnostics cross-resource qui détecte les incohérences d'identité (pas de UAI assigné alors que l'auth locale est désactivée)

### Points forts de l'approche

- **Cohérence** : suit exactement les patterns établis pour les 23 autres ressources
- **Extensibilité** : le nouveau type s'intègre sans modification structurelle du moteur
- **Simplicité UX** : masque 90% de la complexité Azure (encryption, FQDN list, network injection, RAI monitor)
- **Zero breaking change** : migration additive, aucun impact sur les projets existants
- **Coverage immédiate** : PE, identity, RBAC, app settings — tous les mécanismes transversaux fonctionnent out-of-the-box

---

## Annexe — Fichiers complets à créer/modifier

### Créations (30 fichiers)

| Layer | Fichier |
|-------|---------|
| Domain | `DocumentIntelligenceAggregate/DocumentIntelligence.cs` |
| Domain | `DocumentIntelligenceAggregate/Entities/DocumentIntelligenceEnvironmentSettings.cs` |
| Domain | `DocumentIntelligenceAggregate/ValueObjects/DocumentIntelligenceEnvironmentSettingsId.cs` |
| Domain | `DocumentIntelligenceAggregate/ValueObjects/DocumentIntelligenceSku.cs` |
| Domain | `DocumentIntelligenceAggregate/ValueObjects/PublicNetworkAccessMode.cs` |
| Domain | `Common/Errors/Errors.DocumentIntelligence.cs` |
| Application | `DocumentIntelligences/Common/DocumentIntelligenceResult.cs` |
| Application | `DocumentIntelligences/Common/DocumentIntelligenceEnvironmentConfigData.cs` |
| Application | `DocumentIntelligences/Common/Interfaces/Persistence/IDocumentIntelligenceRepository.cs` |
| Application | `DocumentIntelligences/Commands/CreateDocumentIntelligence/CreateDocumentIntelligenceCommand.cs` |
| Application | `DocumentIntelligences/Commands/CreateDocumentIntelligence/CreateDocumentIntelligenceCommandHandler.cs` |
| Application | `DocumentIntelligences/Commands/CreateDocumentIntelligence/CreateDocumentIntelligenceCommandValidator.cs` |
| Application | `DocumentIntelligences/Commands/UpdateDocumentIntelligence/UpdateDocumentIntelligenceCommand.cs` |
| Application | `DocumentIntelligences/Commands/UpdateDocumentIntelligence/UpdateDocumentIntelligenceCommandHandler.cs` |
| Application | `DocumentIntelligences/Commands/UpdateDocumentIntelligence/UpdateDocumentIntelligenceCommandValidator.cs` |
| Application | `DocumentIntelligences/Commands/DeleteDocumentIntelligence/DeleteDocumentIntelligenceCommand.cs` |
| Application | `DocumentIntelligences/Commands/DeleteDocumentIntelligence/DeleteDocumentIntelligenceCommandHandler.cs` |
| Application | `DocumentIntelligences/Queries/GetDocumentIntelligence/GetDocumentIntelligenceQuery.cs` |
| Application | `DocumentIntelligences/Queries/GetDocumentIntelligence/GetDocumentIntelligenceQueryHandler.cs` |
| Infrastructure | `Persistence/Configurations/DocumentIntelligenceConfiguration.cs` |
| Infrastructure | `Persistence/Configurations/DocumentIntelligenceEnvironmentSettingsConfiguration.cs` |
| Infrastructure | `Persistence/Repositories/DocumentIntelligenceRepository.cs` |
| Contracts | `DocumentIntelligences/Requests/DocumentIntelligenceRequestBase.cs` |
| Contracts | `DocumentIntelligences/Requests/CreateDocumentIntelligenceRequest.cs` |
| Contracts | `DocumentIntelligences/Requests/UpdateDocumentIntelligenceRequest.cs` |
| Contracts | `DocumentIntelligences/Responses/DocumentIntelligenceResponse.cs` |
| API | `Controllers/DocumentIntelligenceController.cs` |
| API | `Common/Mapping/DocumentIntelligenceMappingConfig.cs` |
| BicepGeneration | `Generators/DocumentIntelligenceTypeBicepGenerator.cs` |
| Frontend | `shared/interfaces/document-intelligence.interface.ts` |
| Frontend | `shared/services/document-intelligence.service.ts` |

### Modifications (15+ fichiers)

| Fichier | Nature |
|---------|--------|
| `AzureResourceTypes.cs` | +2 constants |
| `ResourceOutputCatalog.cs` | +1 resource entry |
| `AzureRoleDefinitionCatalog.cs` | +1 resource entry |
| `ResourceAbbreviationCatalog.cs` | +1 entry |
| `Application/DependencyInjection.cs` | +1 singleton |
| `Infrastructure/DependencyInjection.cs` | +1 scoped |
| `ProjectDbContext.cs` | +2 DbSet |
| `InfrastructureConfigReadRepository.cs` | +3 switch cases |
| `RoleAssignmentModuleTemplates.cs` | +1 metadata |
| `BicepArmTypeCatalog.cs` | +1 API version |
| `Program.cs` | +1 controller |
| `resource-type.enum.ts` | +5 points (enum, icons, abbreviations, categories, env-settings metadata) |
| `add-resource-dialog` | Form integration |
| `resource-edit.component.ts` | +service injection + load/save/delete |
| `resource-edit.component.html` | +General + Environment tab cases |
| `fr.json` | +i18n keys |
| `en.json` | +i18n keys |

---

## Conclusion

Cette ressource est un excellent candidat pour une intégration "first-class" :
- **Simple** (pas de parent-child, pas de sub-resources, SKU linéaire)
- **Bien documentée** (ARM template officiel stable)
- **Bon ROI** (ouvre le catalogue vers les services AI)
- **Zero refactoring architectural** (l'infra existante couvre tous les besoins transversaux)

L'estimation réaliste est de **3 phases séquentielles** : Backend → Bicep → Frontend, avec validation TDD à chaque étape.
