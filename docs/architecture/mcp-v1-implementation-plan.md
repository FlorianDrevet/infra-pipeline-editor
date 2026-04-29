# MCP V1 — Plan d'implementation et contrats des tools

> Ce document est le plan d'implementation detaille pour la V1 du serveur MCP d'Infra Flow Sculptor.
> Il contient les contrats JSON exhaustifs de chaque tool, le decoupage par phase, la structure cible du code et les dependances entre composants.
>
> **Pre-requis :** lire [mcp-integration.md](mcp-integration.md) pour comprendre les concepts MCP avant de lire ce plan.

---

## Sommaire

1. [Perimetre V1](#1-perimetre-v1)
2. [Decisions d'architecture](#2-decisions-darchitecture)
3. [Structure cible du code](#3-structure-cible-du-code)
4. [Phases d'implementation](#4-phases-dimplementation)
5. [Contrats des tools — Discovery](#5-contrats-des-tools--discovery)
6. [Contrats des tools — Creation conversationnelle](#6-contrats-des-tools--creation-conversationnelle)
7. [Contrats des tools — Generation](#7-contrats-des-tools--generation)
8. [Contrats des tools — Import IaC](#8-contrats-des-tools--import-iac)
9. [Resources MCP](#9-resources-mcp)
10. [Prompts MCP](#10-prompts-mcp)
11. [Modele canonique d'import](#11-modele-canonique-dimport)
12. [Dependances par phase](#12-dependances-par-phase)
13. [Strategie de tests](#13-strategie-de-tests)
14. [Checklist de validation V1](#14-checklist-de-validation-v1)

---

## 1. Perimetre V1

### IN scope

- Transport `stdio` uniquement (serveur local lance par VS Code)
- 8 tools couvrant discovery, creation conversationnelle, generation Bicep et import IaC
- 2 resources MCP en lecture
- 1 prompt MCP de cadrage creation
- Import ARM JSON comme premier adaptateur source
- `.vscode/mcp.json` au niveau workspace
- Tests unitaires pour chaque tool et adaptateur

### OUT of scope V1

- Transport HTTP / endpoint deploye distant
- Authentification / autorisation MCP (le serveur tourne en local, l'utilisateur est implicite)
- Import Terraform (necessite un parseur HCL ou `terraform show -json` en prerequis)
- Import depuis diagrammes (draw.io, Mermaid)
- MCP Tasks pour operations longues
- Elicitation serveur-vers-client (la clarification passe par les retours de tools)
- Push Git depuis MCP
- Generation Pipeline depuis MCP

---

## 2. Decisions d'architecture

| Decision | Choix | Justification |
|---|---|---|
| Transport V1 | `stdio` | Simplicite, pas de configuration reseau, VS Code le lance comme un process local |
| Stateful/Stateless | N/A (`stdio` est inherement stateful par session) | — |
| Host | `Host.CreateApplicationBuilder` + `WithStdioServerTransport()` | Pattern recommande par le SDK pour les serveurs non-web |
| DI | Reutilise `AddApplication()` + `AddInfrastructure()` existants | Le serveur MCP a acces a la meme logique que l'API |
| Registration tools | `WithTools<T>()` explicite par classe | Controle precis, pas de scan assembly |
| Logs | stderr uniquement via `LogToStandardErrorThreshold` | Obligatoire pour `stdio` — stdout est reserve au protocole JSON-RPC |
| Draft storage | `ConcurrentDictionary<string, ProjectCreationDraft>` en memoire | Suffisant pour V1 `stdio` mono-session |
| Import preview storage | `ConcurrentDictionary<string, ImportPreview>` en memoire | Meme raison |
| Modele canonique d'import | `ImportedProjectDefinition` dans `Application/Imports/` | Reutilisable par l'API HTTP et le frontend plus tard |

---

## 3. Structure cible du code

```text
src/
├── Mcp/
│   └── InfraFlowSculptor.Mcp/
│       ├── InfraFlowSculptor.Mcp.csproj
│       ├── Program.cs
│       ├── Tools/
│       │   ├── DiscoveryTools.cs
│       │   ├── ProjectDraftTools.cs
│       │   ├── ProjectCreationTools.cs
│       │   ├── BicepGenerationTools.cs
│       │   └── IacImportTools.cs
│       ├── Resources/
│       │   └── ProjectResources.cs
│       ├── Prompts/
│       │   └── ProjectCreationPrompts.cs
│       └── DependencyInjection/
│           └── McpServiceRegistration.cs
└── Api/
    ├── InfraFlowSculptor.Application/
    │   └── Imports/
    │       ├── Models/
    │       │   ├── ImportedProjectDefinition.cs
    │       │   ├── ImportedResourceDefinition.cs
    │       │   ├── ImportGap.cs
    │       │   └── ImportPreview.cs
    │       ├── Services/
    │       │   ├── IProjectDraftService.cs
    │       │   ├── ProjectDraftService.cs
    │       │   ├── IImportPreviewService.cs
    │       │   └── ImportPreviewService.cs
    │       └── Adapters/
    │           ├── ISourceImportAdapter.cs
    │           └── ArmTemplateImportAdapter.cs
    ├── InfraFlowSculptor.Contracts/
    │   └── Imports/
    │       ├── ProjectCreationDraft.cs
    │       ├── DraftClarificationQuestion.cs
    │       ├── DraftResourceIntent.cs
    │       ├── ImportPreviewResponse.cs
    │       └── ImportGapResponse.cs
    └── InfraFlowSculptor.Infrastructure/
        └── Imports/
            └── ArmTemplateParser.cs

tests/
└── InfraFlowSculptor.Mcp.Tests/
    ├── InfraFlowSculptor.Mcp.Tests.csproj
    ├── Tools/
    │   ├── DiscoveryToolsTests.cs
    │   ├── ProjectDraftToolsTests.cs
    │   ├── ProjectCreationToolsTests.cs
    │   ├── BicepGenerationToolsTests.cs
    │   └── IacImportToolsTests.cs
    └── Adapters/
        └── ArmTemplateImportAdapterTests.cs
```

### References de projet

```text
InfraFlowSculptor.Mcp
  → InfraFlowSculptor.Application
  → InfraFlowSculptor.Infrastructure
  → InfraFlowSculptor.Contracts
  → InfraFlowSculptor.GenerationCore
```

### Packages NuGet requis (Mcp)

- `ModelContextProtocol` (dernier stable)
- `Microsoft.Extensions.Hosting`

---

## 4. Phases d'implementation

### Phase 0 — Scaffolding projet (estimee la plus petite)

**Objectif :** le serveur MCP demarre et repond `initialize` depuis VS Code.

| Etape | Description | Fichiers |
|---|---|---|
| 0.1 | Creer `src/Mcp/InfraFlowSculptor.Mcp/InfraFlowSculptor.Mcp.csproj` | csproj |
| 0.2 | Ecrire `Program.cs` minimal avec `AddMcpServer()` + `WithStdioServerTransport()` | Program.cs |
| 0.3 | Enregistrer `AddApplication()` + `AddInfrastructure()` dans le host DI | Program.cs, McpServiceRegistration.cs |
| 0.4 | Ajouter le projet a `InfraFlowSculptor.slnx` | slnx |
| 0.5 | Creer `.vscode/mcp.json` avec entry `stdio` | .vscode/mcp.json |
| 0.6 | Optionnel : ajouter le projet MCP comme resource dans Aspire AppHost | AppHost/Program.cs |
| 0.7 | Creer `tests/InfraFlowSculptor.Mcp.Tests/` | csproj, GlobalUsings.cs |
| 0.8 | Valider : `dotnet build`, VS Code detecte le serveur MCP, `initialize` repond | — |

### Phase 1 — Tools de discovery (read-only, zero side-effect)

**Objectif :** un agent peut decouvrir les topologies et resource types supportes.

| Etape | Description | Tool |
|---|---|---|
| 1.1 | Implementer `DiscoveryTools.list_repository_topologies` | `list_repository_topologies` |
| 1.2 | Implementer `DiscoveryTools.list_supported_resource_types` | `list_supported_resource_types` |
| 1.3 | Tests unitaires pour les deux tools | — |
| 1.4 | Valider dans VS Code : l'agent voit et appelle les tools | — |

### Phase 2 — Creation conversationnelle de projet

**Objectif :** un utilisateur peut dire "cree-moi un projet X" et obtenir une creation guidee.

| Etape | Description | Tool |
|---|---|---|
| 2.1 | Definir les contrats dans `Contracts/Imports/` : `ProjectCreationDraft`, `DraftClarificationQuestion`, `DraftResourceIntent` | — |
| 2.2 | Creer `IProjectDraftService` + `ProjectDraftService` dans `Application/Imports/` | — |
| 2.3 | Implementer `ProjectDraftTools.draft_project_from_prompt` | `draft_project_from_prompt` |
| 2.4 | Implementer `ProjectDraftTools.validate_project_draft` | `validate_project_draft` |
| 2.5 | Implementer `ProjectCreationTools.create_project_from_draft` (orchestre `CreateProjectWithSetupCommand` via MediatR) | `create_project_from_draft` |
| 2.6 | Implementer `ProjectCreationPrompts` pour le prompt MCP de clarification | prompt `project_creation_guide` |
| 2.7 | Tests unitaires : draft complet, draft incomplet, validation, creation, refus | — |
| 2.8 | Test E2E dans VS Code : scenario complet prompt → clarification → creation | — |

### Phase 3 — Generation Bicep

**Objectif :** un agent peut generer le Bicep d'un projet existant.

| Etape | Description | Tool |
|---|---|---|
| 3.1 | Implementer `BicepGenerationTools.generate_project_bicep` (appelle `GenerateProjectBicepCommand` existant) | `generate_project_bicep` |
| 3.2 | Implementer `ProjectResources` : resource `ifs://projects/{projectId}/summary` | resource |
| 3.3 | Tests unitaires | — |
| 3.4 | Test E2E : creer un projet via MCP, generer son Bicep, lire le resume | — |

### Phase 4 — Import IaC (ARM JSON)

**Objectif :** un utilisateur peut fournir un ARM template et obtenir un projet IFS equivalent.

| Etape | Description | Tool |
|---|---|---|
| 4.1 | Definir `ImportedProjectDefinition`, `ImportedResourceDefinition`, `ImportGap` dans `Application/Imports/Models/` | — |
| 4.2 | Definir `ISourceImportAdapter` et implementer `ArmTemplateImportAdapter` | — |
| 4.3 | Implementer `ArmTemplateParser` dans `Infrastructure/Imports/` | — |
| 4.4 | Implementer `IImportPreviewService` + `ImportPreviewService` | — |
| 4.5 | Implementer `IacImportTools.preview_iac_import` | `preview_iac_import` |
| 4.6 | Implementer `IacImportTools.apply_import_preview` (orchestre creation projet + resources) | `apply_import_preview` |
| 4.7 | Tests unitaires : parsing ARM, normalisation, gaps, preview, apply | — |
| 4.8 | Golden tests avec 2-3 ARM templates representatifs | — |

---

## 5. Contrats des tools — Discovery

### `list_repository_topologies`

**Description affichee a l'agent :** Lists the repository layout topologies supported by Infra Flow Sculptor, with a description of each option and its implications for repository structure.

**Categorie :** read-only, aucun parametre.

**Input :**

```json
{}
```

Aucun parametre requis.

**Output :**

```json
{
  "topologies": [
    {
      "id": "AllInOne",
      "label": "All-in-One (Mono Repo)",
      "description": "One single repository contains infrastructure code, application code, and all pipelines. Requires exactly 1 project-level repository.",
      "requiredRepositoryCount": 1,
      "repositoryContentKinds": [["Infrastructure", "Application"]]
    },
    {
      "id": "SplitInfraCode",
      "label": "Split Infra / Code",
      "description": "Two repositories: one for infrastructure (Bicep + infra pipelines), one for application code (app pipelines). Requires exactly 2 project-level repositories.",
      "requiredRepositoryCount": 2,
      "repositoryContentKinds": [["Infrastructure"], ["Application"]]
    },
    {
      "id": "MultiRepo",
      "label": "Multi-Repo",
      "description": "Repositories are declared per infrastructure configuration, not at project level. The project itself owns no repository. Each config can have its own layout mode.",
      "requiredRepositoryCount": 0,
      "repositoryContentKinds": []
    }
  ]
}
```

---

### `list_supported_resource_types`

**Description affichee a l'agent :** Lists all Azure resource types supported by Infra Flow Sculptor, including their identifiers, ARM provider types, and the minimal information needed to add them to a project.

**Categorie :** read-only, aucun parametre.

**Input :**

```json
{}
```

**Output :**

```json
{
  "resourceTypes": [
    {
      "id": "KeyVault",
      "armType": "Microsoft.KeyVault/vaults",
      "label": "Azure Key Vault",
      "category": "Security",
      "requiredFields": ["name"],
      "optionalFields": ["skuName", "enableSoftDelete", "enablePurgeProtection"],
      "supportedLinks": ["UserAssignedIdentity"],
      "hasEnvironmentSettings": true
    },
    {
      "id": "StorageAccount",
      "armType": "Microsoft.Storage/storageAccounts",
      "label": "Azure Storage Account",
      "category": "Storage",
      "requiredFields": ["name"],
      "optionalFields": ["skuName", "kind", "accessTier"],
      "supportedLinks": ["UserAssignedIdentity"],
      "hasEnvironmentSettings": true,
      "subResources": ["BlobContainer", "StorageQueue", "StorageTable"]
    },
    {
      "id": "AppServicePlan",
      "armType": "Microsoft.Web/serverfarms",
      "label": "App Service Plan",
      "category": "Compute",
      "requiredFields": ["name", "osType"],
      "optionalFields": ["skuName", "skuCapacity"],
      "supportedLinks": [],
      "hasEnvironmentSettings": true
    },
    {
      "id": "WebApp",
      "armType": "Microsoft.Web/sites",
      "label": "Web App",
      "category": "Compute",
      "requiredFields": ["name", "appServicePlanId"],
      "optionalFields": ["dockerfilePath", "sourceCodePath", "buildCommand"],
      "supportedLinks": ["AppServicePlan", "UserAssignedIdentity", "ContainerRegistry"],
      "hasEnvironmentSettings": true
    },
    {
      "id": "FunctionApp",
      "armType": "Microsoft.Web/sites/functionapp",
      "label": "Function App",
      "category": "Compute",
      "requiredFields": ["name", "appServicePlanId"],
      "optionalFields": ["dockerfilePath", "sourceCodePath", "buildCommand"],
      "supportedLinks": ["AppServicePlan", "UserAssignedIdentity", "ContainerRegistry"],
      "hasEnvironmentSettings": true
    },
    {
      "id": "ContainerAppEnvironment",
      "armType": "Microsoft.App/managedEnvironments",
      "label": "Container App Environment",
      "category": "Compute",
      "requiredFields": ["name"],
      "optionalFields": ["logAnalyticsWorkspaceId"],
      "supportedLinks": ["LogAnalyticsWorkspace"],
      "hasEnvironmentSettings": true
    },
    {
      "id": "ContainerApp",
      "armType": "Microsoft.App/containerApps",
      "label": "Container App",
      "category": "Compute",
      "requiredFields": ["name", "containerAppEnvironmentId"],
      "optionalFields": ["dockerImageName", "dockerfilePath", "applicationName"],
      "supportedLinks": ["ContainerAppEnvironment", "UserAssignedIdentity", "ContainerRegistry"],
      "hasEnvironmentSettings": true
    },
    {
      "id": "RedisCache",
      "armType": "Microsoft.Cache/Redis",
      "label": "Azure Cache for Redis",
      "category": "Data",
      "requiredFields": ["name"],
      "optionalFields": ["skuName", "skuFamily", "skuCapacity"],
      "supportedLinks": ["UserAssignedIdentity"],
      "hasEnvironmentSettings": true
    },
    {
      "id": "AppConfiguration",
      "armType": "Microsoft.AppConfiguration/configurationStores",
      "label": "App Configuration",
      "category": "Configuration",
      "requiredFields": ["name"],
      "optionalFields": ["skuName"],
      "supportedLinks": ["UserAssignedIdentity"],
      "hasEnvironmentSettings": true,
      "subResources": ["AppConfigurationKey"]
    },
    {
      "id": "LogAnalyticsWorkspace",
      "armType": "Microsoft.OperationalInsights/workspaces",
      "label": "Log Analytics Workspace",
      "category": "Monitoring",
      "requiredFields": ["name"],
      "optionalFields": ["retentionInDays"],
      "supportedLinks": [],
      "hasEnvironmentSettings": true
    },
    {
      "id": "ApplicationInsights",
      "armType": "Microsoft.Insights/components",
      "label": "Application Insights",
      "category": "Monitoring",
      "requiredFields": ["name", "logAnalyticsWorkspaceId"],
      "optionalFields": [],
      "supportedLinks": ["LogAnalyticsWorkspace"],
      "hasEnvironmentSettings": true
    },
    {
      "id": "CosmosDb",
      "armType": "Microsoft.DocumentDB/databaseAccounts",
      "label": "Azure Cosmos DB",
      "category": "Data",
      "requiredFields": ["name"],
      "optionalFields": [],
      "supportedLinks": ["UserAssignedIdentity"],
      "hasEnvironmentSettings": true
    },
    {
      "id": "SqlServer",
      "armType": "Microsoft.Sql/servers",
      "label": "SQL Server",
      "category": "Data",
      "requiredFields": ["name"],
      "optionalFields": [],
      "supportedLinks": ["UserAssignedIdentity"],
      "hasEnvironmentSettings": true
    },
    {
      "id": "SqlDatabase",
      "armType": "Microsoft.Sql/servers/databases",
      "label": "SQL Database",
      "category": "Data",
      "requiredFields": ["name", "sqlServerId"],
      "optionalFields": ["skuName"],
      "supportedLinks": ["SqlServer"],
      "hasEnvironmentSettings": true
    },
    {
      "id": "ServiceBusNamespace",
      "armType": "Microsoft.ServiceBus/namespaces",
      "label": "Service Bus Namespace",
      "category": "Messaging",
      "requiredFields": ["name"],
      "optionalFields": ["skuName"],
      "supportedLinks": ["UserAssignedIdentity"],
      "hasEnvironmentSettings": true,
      "subResources": ["Queue", "TopicSubscription"]
    },
    {
      "id": "ContainerRegistry",
      "armType": "Microsoft.ContainerRegistry/registries",
      "label": "Container Registry",
      "category": "Compute",
      "requiredFields": ["name"],
      "optionalFields": ["skuName"],
      "supportedLinks": ["UserAssignedIdentity"],
      "hasEnvironmentSettings": true
    },
    {
      "id": "EventHubNamespace",
      "armType": "Microsoft.EventHub/namespaces",
      "label": "Event Hubs Namespace",
      "category": "Messaging",
      "requiredFields": ["name"],
      "optionalFields": ["skuName"],
      "supportedLinks": ["UserAssignedIdentity"],
      "hasEnvironmentSettings": true,
      "subResources": ["EventHub", "ConsumerGroup"]
    },
    {
      "id": "UserAssignedIdentity",
      "armType": "Microsoft.ManagedIdentity/userAssignedIdentities",
      "label": "User Assigned Managed Identity",
      "category": "Security",
      "requiredFields": ["name"],
      "optionalFields": [],
      "supportedLinks": [],
      "hasEnvironmentSettings": false
    }
  ]
}
```

---

## 6. Contrats des tools — Creation conversationnelle

### `draft_project_from_prompt`

**Description affichee a l'agent :** Transforms a free-form user prompt into a structured project creation draft. This tool never creates anything — it only extracts intent, identifies what is missing, and returns clarification questions when required. The draft must be validated and completed before calling create_project_from_draft.

**Categorie :** read-only, extraction d'intent.

**Input :**

```json
{
  "userPrompt": "string (required) — The raw user request in natural language."
}
```

**Exemples d'input :**

```json
{ "userPrompt": "Crée-moi un projet RetailApi en mono repo avec un Key Vault le moins cher possible" }
```

```json
{ "userPrompt": "Crée un projet avec un Key Vault" }
```

**Output — cas complet (status `ready_to_create`) :**

```json
{
  "draftId": "draft_a1b2c3d4",
  "status": "ready_to_create",
  "missingFields": [],
  "clarificationQuestions": [],
  "inferredIntent": {
    "projectName": "RetailApi",
    "description": null,
    "layoutPreset": "AllInOne",
    "environments": [
      {
        "name": "Development",
        "shortName": "dev",
        "prefix": "",
        "suffix": "-dev",
        "location": "westeurope",
        "subscriptionId": "00000000-0000-0000-0000-000000000000",
        "order": 1,
        "requiresApproval": false
      }
    ],
    "resources": [
      {
        "resourceType": "KeyVault",
        "name": "RetailApi-kv",
        "pricingHint": "lowest-cost",
        "environmentSettings": {}
      }
    ],
    "repositories": [
      {
        "alias": "main",
        "contentKinds": ["Infrastructure", "Application"],
        "providerType": null,
        "repositoryUrl": null,
        "defaultBranch": null
      }
    ],
    "agentPoolName": null,
    "pricingIntent": "lowest-cost"
  },
  "warnings": [
    "Subscription ID defaulted to empty — the user should set real subscription IDs before deployment.",
    "Environment Location defaulted to westeurope — the user can change this later."
  ]
}
```

**Output — cas incomplet (status `requires_clarification`) :**

```json
{
  "draftId": "draft_e5f6g7h8",
  "status": "requires_clarification",
  "missingFields": ["layoutPreset"],
  "clarificationQuestions": [
    {
      "field": "layoutPreset",
      "message": "How should the project repositories be organized?",
      "options": [
        {
          "value": "AllInOne",
          "label": "All-in-One (Mono Repo)",
          "description": "One single repository for infrastructure and application code."
        },
        {
          "value": "SplitInfraCode",
          "label": "Split Infra / Code",
          "description": "Two repositories: one for infrastructure, one for application code."
        },
        {
          "value": "MultiRepo",
          "label": "Multi-Repo",
          "description": "Repositories declared per configuration, not at project level."
        }
      ]
    }
  ],
  "inferredIntent": {
    "projectName": null,
    "description": null,
    "layoutPreset": null,
    "environments": null,
    "resources": [
      {
        "resourceType": "KeyVault",
        "name": null,
        "pricingHint": null,
        "environmentSettings": {}
      }
    ],
    "repositories": null,
    "agentPoolName": null,
    "pricingIntent": null
  },
  "warnings": [
    "No project name was provided — the user must specify one."
  ]
}
```

**Regles metier du tool :**

1. Si `layoutPreset` n'est pas explicitement mentionne ou clairement inferable → `status = requires_clarification`
2. Si `projectName` est absent → ajouter aux `missingFields`
3. Un environment par defaut ("Development") peut etre infere mais avec un `warning`
4. Les `subscriptionId` et `location` sont defauts a `00000000-...` et `westeurope` avec warning
5. Les resource names peuvent etre inferes depuis le project name + conventions d'abbreviation
6. `pricingHint` capture l'intention exprimee ("le moins cher", "premium", etc.) mais ne controle pas directement le SKU — il sert de signal pour les suggestions

---

### `validate_project_draft`

**Description affichee a l'agent :** Validates a project creation draft and returns any remaining missing fields, errors, or warnings. Call this after enriching a draft with clarification answers, and before calling create_project_from_draft.

**Categorie :** read-only, validation.

**Input :**

```json
{
  "draftId": "string (required) — The ID returned by draft_project_from_prompt.",
  "overrides": {
    "projectName": "string (optional) — Override or set the project name.",
    "layoutPreset": "string (optional) — Override or set the layout preset.",
    "description": "string (optional) — Override or set the description.",
    "environments": "(optional) — Override the full environments array.",
    "repositories": "(optional) — Override the full repositories array.",
    "agentPoolName": "string (optional) — Set a custom agent pool name."
  }
}
```

**Exemple d'input (reponse a une clarification) :**

```json
{
  "draftId": "draft_e5f6g7h8",
  "overrides": {
    "projectName": "PaymentService",
    "layoutPreset": "AllInOne"
  }
}
```

**Output — valide :**

```json
{
  "draftId": "draft_e5f6g7h8",
  "status": "ready_to_create",
  "missingFields": [],
  "errors": [],
  "warnings": [
    "Subscription ID still defaulted to empty on environment 'Development'."
  ],
  "updatedIntent": {
    "projectName": "PaymentService",
    "description": null,
    "layoutPreset": "AllInOne",
    "environments": [
      {
        "name": "Development",
        "shortName": "dev",
        "prefix": "",
        "suffix": "-dev",
        "location": "westeurope",
        "subscriptionId": "00000000-0000-0000-0000-000000000000",
        "order": 1,
        "requiresApproval": false
      }
    ],
    "resources": [
      {
        "resourceType": "KeyVault",
        "name": "PaymentService-kv",
        "pricingHint": null,
        "environmentSettings": {}
      }
    ],
    "repositories": [
      {
        "alias": "main",
        "contentKinds": ["Infrastructure", "Application"],
        "providerType": null,
        "repositoryUrl": null,
        "defaultBranch": null
      }
    ],
    "agentPoolName": null,
    "pricingIntent": null
  }
}
```

**Output — erreurs de validation :**

```json
{
  "draftId": "draft_e5f6g7h8",
  "status": "requires_clarification",
  "missingFields": ["projectName"],
  "errors": [
    {
      "field": "environments",
      "message": "At least one environment is required."
    }
  ],
  "warnings": [],
  "updatedIntent": { "..." }
}
```

**Regles metier du tool :**

1. Applique les `overrides` au draft en memoire puis revalide
2. Le `status` repasse a `ready_to_create` uniquement si tous les `missingFields` sont resolus et aucune `error`
3. Les `warnings` n'empechent pas la creation — ils signalent des valeurs par defaut a verifier
4. Si le `draftId` n'existe pas → erreur `draft_not_found`
5. Le nom de projet est valide selon les memes regles que `CreateProjectWithSetupCommand` (3-80 caracteres)

---

### `create_project_from_draft`

**Description affichee a l'agent :** Creates a project in Infra Flow Sculptor from a completed and validated draft. This tool is a mutator: it persists the project, its environments, repository slots, and optionally its initial resources. The draft must have status 'ready_to_create' — incomplete drafts are rejected.

**Categorie :** mutateur.

**Input :**

```json
{
  "draftId": "string (required) — The ID of a draft with status 'ready_to_create'."
}
```

**Output — succes :**

```json
{
  "status": "created",
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "projectName": "PaymentService",
  "layoutPreset": "AllInOne",
  "environmentCount": 1,
  "repositoryCount": 1,
  "createdResources": [
    {
      "resourceType": "KeyVault",
      "resourceId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "name": "PaymentService-kv",
      "resourceGroupId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    }
  ],
  "nextSuggestedActions": [
    "Configure environment subscription IDs and locations",
    "Add repository connection details",
    "Add more resources to the project",
    "Generate Bicep with generate_project_bicep"
  ]
}
```

**Output — refus (draft incomplet) :**

```json
{
  "status": "rejected",
  "reason": "Draft 'draft_e5f6g7h8' is not ready to create. Missing fields: layoutPreset. Call validate_project_draft first.",
  "draftId": "draft_e5f6g7h8",
  "missingFields": ["layoutPreset"]
}
```

**Regles metier du tool :**

1. Refuse categoriquement si `status != ready_to_create`
2. Orchestre en interne :
   - Appel a `CreateProjectWithSetupCommand` via MediatR pour le projet + layout + envs + repos
   - Pour chaque resource dans le draft : appel a la commande `Create<ResourceType>Command` correspondante (via un mapping `resourceType → command factory`)
3. Les resources sont creees dans un resource group par defaut nomme d'apres la premiere config
4. Si une resource demande une dependance (ex: `WebApp` → `AppServicePlan`), le tool cree la dependance d'abord si elle est dans le draft, sinon retourne une erreur
5. Supprime le draft de la memoire apres creation reussie

---

## 7. Contrats des tools — Generation

### `generate_project_bicep`

**Description affichee a l'agent :** Generates Bicep infrastructure-as-code files for a project. Returns a summary of generated files organized by configuration. The project must exist and contain at least one resource.

**Categorie :** mutateur (genere et stocke des fichiers).

**Input :**

```json
{
  "projectId": "string (required) — The project ID (GUID format)."
}
```

**Output — succes :**

```json
{
  "status": "generated",
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "projectName": "PaymentService",
  "generatedAt": "2026-04-28T14:30:00Z",
  "commonFiles": [
    "Common/types.bicep",
    "Common/functions.bicep",
    "Common/modules/keyVault.bicep"
  ],
  "configFiles": {
    "main-config": [
      "main-config/main.bicep",
      "main-config/main.bicepparam",
      "main-config/modules/keyVault.bicep"
    ]
  },
  "totalFileCount": 6,
  "summary": "Generated 6 Bicep files for 1 configuration with 1 resource (KeyVault)."
}
```

**Output — projet vide :**

```json
{
  "status": "error",
  "errorCode": "no_resources",
  "message": "Project 'PaymentService' has no resources to generate Bicep for. Add resources first."
}
```

**Regles metier du tool :**

1. Appelle `GenerateProjectBicepCommand` existant via MediatR
2. Ne retourne pas le contenu des fichiers directement — retourne la liste des chemins et un resume
3. Le contenu des fichiers est accessible via la resource `ifs://projects/{projectId}/bicep/{filePath}` si besoin
4. Les erreurs de generation (missing dependencies, invalid config) sont propagees avec `errorCode` et `message`

---

## 8. Contrats des tools — Import IaC

### `preview_iac_import`

**Description affichee a l'agent :** Analyzes an Infrastructure-as-Code source (ARM JSON template for V1) and produces a structured import preview showing what resources would be created in Infra Flow Sculptor, what gaps exist, and what information is missing. This tool never creates anything — it only produces a preview.

**Categorie :** read-only.

**Input :**

```json
{
  "sourceFormat": "string (required) — The format of the source. V1 supports: 'arm-json'.",
  "sourceContent": "string (required) — The raw content of the IaC source file."
}
```

**Exemple d'input :**

```json
{
  "sourceFormat": "arm-json",
  "sourceContent": "{ \"$schema\": \"https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#\", \"contentVersion\": \"1.0.0.0\", \"resources\": [ { \"type\": \"Microsoft.KeyVault/vaults\", \"apiVersion\": \"2023-07-01\", \"name\": \"myKeyVault\", \"location\": \"[resourceGroup().location]\", \"properties\": { \"sku\": { \"family\": \"A\", \"name\": \"standard\" }, \"tenantId\": \"[subscription().tenantId]\" } } ] }"
}
```

**Output :**

```json
{
  "previewId": "preview_x1y2z3",
  "sourceFormat": "arm-json",
  "parsedResourceCount": 1,
  "mappedResources": [
    {
      "sourceResourceType": "Microsoft.KeyVault/vaults",
      "sourceName": "myKeyVault",
      "targetResourceType": "KeyVault",
      "targetName": "myKeyVault",
      "mappingConfidence": "high",
      "extractedProperties": {
        "skuName": "standard"
      },
      "unmappedProperties": [
        "tenantId"
      ]
    }
  ],
  "gaps": [
    {
      "severity": "info",
      "category": "unmapped_property",
      "message": "Property 'tenantId' on KeyVault is auto-managed by Infra Flow Sculptor and will not be imported.",
      "sourceResourceName": "myKeyVault"
    }
  ],
  "unsupportedResources": [],
  "suggestedProjectStructure": {
    "environmentsNeeded": true,
    "layoutPresetSuggestion": "AllInOne",
    "resourceGroupSuggestion": "default-rg"
  },
  "summary": "1 resource parsed, 1 mapped to Infra Flow Sculptor types, 0 unsupported. 1 informational gap."
}
```

**Output — format non supporte :**

```json
{
  "previewId": null,
  "sourceFormat": "terraform-hcl",
  "parsedResourceCount": 0,
  "mappedResources": [],
  "gaps": [],
  "unsupportedResources": [],
  "suggestedProjectStructure": null,
  "summary": "Source format 'terraform-hcl' is not supported in V1. Supported formats: arm-json."
}
```

**Regles metier du tool :**

1. Parse le contenu sans jamais executer de code tiers
2. Utilise le mapping `ArmTypes.*` → `AzureResourceTypes.*` pour la correspondance
3. Chaque resource ARM est classee en `mapped` (correspondance trouvee) ou `unsupported` (pas de type IFS)
4. Les proprietes extractibles sont normalisees vers le modele IFS
5. Le `previewId` est stocke en memoire pour que `apply_import_preview` puisse le reutiliser
6. La `mappingConfidence` est `high` si le type ARM a une correspondance directe, `medium` si heuristique, `low` si ambigu

---

### `apply_import_preview`

**Description affichee a l'agent :** Applies a validated import preview to create or enrich an Infra Flow Sculptor project. Can either create a new project from the imported resources or add them to an existing project. Requires a preview ID from a previous call to preview_iac_import.

**Categorie :** mutateur.

**Input :**

```json
{
  "previewId": "string (required) — The preview ID returned by preview_iac_import.",
  "targetMode": "string (required) — 'new_project' to create a new project, 'existing_project' to add to an existing one.",
  "projectName": "string (required if targetMode='new_project') — Name of the project to create.",
  "layoutPreset": "string (required if targetMode='new_project') — Layout preset for the new project.",
  "projectId": "string (required if targetMode='existing_project') — Target project ID.",
  "environments": [
    {
      "name": "string",
      "shortName": "string",
      "prefix": "string",
      "suffix": "string",
      "location": "string",
      "subscriptionId": "string (GUID)",
      "order": 0,
      "requiresApproval": false
    }
  ],
  "resourceFilter": "string[] (optional) — List of source resource names to include. If null, all mapped resources are imported."
}
```

**Output — succes :**

```json
{
  "status": "applied",
  "previewId": "preview_x1y2z3",
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "projectName": "ImportedProject",
  "createdResources": [
    {
      "sourceResourceName": "myKeyVault",
      "targetResourceType": "KeyVault",
      "targetResourceId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "targetName": "myKeyVault"
    }
  ],
  "skippedResources": [],
  "errors": [],
  "nextSuggestedActions": [
    "Review imported resource configurations",
    "Configure environment-specific settings",
    "Generate Bicep with generate_project_bicep"
  ]
}
```

**Output — preview introuvable :**

```json
{
  "status": "error",
  "errorCode": "preview_not_found",
  "message": "Import preview 'preview_xyz' not found. It may have expired. Run preview_iac_import again."
}
```

**Regles metier du tool :**

1. Refuse si `previewId` n'existe pas en memoire
2. En mode `new_project` : cree le projet + config + resource groups + resources via les commandes existantes
3. En mode `existing_project` : ajoute les resources a un projet existant en reutilisant ses configs et envs
4. Les resources `unsupported` du preview sont exclues automatiquement
5. Le `resourceFilter` permet d'importer un sous-ensemble des resources parsees
6. Supprime le preview de la memoire apres application reussie

---

## 9. Resources MCP

### `ifs://projects/{projectId}/summary`

**Description :** Structured summary of a project including its configurations, resource counts, environments, and layout.

**Output :**

```json
{
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "PaymentService",
  "description": null,
  "layoutPreset": "AllInOne",
  "environmentCount": 1,
  "environments": [
    { "name": "Development", "shortName": "dev", "location": "westeurope" }
  ],
  "configurationCount": 1,
  "configurations": [
    {
      "configId": "...",
      "name": "main-config",
      "resourceGroupCount": 1,
      "resourceCount": 2,
      "resourceTypes": ["KeyVault", "StorageAccount"]
    }
  ],
  "totalResourceCount": 2,
  "repositoryCount": 1,
  "hasGeneratedBicep": true,
  "lastBicepGenerationDate": "2026-04-28T14:30:00Z"
}
```

### `ifs://imports/{previewId}`

**Description :** Full content of a stored import preview. Allows the agent to re-examine a preview without calling the tool again.

**Output :** Same shape as the `preview_iac_import` output.

---

## 10. Prompts MCP

### `project_creation_guide`

**Description :** A guided prompt that helps the agent structure questions to the user when creating a new project. Provides the standard questions about project name, layout, environments, and resources.

**Contenu du prompt :**

```text
You are helping a user create a new infrastructure project in Infra Flow Sculptor.
Follow this structured approach:

1. ASK for the project name if not already provided.

2. ASK for the repository layout topology. The options are:
   - AllInOne: One repository for infrastructure and application code
   - SplitInfraCode: Separate repositories for infrastructure and application
   - MultiRepo: Repositories declared per configuration

3. ASK which Azure resources the project needs. Supported types include:
   KeyVault, StorageAccount, AppServicePlan, WebApp, FunctionApp,
   ContainerAppEnvironment, ContainerApp, RedisCache, AppConfiguration,
   LogAnalyticsWorkspace, ApplicationInsights, CosmosDb, SqlServer,
   SqlDatabase, ServiceBusNamespace, ContainerRegistry, EventHubNamespace,
   UserAssignedIdentity.

4. ASK about environments. At minimum, one environment is needed.
   Typical setups: Development, Staging, Production.
   Each environment needs: name, short name, location, subscription ID.

5. Once you have all required information, use draft_project_from_prompt
   to create the draft, then validate_project_draft, then create_project_from_draft.

IMPORTANT: Never guess the repository layout topology.
If the user hasn't specified it, always ask.
```

---

## 11. Modele canonique d'import

Le modele canonique sert de couche intermediaire entre les formats sources et le modele IFS.

### `ImportedProjectDefinition`

```csharp
public sealed record ImportedProjectDefinition
{
    public required string SourceFormat { get; init; }
    public required IReadOnlyList<ImportedResourceDefinition> Resources { get; init; }
    public IReadOnlyList<ImportedDependency> Dependencies { get; init; } = [];
    public IReadOnlyDictionary<string, string> Metadata { get; init; }
        = new Dictionary<string, string>();
}
```

### `ImportedResourceDefinition`

```csharp
public sealed record ImportedResourceDefinition
{
    public required string SourceType { get; init; }
    public required string SourceName { get; init; }
    public string? MappedResourceType { get; init; }
    public string? MappedName { get; init; }
    public MappingConfidence Confidence { get; init; } = MappingConfidence.High;
    public IReadOnlyDictionary<string, object?> ExtractedProperties { get; init; }
        = new Dictionary<string, object?>();
    public IReadOnlyList<string> UnmappedProperties { get; init; } = [];
}

public enum MappingConfidence { High, Medium, Low }
```

### `ImportedDependency`

```csharp
public sealed record ImportedDependency(
    string FromResourceName,
    string ToResourceName,
    string DependencyType);
```

### `ImportGap`

```csharp
public sealed record ImportGap
{
    public required ImportGapSeverity Severity { get; init; }
    public required string Category { get; init; }
    public required string Message { get; init; }
    public string? SourceResourceName { get; init; }
}

public enum ImportGapSeverity { Info, Warning, Error }
```

---

## 12. Dependances par phase

```text
Phase 0 — Scaffolding
  Pas de dependance fonctionnelle.
  Depend uniquement de l'infrastructure existante (Application, Infrastructure, Contracts DI).

Phase 1 — Discovery
  Depend de Phase 0.
  Pas de nouveau code dans Application/Infrastructure.
  Utilise AzureResourceTypes et LayoutPresetEnum directement.

Phase 2 — Creation conversationnelle
  Depend de Phase 1.
  Cree : Contracts/Imports/*, Application/Imports/Services/IProjectDraftService.
  Utilise : CreateProjectWithSetupCommand, Create<Resource>Command existants.
  Impact Application : ajout de IProjectDraftService et registration DI.

Phase 3 — Generation Bicep
  Depend de Phase 0 (Phase 2 est un plus mais pas obligatoire).
  Utilise : GenerateProjectBicepCommand existant, GetProjectQuery existant.
  Pas de nouveau code dans Application hors du tool MCP lui-meme.

Phase 4 — Import IaC
  Depend de Phase 2 (reutilise le draft service pour la creation post-import).
  Cree : Application/Imports/Models/*, Application/Imports/Adapters/*,
         Infrastructure/Imports/ArmTemplateParser.
  Impact Application : ajout de IImportPreviewService, ISourceImportAdapter, registration DI.
```

---

## 13. Strategie de tests

### Par phase

| Phase | Type de test | Cible |
|---|---|---|
| 0 | Smoke test | Le serveur demarre, repond `initialize`, s'arrete proprement |
| 1 | Unit | `DiscoveryTools` retournent les bonnes topologies et types |
| 2 | Unit | `ProjectDraftService` : intent extraction, missing field detection, validation |
| 2 | Unit | `ProjectDraftTools` : mapping input → service, formatting output |
| 2 | Unit | `ProjectCreationTools` : refus draft incomplet, orchestration creation |
| 3 | Unit | `BicepGenerationTools` : mapping vers commande existante, formatting resultat |
| 4 | Unit | `ArmTemplateImportAdapter` : parsing ARM → `ImportedProjectDefinition` |
| 4 | Unit | `ImportPreviewService` : gap detection, confidence scoring |
| 4 | Golden | ARM templates representatifs → previews attendus |

### Contrainte transport

Ne pas utiliser `WithStdioServerTransport()` dans les tests unitaires. Tester les tools comme des classes C# classiques injectees avec des mocks des services sous-jacents.

### Projet de tests

`tests/InfraFlowSculptor.Mcp.Tests/` avec references vers :
- `InfraFlowSculptor.Mcp`
- `InfraFlowSculptor.Application`
- `InfraFlowSculptor.Contracts`
- xUnit + FluentAssertions + NSubstitute

---

## 14. Checklist de validation V1

```text
[ ] Phase 0 : le serveur demarre en stdio et repond initialize
[ ] Phase 0 : .vscode/mcp.json fonctionne dans VS Code
[ ] Phase 0 : dotnet build .\InfraFlowSculptor.slnx passe
[ ] Phase 1 : list_repository_topologies retourne les 3 presets
[ ] Phase 1 : list_supported_resource_types retourne les 18 types
[ ] Phase 2 : draft_project_from_prompt detecte les champs manquants
[ ] Phase 2 : validate_project_draft applique les overrides et revalide
[ ] Phase 2 : create_project_from_draft refuse un draft incomplet
[ ] Phase 2 : create_project_from_draft cree un projet complet
[ ] Phase 2 : scenario bout en bout : prompt → clarification → creation
[ ] Phase 3 : generate_project_bicep genere pour un projet avec resources
[ ] Phase 3 : resource ifs://projects/{id}/summary retourne un resume
[ ] Phase 4 : preview_iac_import parse un ARM JSON avec Key Vault
[ ] Phase 4 : preview_iac_import detecte les resources non supportees
[ ] Phase 4 : apply_import_preview cree un projet depuis un preview
[ ] Phase 4 : golden tests sur 2-3 ARM templates representatifs
[ ] Transversal : tous les tests unitaires passent
[ ] Transversal : dotnet test .\InfraFlowSculptor.slnx passe
[ ] Transversal : aucun log sur stdout en mode stdio
```

---

## Ordre de lecture recommande apres ce plan

1. [mcp-integration.md](mcp-integration.md) — les concepts MCP si pas encore lu
2. `.github/skills/mcp-dotnet-server/SKILL.md` — les regles de conception detaillees
3. [overview.md](overview.md) — l'architecture generale du projet
4. [cqrs-patterns.md](cqrs-patterns.md) — le pattern CQRS pour comprendre les commandes reutilisees
