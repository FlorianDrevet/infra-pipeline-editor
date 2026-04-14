# Références cross-config — Dépendances entre configurations

## Concept

Une **InfrastructureConfig** modélise un ensemble de ressources Azure déployées ensemble (un seul fichier `main.bicep`). Mais dans un projet réel, certaines ressources déployées dans une config ont besoin de **référencer des ressources d'une autre config** du même projet. Exemples :

- Une **Container App** (Config B) a besoin de l'ID d'un **Key Vault** (Config A) pour lire ses secrets
- Un **App Service** (Config B) doit accepter un **Role Assignment** sur un **Storage Account** (Config A)
- Une **SQL Database** (Config B) est reliée à un **SQL Server** (Config A) via une relation parent-enfant

Les **références cross-config** permettent de modéliser ces dépendances dans l'application, et de les traduire automatiquement en déclarations `existing` dans le Bicep généré.

---

## Architecture technique

### Domain — `CrossConfigResourceReference`

```
Fichier : src/Api/InfraFlowSculptor.Domain/InfrastructureConfigAggregate/Entities/CrossConfigResourceReference.cs
```

L'entité vit comme enfant de l'agrégat `InfrastructureConfig` (la config **source** qui a besoin de la référence) :

```csharp
public sealed class CrossConfigResourceReference : Entity<CrossConfigResourceReferenceId>
{
    /// <summary>ID de la config cible qui possède la ressource référencée.</summary>
    public InfrastructureConfigId TargetConfigId { get; private set; }

    /// <summary>ID de la ressource Azure référencée dans la config cible.</summary>
    public AzureResourceId TargetResourceId { get; private set; }

    /// <summary>ID de la config source (celle qui possède cette référence).</summary>
    public InfrastructureConfigId InfraConfigId { get; private set; }
}
```

L'agrégat `InfrastructureConfig` expose les méthodes métier :
- `AddCrossConfigReference(targetConfigId, targetResourceId)` — Ajoute une référence (validates same-config et doublon)
- `RemoveCrossConfigReference(referenceId)` — Supprime une référence

La collection `CrossConfigReferences` est une `IReadOnlyCollection<CrossConfigResourceReference>` encapsulée dans l'agrégat.

### Application — Commandes et Queries

| Fichier | Rôle |
|---------|------|
| `AddCrossConfigReferenceCommand` / Handler | Ajoute une référence. Vérifie accès en écriture, que la ressource cible existe, et qu'elle est dans le même projet. |
| `RemoveCrossConfigReferenceCommand` / Handler | Supprime une référence existante. |
| `ListCrossConfigReferencesQuery` / Handler | Liste les références **sortantes** (cette config → autres configs). |
| `ListIncomingCrossConfigReferencesQuery` / Handler | Liste les références **entrantes** (autres configs → cette config). |

### Infrastructure — Résolution des metadata

Le `InfrastructureConfigReadRepository` enrichit les références brutes avec les informations nécessaires à la génération Bicep :
- Nom de la ressource cible, son type ARM, son abréviation
- Nom du resource group cible
- Nom de la config cible

### Bicep — Déclarations `existing`

Le modèle `ExistingResourceReference` (`GenerationCore`) est passé au `BicepAssembler` qui génère :

1. Les **resource groups externes** comme `existing` :
```bicep
// Cross-configuration existing resource groups
resource existing_rgSharedInfra 'Microsoft.Resources/resourceGroups@2024-07-01' existing = {
  name: resourceGroupName('rg-shared-infra', 'rg', 'ResourceGroup')
}
```

2. Les **ressources externes** comme `existing` avec un scope vers le RG :
```bicep
// Cross-configuration existing resources
resource existing_kvShared 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceName('kv-shared', 'kv', 'KeyVault')
  scope: existing_rgSharedInfra
}
```

Le mot-clé Bicep `existing` indique à ARM que la ressource existe déjà et ne doit pas être créée — on récupère simplement une référence à son ID et ses propriétés.

---

## Flux utilisateur

```
1. L'utilisateur ouvre Config B dans le frontend
2. Onglet "Références cross-config"
3. Clique "Ajouter une référence"
4. Sélectionne une ressource d'une autre config du même projet
5. L'API valide : même projet, ressource existante, pas de doublon
6. La référence est créée dans l'agrégat InfrastructureConfig

Lors de la génération Bicep :
7. Le handler de génération charge les cross-config references
8. Les résout en ExistingResourceReference (nom, type ARM, RG, abréviation)
9. Le BicepAssembler génère les blocs `existing` dans main.bicep
10. Les modules qui ont besoin de la ressource reçoivent une référence à son ID
```

---

## Endpoints API

| Méthode | URL | Description |
|---------|-----|-------------|
| `GET` | `/infra-config/{id}/cross-config-references` | Liste les références sortantes |
| `POST` | `/infra-config/{id}/cross-config-references` | Ajoute une référence |
| `DELETE` | `/infra-config/{id}/cross-config-references/{refId}` | Supprime une référence |
| `GET` | `/infra-config/{id}/incoming-cross-config-references` | Liste les références entrantes |

---

## Frontend

Le composant `config-detail.component.ts` charge les références sortantes et entrantes en parallèle :

```typescript
const [refs, incomingRefs] = await Promise.all([
  this.infraConfigService.getCrossConfigReferences(configId),
  this.infraConfigService.getIncomingCrossConfigReferences(configId),
]);
```

Les deux listes sont affichées dans un onglet dédié, groupées par config source/cible.

---

## Pages connexes

- [Génération Bicep](../architecture/bicep-generation.md) — Le moteur qui consomme les références pour générer les déclarations `existing`
- [CQRS et MediatR](../architecture/cqrs-patterns.md) — Pattern des commandes et queries
- [Domain-Driven Design](../architecture/ddd-concepts.md) — Entités et agrégats
