# Plan d'implémentation — Privatisation réseau (VNet, Subnets, Private Endpoints, CDN)

> **Statut :** Plan exhaustif — à implémenter ultérieurement via PRs dédiées.
> **Auteur :** Architecture review — 2026-05-13
> **Complexité :** Très élevée — ~120+ fichiers impactés, 5+ PRs recommandées.

---

## Table des matières

1. [Vue d'ensemble](#1-vue-densemble)
2. [Nouvelles ressources à créer](#2-nouvelles-ressources-à-créer)
3. [Modification des ressources existantes](#3-modification-des-ressources-existantes)
4. [Architecture Domain Model](#4-architecture-domain-model)
5. [Application Layer (CQRS)](#5-application-layer-cqrs)
6. [Infrastructure / Persistence](#6-infrastructure--persistence)
7. [Contracts Layer](#7-contracts-layer)
8. [API Layer](#8-api-layer)
9. [Bicep Generation — Modules génériques](#9-bicep-generation--modules-génériques)
10. [Frontend — Configuration UI](#10-frontend--configuration-ui)
11. [Validation de SKU pour la privatisation](#11-validation-de-sku-pour-la-privatisation)
12. [Séquencement des PRs](#12-séquencement-des-prs)
13. [Risques et dépendances](#13-risques-et-dépendances)

---

## 1. Vue d'ensemble

### Objectif

Permettre la configuration de la privatisation réseau (Private Endpoints, VNets, Subnets, Private DNS Zones, CDN/Front Door) pour l'ensemble des ressources Azure supportées par InfraFlowSculptor. Générer le Bicep correspondant via des **modules génériques réutilisables**.

### Périmètre fonctionnel

| Capacité | Description |
|----------|-------------|
| **VNet** | Nouvelle ressource configurable avec address space per-env |
| **Subnet** | Sous-ressource du VNet, avec delegation, NSG, service endpoints |
| **Private Endpoint (PE)** | Configuration attachée à chaque ressource supportant la privatisation |
| **Private DNS Zone** | Gestion automatique des zones DNS privées liées aux PE |
| **CDN / Front Door** | Nouvelle ressource pour exposition publique sécurisée des services privatisés |
| **NSG** | Network Security Group associé aux subnets |
| **SKU Validation** | Blocage de la privatisation si le SKU de la ressource cible ne la supporte pas |

### Ressources existantes impactées (ajout de config PE)

Toutes les ressources qui supportent Private Endpoints dans Azure :
- KeyVault
- StorageAccount
- AppConfiguration
- CosmosDb
- SqlServer
- RedisCache
- ServiceBusNamespace
- EventHubNamespace
- ContainerRegistry
- ApplicationInsights (via Private Link Scope)
- LogAnalyticsWorkspace (via Private Link Scope)
- WebApp / FunctionApp (via VNet Integration + PE)

---

## 2. Nouvelles ressources à créer

### 2.1. VirtualNetwork (VNet)

| Propriété | Type | Scope |
|-----------|------|-------|
| `Name` | Name | Resource-level |
| `Location` | Location | Resource-level |
| `AddressSpaces` | IReadOnlyList\<string\> | Per-environment |
| `DnsServers` | IReadOnlyList\<string\>? | Per-environment |
| `EnableDdosProtection` | bool | Resource-level |

**ARM type :** `Microsoft.Network/virtualNetworks`
**Abréviation :** `vnet`

### 2.2. Subnet (enfant de VNet)

| Propriété | Type | Scope |
|-----------|------|-------|
| `Name` | Name | Entité owned par VNet |
| `AddressPrefix` | string | Per-environment |
| `Delegation` | SubnetDelegation? | Resource-level (enum: `Microsoft.Web/serverFarms`, `Microsoft.App/environments`, etc.) |
| `ServiceEndpoints` | IReadOnlyList\<string\>? | Resource-level |
| `PrivateEndpointNetworkPolicies` | PrivateEndpointNetworkPolicy | Resource-level (enum: `Enabled`, `Disabled`, `NetworkSecurityGroupEnabled`, `RouteTableEnabled`) |
| `NsgId` | AzureResourceId? | FK optionnel vers NSG |

**Structure domain :** Entity owned par VirtualNetwork (comme `BlobContainer` dans `StorageAccount`)

### 2.3. NetworkSecurityGroup (NSG)

| Propriété | Type | Scope |
|-----------|------|-------|
| `Name` | Name | Resource-level |
| `Location` | Location | Resource-level |
| `SecurityRules` | IReadOnlyList\<NsgRule\> | Resource-level owned collection |

**NsgRule (owned entity) :**
- `Name`, `Priority` (int), `Direction` (Inbound/Outbound), `Access` (Allow/Deny)
- `Protocol` (Tcp/Udp/Icmp/Any), `SourceAddressPrefix`, `DestinationAddressPrefix`
- `SourcePortRange`, `DestinationPortRange`

**ARM type :** `Microsoft.Network/networkSecurityGroups`
**Abréviation :** `nsg`

### 2.4. PrivateDnsZone

| Propriété | Type | Scope |
|-----------|------|-------|
| `Name` | Name (auto-derived from resource type: `privatelink.vaultcore.azure.net`, etc.) |
| `VirtualNetworkLinks` | IReadOnlyList\<VNetLinkConfig\> | Links vers VNets pour resolution |

**ARM type :** `Microsoft.Network/privateDnsZones`
**Abréviation :** `pdnsz`

> **Décision architecturale :** Les Private DNS Zones sont générées **automatiquement** par le moteur Bicep quand un PE est configuré. Pas besoin d'une ressource configurable manuellement dans la plupart des cas. On crée toutefois l'entité pour permettre une configuration avancée (custom DNS zone names, multiple VNet links).

### 2.5. FrontDoor (CDN/WAF)

| Propriété | Type | Scope |
|-----------|------|-------|
| `Name` | Name | Resource-level |
| `Location` | `global` (fixe) | Resource-level |
| `SkuName` | FrontDoorSku (Standard_AzureFrontDoor / Premium_AzureFrontDoor) | Per-environment |
| `Origins` | IReadOnlyList\<FrontDoorOrigin\> | Config des origines (FK vers ressource cible) |
| `WafPolicyEnabled` | bool | Resource-level |
| `CustomDomains` | réutilise `CustomDomain` existant | Hérité de AzureResource |

**FrontDoorOrigin (owned entity) :**
- `TargetResourceId` (AzureResourceId) — ressource exposée (WebApp, ContainerApp, StorageAccount)
- `HostName` (string?) — override si nécessaire
- `PrivateLinkEnabled` (bool) — utilise PE pour la connexion à l'origin
- `Weight` (int), `Priority` (int)

**ARM type :** `Microsoft.Cdn/profiles` (AFD = Azure Front Door via CDN profile)
**Abréviation :** `afd`

---

## 3. Modification des ressources existantes

### 3.1. Nouveau concept : PrivateEndpointConfiguration

Ajout d'une **entité enfant** sur `AzureResource` (base class) pour la configuration PE :

```csharp
public sealed class PrivateEndpointConfig : Entity<PrivateEndpointConfigId>
{
    public AzureResourceId ResourceId { get; private set; }
    public AzureResourceId SubnetId { get; private set; }          // FK vers Subnet (via VNet)
    public string GroupId { get; private set; }                     // PE sub-resource group (e.g., "vault", "blob", "sqlServer")
    public bool AutoApproval { get; private set; }
    public AzureResourceId? PrivateDnsZoneId { get; private set; } // FK optionnel vers PrivateDnsZone
    public string? CustomNetworkInterfaceName { get; private set; }
}
```

### 3.2. Modification de AzureResource (base class)

```csharp
// Ajout sur AzureResource :
private readonly List<PrivateEndpointConfig> _privateEndpointConfigs = [];
public IReadOnlyCollection<PrivateEndpointConfig> PrivateEndpointConfigs => _privateEndpointConfigs.AsReadOnly();

// Méthodes domain :
public void AddPrivateEndpoint(AzureResourceId subnetId, string groupId, bool autoApproval, AzureResourceId? dnsZoneId, string? customNicName);
public void RemovePrivateEndpoint(PrivateEndpointConfigId configId);
public void UpdatePrivateEndpoint(PrivateEndpointConfigId configId, ...);
```

### 3.3. PublicNetworkAccess (per-environment)

Pour chaque ressource supportant la privatisation, ajouter dans les `EnvironmentSettings` :

```csharp
public PublicNetworkAccess? PublicNetworkAccess { get; private set; }
// Enum: Enabled, Disabled, SecuredByPerimeter
```

**Ressources impactées :** KeyVault, StorageAccount, AppConfiguration, CosmosDb, SqlServer, RedisCache, ServiceBusNamespace, EventHubNamespace, ContainerRegistry.

> **Note :** `AppConfiguration` a déjà un champ `PublicNetworkAccess` dans ses `EnvironmentSettings`. Le pattern existant sera généralisé.

### 3.4. VNet Integration pour Compute (WebApp/FunctionApp/ContainerApp)

En plus du PE (inbound), les ressources compute nécessitent une **VNet Integration** (outbound) :

```csharp
// Sur WebApp, FunctionApp :
public AzureResourceId? VnetIntegrationSubnetId { get; private set; }

// Sur ContainerAppEnvironment :
public AzureResourceId? InfrastructureSubnetId { get; private set; }
public AzureResourceId? RuntimeSubnetId { get; private set; }
```

---

## 4. Architecture Domain Model

### 4.1. Nouveaux fichiers Domain

```
src/Api/InfraFlowSculptor.Domain/
├── VirtualNetworkAggregate/
│   ├── VirtualNetwork.cs                          # Aggregate root
│   ├── Entities/
│   │   ├── Subnet.cs                             # Owned entity
│   │   ├── SubnetId.cs                           # Strongly-typed ID
│   │   └── VirtualNetworkEnvironmentSettings.cs  # Per-env (address spaces)
│   └── ValueObjects/
│       ├── VirtualNetworkEnvironmentSettingsId.cs
│       ├── SubnetDelegation.cs                   # EnumValueObject
│       └── PrivateEndpointNetworkPolicy.cs       # EnumValueObject
├── NetworkSecurityGroupAggregate/
│   ├── NetworkSecurityGroup.cs
│   ├── Entities/
│   │   ├── NsgRule.cs
│   │   ├── NsgRuleId.cs
│   │   └── NetworkSecurityGroupEnvironmentSettings.cs (vide pour l'instant)
│   └── ValueObjects/
│       ├── NetworkSecurityGroupEnvironmentSettingsId.cs
│       ├── NsgDirection.cs                       # EnumValueObject
│       ├── NsgAccess.cs                          # EnumValueObject
│       └── NsgProtocol.cs                        # EnumValueObject
├── PrivateDnsZoneAggregate/
│   ├── PrivateDnsZone.cs
│   ├── Entities/
│   │   ├── VirtualNetworkLink.cs
│   │   └── VirtualNetworkLinkId.cs
│   └── ValueObjects/
│       └── PrivateDnsZoneId.cs  (ID déjà dans Common)
├── FrontDoorAggregate/
│   ├── FrontDoor.cs
│   ├── Entities/
│   │   ├── FrontDoorOrigin.cs
│   │   ├── FrontDoorOriginId.cs
│   │   └── FrontDoorEnvironmentSettings.cs
│   └── ValueObjects/
│       ├── FrontDoorEnvironmentSettingsId.cs
│       └── FrontDoorSku.cs                       # EnumValueObject
├── Common/
│   ├── BaseModels/
│   │   └── Entites/
│   │       └── PrivateEndpointConfig.cs          # Shared entity (owned by AzureResource)
│   ├── ValueObjects/
│   │   ├── PrivateEndpointConfigId.cs
│   │   └── PublicNetworkAccess.cs                # EnumValueObject
│   └── Errors/
│       ├── Errors.VirtualNetwork.cs
│       ├── Errors.NetworkSecurityGroup.cs
│       ├── Errors.PrivateDnsZone.cs
│       └── Errors.FrontDoor.cs
```

### 4.2. PE GroupIds par type de ressource (catalogue domain)

```csharp
public static class PrivateEndpointGroupIdCatalog
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> GroupIdsByResourceType =
        new Dictionary<string, IReadOnlyList<string>>
        {
            [AzureResourceTypes.KeyVault] = ["vault"],
            [AzureResourceTypes.StorageAccount] = ["blob", "file", "queue", "table", "dfs", "web"],
            [AzureResourceTypes.AppConfiguration] = ["configurationStores"],
            [AzureResourceTypes.CosmosDb] = ["Sql", "MongoDB", "Cassandra", "Gremlin", "Table"],
            [AzureResourceTypes.SqlServer] = ["sqlServer"],
            [AzureResourceTypes.RedisCache] = ["redisCache"],
            [AzureResourceTypes.ServiceBusNamespace] = ["namespace"],
            [AzureResourceTypes.EventHubNamespace] = ["namespace"],
            [AzureResourceTypes.ContainerRegistry] = ["registry"],
            [AzureResourceTypes.WebApp] = ["sites"],
            [AzureResourceTypes.FunctionApp] = ["sites"],
            [AzureResourceTypes.ApplicationInsights] = ["azuremonitor"],
            [AzureResourceTypes.LogAnalyticsWorkspace] = ["azuremonitor"],
        }.ToFrozenDictionary();
}
```

### 4.3. Private DNS Zone names par GroupId

```csharp
public static class PrivateDnsZoneNameCatalog
{
    // Used by Bicep generator to auto-derive DNS zone names
    public static readonly IReadOnlyDictionary<string, string> ZoneNameByGroupId =
        new Dictionary<string, string>
        {
            ["vault"] = "privatelink.vaultcore.azure.net",
            ["blob"] = "privatelink.blob.core.windows.net",
            ["file"] = "privatelink.file.core.windows.net",
            ["queue"] = "privatelink.queue.core.windows.net",
            ["table"] = "privatelink.table.core.windows.net",
            ["dfs"] = "privatelink.dfs.core.windows.net",
            ["web"] = "privatelink.web.core.windows.net",
            ["configurationStores"] = "privatelink.azconfig.io",
            ["Sql"] = "privatelink.documents.azure.com",
            ["sqlServer"] = "privatelink.database.windows.net",
            ["redisCache"] = "privatelink.redis.cache.windows.net",
            ["namespace"] = "privatelink.servicebus.windows.net",  // SB + EH share
            ["registry"] = "privatelink.azurecr.io",
            ["sites"] = "privatelink.azurewebsites.net",
            ["azuremonitor"] = "privatelink.monitor.azure.com",
        }.ToFrozenDictionary();
}
```

---

## 5. Application Layer (CQRS)

### 5.1. Nouvelles features CQRS (suivant `new-azure-resource` skill × 4 ressources)

| Ressource | Commands | Queries | Handlers | Validators |
|-----------|----------|---------|----------|------------|
| VirtualNetwork | Create, Update, Delete, AddSubnet, UpdateSubnet, RemoveSubnet | Get, List | 7 | 4 |
| NetworkSecurityGroup | Create, Update, Delete, AddRule, UpdateRule, RemoveRule | Get | 7 | 4 |
| PrivateDnsZone | Create, Update, Delete, AddVNetLink, RemoveVNetLink | Get | 5 | 3 |
| FrontDoor | Create, Update, Delete, AddOrigin, UpdateOrigin, RemoveOrigin | Get | 7 | 4 |

### 5.2. Commandes PE sur les ressources existantes

Nouveau dossier cross-cutting dans Application :

```
src/Api/InfraFlowSculptor.Application/PrivateEndpoints/
├── Commands/
│   ├── AddPrivateEndpoint/
│   │   ├── AddPrivateEndpointCommand.cs          # { ResourceId, SubnetId, GroupId, ... }
│   │   ├── AddPrivateEndpointCommandHandler.cs    # Charge via IAzureResourceRepository polymorphique
│   │   └── AddPrivateEndpointCommandValidator.cs  # Valide GroupId vs ResourceType
│   ├── UpdatePrivateEndpoint/
│   │   └── ...
│   └── RemovePrivateEndpoint/
│       └── ...
└── Queries/
    ├── GetPrivateEndpointConfigs/
    │   └── ...                                   # Liste les PE configs d'une ressource
    └── GetAvailableGroupIds/
        └── ...                                   # Retourne GroupIds valides pour un ResourceType
```

### 5.3. Commandes VNet Integration sur Compute

```
src/Api/InfraFlowSculptor.Application/WebApps/Commands/SetVnetIntegration/
src/Api/InfraFlowSculptor.Application/FunctionApps/Commands/SetVnetIntegration/
src/Api/InfraFlowSculptor.Application/ContainerAppEnvironments/Commands/SetInfrastructureSubnet/
```

### 5.4. Interface Repositories

```csharp
public interface IVirtualNetworkRepository : IRepository<VirtualNetwork, VirtualNetworkId> { }
public interface INetworkSecurityGroupRepository : IRepository<NetworkSecurityGroup, NetworkSecurityGroupId> { }
public interface IPrivateDnsZoneRepository : IRepository<PrivateDnsZone, PrivateDnsZoneId> { }
public interface IFrontDoorRepository : IRepository<FrontDoor, FrontDoorId> { }
```

---

## 6. Infrastructure / Persistence

### 6.1. Nouvelles configurations EF Core

| Fichier | Notes |
|---------|-------|
| `VirtualNetworkConfiguration.cs` | TPT, owns `Subnets` et `EnvironmentSettings` |
| `SubnetConfiguration.cs` | Owned entity with unique `(VirtualNetworkId, Name)` |
| `NetworkSecurityGroupConfiguration.cs` | TPT, owns `SecurityRules` |
| `NsgRuleConfiguration.cs` | Owned entity |
| `PrivateDnsZoneConfiguration.cs` | TPT, owns `VirtualNetworkLinks` |
| `FrontDoorConfiguration.cs` | TPT, owns `Origins` et `EnvironmentSettings` |
| `PrivateEndpointConfigConfiguration.cs` | Entity liée à `AzureResource` (one-to-many), FK cascade |

### 6.2. Modification de AzureResourceConfiguration

```csharp
// Ajouter dans AzureResourceConfiguration.Configure() :
builder.HasMany(x => x.PrivateEndpointConfigs)
    .WithOne()
    .HasForeignKey(x => x.ResourceId)
    .OnDelete(DeleteBehavior.Cascade);

builder.Navigation(x => x.PrivateEndpointConfigs)
    .HasField("_privateEndpointConfigs")
    .UsePropertyAccessMode(PropertyAccessMode.Field);
```

### 6.3. Modification de ProjectDbContext

Ajouter les `DbSet<>` pour :
- `VirtualNetwork`, `VirtualNetworkEnvironmentSettings`, `Subnet`
- `NetworkSecurityGroup`, `NsgRule`
- `PrivateDnsZone`, `VirtualNetworkLink`
- `FrontDoor`, `FrontDoorOrigin`, `FrontDoorEnvironmentSettings`
- `PrivateEndpointConfig`

### 6.4. Modification de InfrastructureConfigReadRepository

- Charger les `PrivateEndpointConfigs` dans les includes de toutes les ressources
- Ajouter les nouveaux switch cases dans `MapResource()` et `GetResourceTypeString()`
- Mapper les `PrivateEndpointConfigs` vers des DTOs dans le read model

### 6.5. Migrations

1. `AddVirtualNetworkAndSubnets`
2. `AddNetworkSecurityGroup`
3. `AddPrivateDnsZone`
4. `AddFrontDoor`
5. `AddPrivateEndpointConfigToAzureResource`
6. `AddPublicNetworkAccessToEnvironmentSettings` (modifie 9 tables d'env settings)
7. `AddVnetIntegrationToCompute`

---

## 7. Contracts Layer

### 7.1. Nouveaux contracts

```
src/Api/InfraFlowSculptor.Contracts/
├── VirtualNetworks/
│   ├── Requests/
│   │   ├── CreateVirtualNetworkRequest.cs
│   │   ├── UpdateVirtualNetworkRequest.cs
│   │   ├── AddSubnetRequest.cs
│   │   └── UpdateSubnetRequest.cs
│   └── Responses/
│       ├── VirtualNetworkResponse.cs
│       └── SubnetResponse.cs
├── NetworkSecurityGroups/
│   ├── Requests/ ...
│   └── Responses/ ...
├── PrivateDnsZones/
│   ├── Requests/ ...
│   └── Responses/ ...
├── FrontDoors/
│   ├── Requests/ ...
│   └── Responses/ ...
└── PrivateEndpoints/
    ├── Requests/
    │   ├── AddPrivateEndpointRequest.cs
    │   └── UpdatePrivateEndpointRequest.cs
    └── Responses/
        ├── PrivateEndpointConfigResponse.cs
        └── AvailableGroupIdsResponse.cs
```

### 7.2. Modification des responses existantes

Toutes les `*Response.cs` des ressources supportant PE doivent inclure :

```csharp
public IReadOnlyList<PrivateEndpointConfigResponse> PrivateEndpointConfigs { get; init; } = [];
```

Les `*EnvironmentConfigResponse.cs` des ressources avec `PublicNetworkAccess` doivent inclure :

```csharp
public string? PublicNetworkAccess { get; init; }
```

---

## 8. API Layer

### 8.1. Nouveaux controllers

| Controller | Endpoints | Route |
|-----------|-----------|-------|
| `VirtualNetworkController` | GET, POST, PUT, DELETE + Subnet CRUD | `/resource-groups/{rgId}/virtual-networks` |
| `NetworkSecurityGroupController` | GET, POST, PUT, DELETE + Rule CRUD | `/resource-groups/{rgId}/network-security-groups` |
| `PrivateDnsZoneController` | GET, POST, PUT, DELETE + VNet Link CRUD | `/resource-groups/{rgId}/private-dns-zones` |
| `FrontDoorController` | GET, POST, PUT, DELETE + Origin CRUD | `/resource-groups/{rgId}/front-doors` |
| `PrivateEndpointController` | GET, POST, PUT, DELETE (transverse) | `/resources/{resourceId}/private-endpoints` |

### 8.2. Endpoints PE transversaux

```
GET    /resources/{resourceId}/private-endpoints              → Liste les PE configs
POST   /resources/{resourceId}/private-endpoints              → Ajoute un PE
PUT    /resources/{resourceId}/private-endpoints/{peId}       → Modifie un PE
DELETE /resources/{resourceId}/private-endpoints/{peId}       → Supprime un PE
GET    /resources/{resourceId}/private-endpoints/group-ids    → GroupIds disponibles
```

### 8.3. Mapping configs (Mapster)

Créer `PrivateEndpointMappingConfig.cs`, `VirtualNetworkMappingConfig.cs`, etc.

---

## 9. Bicep Generation — Modules génériques

### 9.1. Architecture des modules PE (réutilisable)

Le principe : un **module générique `privateEndpoint.module.bicep`** déployé N fois par ressource cible.

```
modules/
├── Common/
│   ├── privateEndpoint.module.bicep       # Module générique PE
│   ├── privateEndpoint.types.bicep        # Types exportés
│   ├── privateDnsZoneGroup.module.bicep   # DNS zone group attachment
│   └── privateDnsZone.module.bicep        # DNS zone creation (si auto-managed)
├── VirtualNetwork/
│   ├── virtualNetwork.module.bicep
│   └── types.bicep
├── NetworkSecurityGroup/
│   ├── networkSecurityGroup.module.bicep
│   └── types.bicep
└── FrontDoor/
    ├── frontDoor.module.bicep
    └── types.bicep
```

### 9.2. Module générique Private Endpoint

```bicep
// modules/Common/privateEndpoint.module.bicep
@description('Name of the private endpoint')
param name string

@description('Location of the private endpoint')
param location string

@description('Resource ID of the target resource')
param privateLinkServiceId string

@description('Sub-resource group ID(s)')
param groupIds array

@description('Resource ID of the subnet')
param subnetId string

@description('Private DNS Zone ID for DNS record registration')
param privateDnsZoneId string = ''

@description('Custom network interface name')
param customNetworkInterfaceName string = ''

param tags object = {}

resource privateEndpoint 'Microsoft.Network/privateEndpoints@2023-11-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    privateLinkServiceConnections: [
      {
        name: name
        properties: {
          privateLinkServiceId: privateLinkServiceId
          groupIds: groupIds
        }
      }
    ]
    subnet: {
      id: subnetId
    }
    customNetworkInterfaceName: !empty(customNetworkInterfaceName) ? customNetworkInterfaceName : null
  }
}

resource dnsZoneGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-11-01' = if (!empty(privateDnsZoneId)) {
  parent: privateEndpoint
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'config'
        properties: {
          privateDnsZoneId: privateDnsZoneId
        }
      }
    ]
  }
}

output id string = privateEndpoint.id
output networkInterfaceId string = privateEndpoint.properties.networkInterfaces[0].id
```

### 9.3. Intégration dans le pipeline de génération

**Nouvelle stage** entre `OutputInjectionStage` (500) et `AppSettingsInjectionStage` (600) :

```
550 | PrivateEndpointInjectionStage | Génère les déploiements PE companion pour chaque ressource qui a des PE configs
```

**Logique :**
1. Pour chaque `ModuleWorkItem` dont la `ResourceDefinition` a des `PrivateEndpointConfigs` :
   - Créer un `CompanionModule` pour chaque PE config
   - Le companion utilise le module générique `Common/privateEndpoint.module.bicep`
   - Le nom du déploiement suit : `{resourceName}-pe-{groupId}`
2. `main.bicep` déploie les companions **après** le module principal (via `dependsOn`)

### 9.4. Generators pour nouvelles ressources

| Generator | Fichier |
|-----------|---------|
| `VirtualNetworkTypeBicepGenerator` | Standard, émet subnets inline dans le module |
| `NetworkSecurityGroupTypeBicepGenerator` | Standard, émet rules inline |
| `PrivateDnsZoneTypeBicepGenerator` | Émet zone + VNet links |
| `FrontDoorTypeBicepGenerator` | Émet profile + endpoints + origins + routes |

### 9.5. Modification de main.bicep pour PE

```bicep
// Après chaque module principal avec PE :
module kvPrivateEndpoint 'modules/Common/privateEndpoint.module.bicep' = {
  name: 'keyVault-pe-vault'
  params: {
    name: '${keyVault.outputs.name}-pe-vault'
    location: location
    privateLinkServiceId: keyVault.outputs.id
    groupIds: ['vault']
    subnetId: vnet.outputs.subnets['pe-subnet'].id   // Resolved from config
    privateDnsZoneId: pdnszVault.outputs.id
    tags: tags
  }
  dependsOn: [keyVault]
}
```

### 9.6. Modification des ressources cibles (publicNetworkAccess)

Les generators existants doivent conditionner la propriété `publicNetworkAccess` :

```bicep
// Dans chaque module de ressource privatisable :
@allowed(['Enabled', 'Disabled', 'SecuredByPerimeter'])
param publicNetworkAccess string = 'Enabled'

resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
  // ...
  properties: {
    // ... existing
    publicNetworkAccess: publicNetworkAccess
    networkAcls: publicNetworkAccess == 'Disabled' ? {
      defaultAction: 'Deny'
      bypass: 'AzureServices'
    } : null
  }
}
```

### 9.7. VNet Integration Bicep (Compute)

```bicep
// Dans WebApp/FunctionApp module (si vnetIntegrationSubnetId configuré) :
resource vnetIntegration 'Microsoft.Web/sites/networkConfig@2023-12-01' = {
  parent: webApp
  name: 'virtualNetwork'
  properties: {
    subnetResourceId: vnetIntegrationSubnetId
    swiftSupported: true
  }
}
```

### 9.8. GenerationCore — Nouvelles constantes

```csharp
// AzureResourceTypes.cs :
public const string VirtualNetwork = "VirtualNetwork";
public const string NetworkSecurityGroup = "NetworkSecurityGroup";
public const string PrivateDnsZone = "PrivateDnsZone";
public const string FrontDoor = "FrontDoor";

public const string VirtualNetworkType = "Microsoft.Network/virtualNetworks";
public const string NetworkSecurityGroupType = "Microsoft.Network/networkSecurityGroups";
public const string PrivateDnsZoneType = "Microsoft.Network/privateDnsZones";
public const string FrontDoorType = "Microsoft.Cdn/profiles";
public const string PrivateEndpointType = "Microsoft.Network/privateEndpoints";
```

---

## 10. Frontend — Configuration UI

### 10.1. Nouvelles interfaces TypeScript

```
src/Front/src/app/shared/interfaces/
├── virtual-network.interface.ts
├── network-security-group.interface.ts
├── private-dns-zone.interface.ts
├── front-door.interface.ts
└── private-endpoint.interface.ts
```

### 10.2. Nouveaux services Angular

```
src/Front/src/app/shared/services/
├── virtual-network.service.ts
├── network-security-group.service.ts
├── private-dns-zone.service.ts
├── front-door.service.ts
└── private-endpoint.service.ts
```

### 10.3. resource-type.enum.ts — Modifications

```typescript
// Ajout dans ResourceTypeEnum :
VirtualNetwork = 'VirtualNetwork',
NetworkSecurityGroup = 'NetworkSecurityGroup',
PrivateDnsZone = 'PrivateDnsZone',
FrontDoor = 'FrontDoor',

// Nouvelle catégorie :
RESOURCE_TYPE_CATEGORIES = {
  // ... existing
  Networking: [
    ResourceTypeEnum.VirtualNetwork,
    ResourceTypeEnum.NetworkSecurityGroup,
    ResourceTypeEnum.PrivateDnsZone,
    ResourceTypeEnum.FrontDoor,
  ],
};

// Icons :
RESOURCE_TYPE_ICONS = {
  // ... existing
  [ResourceTypeEnum.VirtualNetwork]: 'hub',          // Material icon
  [ResourceTypeEnum.NetworkSecurityGroup]: 'shield',
  [ResourceTypeEnum.PrivateDnsZone]: 'dns',
  [ResourceTypeEnum.FrontDoor]: 'language',
};

// Abbreviations :
RESOURCE_TYPE_ABBREVIATIONS = {
  // ... existing
  [ResourceTypeEnum.VirtualNetwork]: 'vnet',
  [ResourceTypeEnum.NetworkSecurityGroup]: 'nsg',
  [ResourceTypeEnum.PrivateDnsZone]: 'pdnsz',
  [ResourceTypeEnum.FrontDoor]: 'afd',
};

// Parent-child :
PARENT_CHILD_RESOURCE_TYPES = {
  // ... existing
  [ResourceTypeEnum.VirtualNetwork]: [],  // Subnets inline, not separate resources
};
```

### 10.4. Resource Edit — VNet avec Subnets (inline editing)

Le VNet aura un pattern similaire au StorageAccount (sub-resources inline) :

```html
@if (resourceType === 'VirtualNetwork') {
  <div class="form-section">
    <h3>{{ 'RESOURCE_EDIT.VIRTUAL_NETWORK.CONFIGURATION' | translate }}</h3>
    <app-ds-toggle formControlName="enableDdosProtection"
      [label]="'RESOURCE_EDIT.VIRTUAL_NETWORK.ENABLE_DDOS' | translate" />
  </div>

  <!-- Subnets section (like blob containers for StorageAccount) -->
  <div class="form-section">
    <h3>{{ 'RESOURCE_EDIT.VIRTUAL_NETWORK.SUBNETS' | translate }}</h3>
    <app-ds-button (click)="openAddSubnetDialog()" icon="add">
      {{ 'RESOURCE_EDIT.VIRTUAL_NETWORK.ADD_SUBNET' | translate }}
    </app-ds-button>
    <app-ds-table [dataSource]="subnets" [columns]="subnetColumns">
      <!-- Name, AddressPrefix, Delegation, Actions -->
    </app-ds-table>
  </div>
}
```

### 10.5. Onglet Private Endpoint (transversal — toutes les ressources)

Ajouter un **3ème onglet** dans `resource-edit.component.html` pour les ressources privatisables :

```html
<!-- Nouvel onglet "Networking" -->
@if (supportsPrivateEndpoints()) {
  <mat-tab [label]="'RESOURCE_EDIT.TABS.NETWORKING' | translate">
    <div class="tab-content">
      <!-- Public Network Access (per-env) -->
      <div class="form-section">
        <h3>{{ 'RESOURCE_EDIT.NETWORKING.PUBLIC_ACCESS' | translate }}</h3>
        <app-ds-select formControlName="publicNetworkAccess"
          [options]="publicNetworkAccessOptions"
          [label]="'RESOURCE_EDIT.NETWORKING.PUBLIC_NETWORK_ACCESS' | translate" />
      </div>

      <!-- Private Endpoints list -->
      <div class="form-section">
        <h3>{{ 'RESOURCE_EDIT.NETWORKING.PRIVATE_ENDPOINTS' | translate }}</h3>
        <app-ds-button (click)="openAddPrivateEndpointDialog()" icon="add">
          {{ 'RESOURCE_EDIT.NETWORKING.ADD_PE' | translate }}
        </app-ds-button>
        @for (pe of privateEndpointConfigs(); track pe.id) {
          <app-ds-card>
            <div class="pe-config">
              <span>{{ pe.subnetName }} → {{ pe.groupId }}</span>
              <app-ds-icon-button icon="delete" (click)="removePrivateEndpoint(pe.id)" />
            </div>
          </app-ds-card>
        }
      </div>

      <!-- VNet Integration (compute only) -->
      @if (isComputeResource()) {
        <div class="form-section">
          <h3>{{ 'RESOURCE_EDIT.NETWORKING.VNET_INTEGRATION' | translate }}</h3>
          <app-ds-select formControlName="vnetIntegrationSubnetId"
            [options]="availableSubnets()"
            [label]="'RESOURCE_EDIT.NETWORKING.INTEGRATION_SUBNET' | translate" />
        </div>
      }
    </div>
  </mat-tab>
}
```

### 10.6. Add Private Endpoint Dialog

```
src/Front/src/app/features/resource-edit/add-private-endpoint-dialog/
├── add-private-endpoint-dialog.component.ts
├── add-private-endpoint-dialog.component.html
└── add-private-endpoint-dialog.component.scss
```

**Champs :**
1. **VNet** (select parmi les VNets du même RG ou cross-config)
2. **Subnet** (select filtré par VNet sélectionné)
3. **Group ID** (select filtré par le ResourceType de la ressource courante)
4. **Auto-approval** (toggle)
5. **Private DNS Zone** (select ou auto-derive)
6. **Custom NIC Name** (optional text)

### 10.7. Add Resource Dialog — Nouveaux types

Ajouter la catégorie "Networking" dans le step `type` du dialog. Les VNets seront dans la catégorie `Networking` avec une icône réseau.

Le step `environments` pour VNet demandera les `addressSpaces` per-env (textarea JSON ou input répétable).

### 10.8. i18n (fr.json + en.json)

Ajouter les clés :
- `RESOURCE_EDIT.TABS.NETWORKING`
- `RESOURCE_EDIT.NETWORKING.*` (section labels, button labels)
- `RESOURCE_EDIT.VIRTUAL_NETWORK.*` (VNet-specific)
- `RESOURCE_EDIT.NSG.*`
- `RESOURCE_EDIT.FRONT_DOOR.*`
- `ADD_DIALOG_TITLE_VirtualNetwork`, `ADD_DIALOG_TITLE_NetworkSecurityGroup`, etc.
- `RESOURCE_TYPE.VirtualNetwork`, `RESOURCE_TYPE.NetworkSecurityGroup`, etc.

---

## 11. Validation de SKU pour la privatisation

### 11.1. Matrice de compatibilité SKU ↔ PE

| Ressource | SKUs supportant PE | SKUs NE supportant PAS PE |
|-----------|-------------------|--------------------------|
| KeyVault | Standard, Premium | — (tous supportent) |
| StorageAccount | Standard_LRS, Standard_GRS, Standard_RAGRS, Standard_ZRS, Premium_LRS, Premium_ZRS | — (tous supportent) |
| RedisCache | **Premium uniquement** | Basic, Standard |
| AppConfiguration | Standard | Free |
| CosmosDb | Tous | — |
| SqlServer | Tous | — |
| ServiceBusNamespace | **Premium uniquement** | Basic, Standard |
| EventHubNamespace | **Premium, Dedicated** | Basic, Standard |
| ContainerRegistry | **Premium uniquement** | Basic, Standard |
| WebApp/FunctionApp | Standard, Premium, PremiumV2, PremiumV3, Isolated, IsolatedV2 | Free, Shared, Basic |
| FrontDoor | **Premium_AzureFrontDoor** (pour PE origins) | Standard_AzureFrontDoor |

### 11.2. Implémentation Domain — Catalogue de compatibilité

```csharp
public static class PrivateEndpointSkuCompatibility
{
    /// <summary>
    /// Validates whether the given SKU supports private endpoints for the resource type.
    /// Returns true if PE is supported, false otherwise.
    /// </summary>
    public static bool IsPrivateEndpointCompatible(string resourceType, string? sku)
    {
        return resourceType switch
        {
            AzureResourceTypes.RedisCache => sku?.Equals("Premium", StringComparison.OrdinalIgnoreCase) == true,
            AzureResourceTypes.AppConfiguration => !sku?.Equals("Free", StringComparison.OrdinalIgnoreCase) == true,
            AzureResourceTypes.ServiceBusNamespace => sku?.Equals("Premium", StringComparison.OrdinalIgnoreCase) == true,
            AzureResourceTypes.EventHubNamespace => sku is "Premium" or "Dedicated",
            AzureResourceTypes.ContainerRegistry => sku?.Equals("Premium", StringComparison.OrdinalIgnoreCase) == true,
            _ => true, // KeyVault, StorageAccount, CosmosDb, SqlServer support PE on all SKUs
        };
    }
}
```

### 11.3. Validation dans AddPrivateEndpointCommandValidator

```csharp
public class AddPrivateEndpointCommandValidator : AbstractValidator<AddPrivateEndpointCommand>
{
    public AddPrivateEndpointCommandValidator()
    {
        RuleFor(x => x.ResourceId).NotEmpty();
        RuleFor(x => x.SubnetId).NotEmpty();
        RuleFor(x => x.GroupId).NotEmpty()
            .Must((cmd, groupId) => IsValidGroupId(cmd.ResourceType, groupId))
            .WithMessage("Invalid group ID for this resource type.");
    }
}
```

### 11.4. Validation dans le Handler (post-load, domain-level)

```csharp
// Dans AddPrivateEndpointCommandHandler :
var resource = await _repository.GetByIdAsync(command.ResourceId);
var sku = GetCurrentSku(resource); // Extract from env settings

if (!PrivateEndpointSkuCompatibility.IsPrivateEndpointCompatible(resource.ResourceType, sku))
    return Errors.PrivateEndpoint.IncompatibleSku(resource.ResourceType, sku);
```

### 11.5. Frontend — Validation dynamique

```typescript
// Dans le composant Networking tab :
get canAddPrivateEndpoint(): boolean {
  const sku = this.getEffectiveSku();
  return PRIVATE_ENDPOINT_SKU_REQUIREMENTS[this.resourceType]?.includes(sku) ?? true;
}

// Afficher un warning si SKU incompatible :
@if (!canAddPrivateEndpoint) {
  <app-ds-banner type="warning">
    {{ 'RESOURCE_EDIT.NETWORKING.SKU_INCOMPATIBLE' | translate: { sku: currentSku } }}
  </app-ds-banner>
}
```

### 11.6. AppServicePlan SKU pour WebApp/FunctionApp PE

La validation pour WebApp/FunctionApp passe par le SKU de leur **AppServicePlan** parent :

```csharp
// Charger l'ASP parent et vérifier son SKU
var aspSku = GetAppServicePlanSku(webApp.AppServicePlanId, environmentName);
if (!AppServicePlanSkuSupportsVnetIntegration(aspSku))
    return Errors.PrivateEndpoint.AppServicePlanSkuIncompatible(aspSku);
```

---

## 12. Séquencement des PRs

### PR 1 — Domain & Persistence (Fondations réseau)

**Scope :** VirtualNetwork aggregate + Subnet + NSG aggregate + PrivateDnsZone aggregate + PrivateEndpointConfig entity + PublicNetworkAccess VO + catalogues (GroupId, DNS Zone names, SKU compat)

**Fichiers :** ~35 créés, ~8 modifiés
**Dépendances :** Aucune
**Tests :** Domain unit tests pour les invariants, catalogues, et validations

### PR 2 — Application + Infrastructure + API (CQRS réseau)

**Scope :** Commands/Queries/Handlers pour VNet, NSG, PrivateDnsZone + Repositories + EF Configs + Migrations + Controllers + Contracts

**Fichiers :** ~60 créés, ~15 modifiés
**Dépendances :** PR 1
**Tests :** Handler tests, validator tests

### PR 3 — Private Endpoint transversal (Backend)

**Scope :** PE commands/handlers sur la base AzureResource + modification des 12 ressources existantes (PublicNetworkAccess dans env settings) + VNet Integration sur compute + migration + API endpoints transversaux

**Fichiers :** ~20 créés, ~30 modifiés
**Dépendances :** PR 1, PR 2
**Tests :** PE handler tests, SKU validation tests

### PR 4 — Bicep Generation (Modules génériques + stages)

**Scope :** Module générique PE + generators pour VNet/NSG/PrivateDnsZone + PrivateEndpointInjectionStage + modification des generators existants (publicNetworkAccess) + VNet Integration Bicep + FrontDoor generator

**Fichiers :** ~15 créés, ~20 modifiés
**Dépendances :** PR 1, PR 2, PR 3
**Tests :** Generator tests, stage tests, parity tests

### PR 5 — FrontDoor (Backend + Bicep)

**Scope :** FrontDoor aggregate + CQRS + persistence + Bicep generator

**Fichiers :** ~25 créés, ~10 modifiés
**Dépendances :** PR 1, PR 2
**Tests :** Domain + handler + generator tests

### PR 6 — Frontend (Networking UI)

**Scope :** Tous les composants frontend : nouvelles ressources réseau, onglet Networking transversal, add PE dialog, SKU warnings, VNet Integration UI, i18n

**Fichiers :** ~20 créés, ~15 modifiés
**Dépendances :** PR 2, PR 3 (API endpoints disponibles)
**Tests :** Component tests si applicable

---

## 13. Risques et dépendances

### Risques techniques

| # | Risque | Mitigation |
|---|--------|-----------|
| 1 | **Blast radius sur AzureResource** — Ajouter `PrivateEndpointConfigs` modifie la base class partagée par 18 agrégats | Bien isoler l'entité PE comme une collection owned avec lazy loading. Tests de régression sur tous les agrégats. |
| 2 | **Complexité Bicep PE** — Le module PE dépend de la résolution des IDs de subnet et DNS zone au deploy time | Utiliser des `existing` resource references dans `main.bicep` pour résoudre les IDs. Pattern éprouvé dans le code existant (cross-config references). |
| 3 | **SKU validation cross-resource** — WebApp PE dépend du SKU de l'AppServicePlan parent | Charger le parent dans le handler PE et valider explicitement. Pas de validation lazy. |
| 4 | **Migration EF lourde** — 7 migrations impactant de nombreuses tables | Regrouper les migrations par PR pour minimiser les conflits. Tester sur snapshot frais. |
| 5 | **Frontend complexity** — L'onglet Networking est transversal (toutes les ressources) | Extraire un composant `NetworkingTabComponent` réutilisable injecté dans `resource-edit` plutôt que de dupliquer. |
| 6 | **Performance read model** — Include de PrivateEndpointConfigs sur chaque ressource | Lazy ou split query. Le PE est rarement consulté hors du tab Networking. |
| 7 | **Ordering Bicep deployments** — PE doit déployer après la ressource cible ET le VNet/Subnet | Le `dependsOn` dans main.bicep gère cela nativement. Le `PrivateEndpointInjectionStage` émet les `dependsOn` automatiquement. |

### Dépendances externes

- **Azure API versions** : Utiliser `2023-11-01` pour Network, `2023-07-01` pour Key Vault, CDN profiles
- **Bicep linting** : Valider que les modules génériques passent `az bicep build` sans warning
- **AVM alignment** : Le module PE générique s'inspire des Azure Verified Modules mais reste indépendant (pas de dépendance directe AVM)

### Prérequis techniques

- Aucun refactoring préalable bloquant identifié
- Le pattern `CompanionModule` existant (StorageAccount) est directement réutilisable pour les PE
- Le pattern `CrossConfigResourceReference` est réutilisable pour les VNets partagés entre configs
- `InfrastructureConfigReadRepository` devra supporter le chargement conditionnel des PE (optimization)

---

## Annexes

### A. Schéma de déploiement Bicep typique (privatisé)

```
main.bicep
├── modules/VirtualNetwork/virtualNetwork.module.bicep     ← VNet + subnets inline
├── modules/NetworkSecurityGroup/nsg.module.bicep          ← NSG + rules
├── modules/Common/privateDnsZone.module.bicep             ← DNS zone (×N par type)
├── modules/KeyVault/keyVault.module.bicep                 ← KV avec publicNetworkAccess: 'Disabled'
├── modules/Common/privateEndpoint.module.bicep            ← PE pour KV (dependsOn: KV + VNet)
├── modules/StorageAccount/storageAccount.module.bicep     ← SA avec publicNetworkAccess: 'Disabled'
├── modules/Common/privateEndpoint.module.bicep            ← PE pour SA-blob
├── modules/Common/privateEndpoint.module.bicep            ← PE pour SA-queue
├── modules/FrontDoor/frontDoor.module.bicep               ← AFD avec private link origins
└── ...
```

### B. Flow Frontend — Ajout PE sur une ressource existante

1. Utilisateur ouvre `resource-edit` pour un KeyVault
2. Tab "Networking" affiche l'état actuel (public access + PE list)
3. Clic "Add Private Endpoint" → Dialog
4. Dialog : sélection VNet → Subnet → Group ID (auto-filtré : "vault") → DNS Zone (auto-derived)
5. Confirmation → POST `/resources/{kvId}/private-endpoints`
6. Refresh de la liste PE → Nouveau PE visible
7. Optionnel : basculer `publicNetworkAccess` sur "Disabled" (per-env dans l'onglet Environments)

### C. Estimation volumétrique

| Couche | Fichiers créés | Fichiers modifiés | Total |
|--------|---------------|-------------------|-------|
| Domain | ~20 | ~5 | ~25 |
| Application | ~35 | ~8 | ~43 |
| Infrastructure | ~12 | ~10 | ~22 |
| Contracts | ~15 | ~12 | ~27 |
| API | ~8 | ~3 | ~11 |
| BicepGeneration | ~8 | ~20 | ~28 |
| GenerationCore | ~2 | ~2 | ~4 |
| Frontend | ~20 | ~15 | ~35 |
| Tests | ~25 | ~5 | ~30 |
| i18n | 0 | 2 | 2 |
| **TOTAL** | **~145** | **~82** | **~227** |
