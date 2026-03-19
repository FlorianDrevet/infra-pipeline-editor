# Ressources Azure Managées — Infra Flow Sculptor

Ce document liste les ressources Azure provisionnées ou gérées par **Infra Flow Sculptor**.

---

## Ressources provisionnées par l'application

Les ressources suivantes sont **créées et gérées** via Infra Flow Sculptor (configuration stockée en BDD + génération Bicep) :

| Type | Entité Domaine | Notes |
|------|---------------|-------|
| Resource Group | `ResourceGroup` | Contenant logique des ressources |
| Azure Key Vault | `KeyVault` | Gestion des secrets, SKU Premium/Standard |
| Azure Cache for Redis | `RedisCache` | Cache distribué, TLS 1.0/1.1/1.2, policies mémoire |
| Azure Storage Account | `StorageAccount` | Stockage objet, HTTPS only, TLS min |

---

## Ressources Azure de l'infrastructure applicative

Ces ressources sont **requises pour faire tourner** Infra Flow Sculptor lui-même (hors scope de génération Bicep) :

### Azure Entra ID

| Ressource | Client ID | Notes |
|-----------|-----------|-------|
| App Registration — API | `4f7f2dbd-8a11-42c4-bedc-acbb127e9394` | Audience JWT : `api://4f7f2dbd-8a11-42c4-bedc-acbb127e9394` |
| App Registration — Frontend | `24c34231-a984-43b3-8ac3-9278ebd067ef` | MSAL PKCE, redirect : `http://localhost:4200` (dev) |

### Base de données

| Ressource | Type | Notes |
|-----------|------|-------|
| PostgreSQL | Azure Database for PostgreSQL Flexible Server | EF Core, migrations auto au démarrage |

### Stockage

| Ressource | Type | Usage |
|-----------|------|-------|
| Blob Storage | Azure Storage Account | Stockage des fichiers Bicep générés par `BicepGenerator.Api` |

---

## Conventions de nommage

Voir [conventions-nommage.md](./conventions-nommage.md) pour les règles de nommage des ressources Azure.

---

## Localisation

Les ressources Azure sont déployées dans les régions suivantes (configurables via le value object `Location`) :

| Valeur | Région Azure |
|--------|-------------|
| `FranceCentral` | France Centre |
| `WestEurope` | Europe Ouest |
| `NorthEurope` | Europe Nord |
| `EastUS` | USA Est |

---

*Référence : [Architecture Cloud (wiki ADO)](https://dev.azure.com/florian-drevet/Infra%20Flow%20Sculptor/_wiki/wikis/Infra-Flow-Sculptor-Wiki/Documentation-Azure/Architecture-Cloud) | [Authentification (wiki ADO)](https://dev.azure.com/florian-drevet/Infra%20Flow%20Sculptor/_wiki/wikis/Infra-Flow-Sculptor-Wiki/Reference-Technique/Authentification)*
