# Architecture Cloud — Infra Flow Sculptor

## Vue d'ensemble

L'architecture Azure d'Infra Flow Sculptor s'appuie sur les services suivants :

```
┌─────────────────────────────────────────────────────────┐
│                   Azure Subscription                     │
│                                                         │
│  ┌──────────────┐    ┌──────────────┐                  │
│  │  Resource    │    │  Resource    │                  │
│  │  Group App   │    │  Group Data  │                  │
│  │              │    │              │                  │
│  │ - App Svc    │    │ - PostgreSQL │                  │
│  │ - Container  │    │ - Blob Stor. │                  │
│  │              │    │ - Key Vault  │                  │
│  └──────────────┘    └──────────────┘                  │
│                                                         │
│  ┌──────────────────────────────────┐                  │
│  │     Azure Entra ID (IAM)         │                  │
│  │  - App Registration (API)        │                  │
│  │  - App Registration (Frontend)   │                  │
│  │  - Service Principals            │                  │
│  └──────────────────────────────────┘                  │
└─────────────────────────────────────────────────────────┘
```

---

## Services Azure utilisés

### Compute

| Service | Usage | SKU recommandé |
|---------|-------|---------------|
| Azure App Service | Hébergement des APIs .NET | B1 (dev) / P1v3 (prod) |
| Azure Container Apps | Alternative pour les APIs (optionnel) | Consumption |

### Données

| Service | Usage | Notes |
|---------|-------|-------|
| Azure Database for PostgreSQL | Base de données principale | Flexible Server |
| Azure Blob Storage | Stockage des fichiers Bicep générés | LRS en dev, GRS en prod |
| Azure Key Vault | Secrets applicatifs, certificats | Standard |
| Azure Cache for Redis | Cache applicatif (optionnel) | C1 Basic (dev) |

### Identité et Sécurité

| Service | Usage |
|---------|-------|
| Azure Entra ID | Authentification JWT Bearer (API + Frontend) |
| Managed Identity | Authentification des services Azure entre eux |
| Azure RBAC | Contrôle d'accès aux ressources |

### DevOps

| Service | Usage |
|---------|-------|
| Azure DevOps Repos | Code source du projet |
| Azure DevOps Pipelines | CI/CD |
| Azure DevOps Boards | Gestion de projet (Scrum) |
| Azure DevOps Wiki | Documentation (ce wiki) |

---

## Flux d'authentification

```
Frontend (Angular)
      │
      │  MSAL (PKCE)
      ▼
Azure Entra ID ──────────────► JWT Token (audience: api://4f7f2dbd-8a11-42c4-bedc-acbb127e9394)
      │
      │  Bearer token
      ▼
InfraFlowSculptor.Api
      │
      │  Managed Identity / Service Principal
      ▼
Azure Resources (PostgreSQL, Blob Storage, Key Vault)
```

---

## Environnements

| Environnement | Description | Branche Git |
|---------------|-------------|-------------|
| `dev` | Développement local (Aspire) | toute branche |
| `staging` | Pré-production | `develop` |
| `prod` | Production | `main` |

---

*Référence : [Vue d'ensemble Architecture (wiki ADO)](https://dev.azure.com/florian-drevet/Infra%20Flow%20Sculptor/_wiki/wikis/Infra-Flow-Sculptor-Wiki/Architecture/Vue-d-ensemble)*
