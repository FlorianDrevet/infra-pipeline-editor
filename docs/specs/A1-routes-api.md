# A1 — Routes de l'API principale

> Statut : v0, **générée** le 2026-10-03 depuis `src/Api/InfraFlowSculptor.Api/Controllers`
> (229 routes). Résumé = texte `WithSummary` du code, vide quand il n'y en a pas.
> Toutes les routes exigent une authentification (Entra ID ou PAT IFS, voir
> [02](02-acces-et-droits.md)). Les contrôles de rôle sont faits par les handlers.
>
> Remarques :
> - Aucune route `GET` pour la configuration du Private Endpoint (D02, [11](11-reseau-et-exposition.md)).
> - Le serveur MCP a sa propre surface ([18](18-serveur-mcp.md)). Il n'existe plus d'API de génération
>   séparée : `src/BicepGenerators`, cité par `CLAUDE.md`, n'existe pas dans le dépôt ; la génération
>   tourne dans l'API principale.

### AppConfiguration

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/app-configuration/{id:guid}` | Get an App Configuration |
| POST | `/app-configuration` | Create an App Configuration |
| PUT | `/app-configuration/{id:guid}` | Update an App Configuration |
| DELETE | `/app-configuration/{id:guid}` | Delete an App Configuration |

### AppConfigurationKey

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/azure-resources/{appConfigurationId:guid}/configuration-keys` |  |
| POST | `/azure-resources/{appConfigurationId:guid}/configuration-keys` |  |
| DELETE | `/azure-resources/{appConfigurationId:guid}/configuration-keys/{configurationKeyId:guid}` |  |

### AppServicePlan

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/app-service-plan/{id:guid}` | Get an App Service Plan |
| POST | `/app-service-plan` | Create an App Service Plan |
| PUT | `/app-service-plan/{id:guid}` | Update an App Service Plan |
| DELETE | `/app-service-plan/{id:guid}` | Delete an App Service Plan |

### AppSetting

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/azure-resources/{resourceId:guid}/app-settings` |  |
| POST | `/azure-resources/{resourceId:guid}/app-settings` |  |
| DELETE | `/azure-resources/{resourceId:guid}/app-settings/{appSettingId:guid}` |  |
| PUT | `/azure-resources/{resourceId:guid}/app-settings/{appSettingId:guid}` |  |
| GET | `/azure-resources/{resourceId:guid}/available-outputs` |  |
| GET | `/azure-resources/{resourceId:guid}/check-keyvault-access/{keyVaultId:guid}` |  |

### ApplicationInsights

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/application-insights/{id:guid}` | Get an Application Insights resource |
| POST | `/application-insights` | Create an Application Insights resource |
| PUT | `/application-insights/{id:guid}` | Update an Application Insights resource |
| DELETE | `/application-insights/{id:guid}` | Delete an Application Insights resource |

### BicepGeneration

| Verbe | Route | Résumé |
|---|---|---|
| POST | `/generate-bicep` |  |
| GET | `/generate-bicep/{configId:guid}/download` |  |
| GET | `/generate-bicep/{configId:guid}/files/{*filePath}` |  |
| POST | `/generate-bicep/{configId:guid}/push-to-git` | Push generated Bicep files to Git |

### BootstrapGeneration

| Verbe | Route | Résumé |
|---|---|---|
| POST | `/generate-bootstrap` |  |
| GET | `/generate-bootstrap/{configId:guid}/download` |  |
| GET | `/generate-bootstrap/{configId:guid}/files/{*filePath}` |  |
| POST | `/generate-bootstrap/{configId:guid}/push-to-git` | Push generated bootstrap files to Git |

### Catalog

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/catalogs/test-frameworks` |  |

### ContainerApp

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/container-app/{id:guid}` | Get a Container App |
| POST | `/container-app` | Create a Container App |
| PUT | `/container-app/{id:guid}` | Update a Container App |
| DELETE | `/container-app/{id:guid}` | Delete a Container App |

### ContainerAppEnvironment

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/container-app-environment/{id:guid}` | Get a Container App Environment |
| POST | `/container-app-environment` | Create a Container App Environment |
| PUT | `/container-app-environment/{id:guid}` | Update a Container App Environment |
| DELETE | `/container-app-environment/{id:guid}` | Delete a Container App Environment |

### ContainerRegistry

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/container-registry/{id:guid}` | Get a Container Registry |
| POST | `/container-registry` | Create a Container Registry |
| PUT | `/container-registry/{id:guid}` | Update a Container Registry |
| DELETE | `/container-registry/{id:guid}` | Delete a Container Registry |
| GET | `/azure-resources/{resourceId:guid}/check-acr-pull-access/{containerRegistryId:guid}` | Check ACR Pull access |

### CosmosDb

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/cosmos-db/{id:guid}` | Get a Cosmos DB account |
| POST | `/cosmos-db` | Create a Cosmos DB account |
| PUT | `/cosmos-db/{id:guid}` | Update a Cosmos DB account |
| DELETE | `/cosmos-db/{id:guid}` | Delete a Cosmos DB account |

### CustomDomain

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/azure-resources/{resourceId:guid}/custom-domains` |  |
| POST | `/azure-resources/{resourceId:guid}/custom-domains` |  |
| DELETE | `/azure-resources/{resourceId:guid}/custom-domains/{customDomainId:guid}` |  |
| POST | `/azure-resources/{resourceId:guid}/custom-domains/{customDomainId:guid}/validate-dns` |  |
| GET | `/azure-resources/{resourceId:guid}/custom-domains/{customDomainId:guid}/dns-instructions` |  |

### DocumentIntelligence

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/document-intelligence/{id:guid}` | Get a Document Intelligence resource |
| POST | `/document-intelligence` | Create a Document Intelligence resource |
| PUT | `/document-intelligence/{id:guid}` | Update a Document Intelligence resource |
| DELETE | `/document-intelligence/{id:guid}` | Delete a Document Intelligence resource |

### EventHubNamespace

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/event-hubs/{id:guid}` | Get an Event Hub Namespace |
| POST | `/event-hubs` | Create an Event Hub Namespace |
| PUT | `/event-hubs/{id:guid}` | Update an Event Hub Namespace |
| DELETE | `/event-hubs/{id:guid}` | Delete an Event Hub Namespace |
| POST | `/event-hubs/{id:guid}/event-hubs` | Add an event hub |
| DELETE | `/event-hubs/{id:guid}/event-hubs/{eventHubId:guid}` | Remove an event hub |
| POST | `/event-hubs/{id:guid}/consumer-groups` | Add a consumer group |
| DELETE | `/event-hubs/{id:guid}/consumer-groups/{consumerGroupId:guid}` | Remove a consumer group |

### FunctionApp

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/function-app/{id:guid}` | Get a Function App |
| POST | `/function-app` | Create a Function App |
| PUT | `/function-app/{id:guid}` | Update a Function App |
| DELETE | `/function-app/{id:guid}` | Delete a Function App |

### Git

| Verbe | Route | Résumé |
|---|---|---|
| POST | `/git/verify-connection` | Verify a Git repository connection |

### Import

| Verbe | Route | Résumé |
|---|---|---|
| POST | `/imports/preview` |  |
| POST | `/imports/apply` |  |

### InfrastructureConfig

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/infra-config` | List my Infrastructure Configurations |
| GET | `/infra-config/{id:guid}` | Get an Infrastructure Configuration |
| GET | `/infra-config/{id:guid}/resource-groups` | List resource groups for a configuration |
| POST | `/infra-config` | Create an Infrastructure Configuration |
| PUT | `/infra-config/{id:guid}/inheritance` | Toggle project-level inheritance |
| PUT | `/infra-config/{id:guid}/tags` | Set configuration-level tags |
| DELETE | `/infra-config/{id:guid}` | Delete an infrastructure configuration |
| GET | `/infra-config/{id:guid}/cross-config-references` | List cross-config references |
| POST | `/infra-config/{id:guid}/cross-config-references` | Add a cross-config resource reference |
| DELETE | `/infra-config/{id:guid}/cross-config-references/{refId:guid}` | Remove a cross-config resource reference |
| GET | `/infra-config/{id:guid}/incoming-cross-config-references` | List incoming cross-config references |
| GET | `/infra-config/{id:guid}/diagnostics` | Get configuration diagnostics |

### KeyVault

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/keyvault/{id:guid}` | Get a Key Vault |
| POST | `/keyvault` | Create a Key Vault |
| PUT | `/keyvault/{id:guid}` | Update a Key Vault |
| DELETE | `/keyvault/{id:guid}` | Delete a Key Vault |

### LogAnalyticsWorkspace

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/log-analytics-workspace/{id:guid}` | Get a Log Analytics Workspace |
| POST | `/log-analytics-workspace` | Create a Log Analytics Workspace |
| PUT | `/log-analytics-workspace/{id:guid}` | Update a Log Analytics Workspace |
| DELETE | `/log-analytics-workspace/{id:guid}` | Delete a Log Analytics Workspace |

### NamingTemplate

| Verbe | Route | Résumé |
|---|---|---|
| PUT | `/infra-config/{id:guid}/naming/default` | Set the default naming template |
| PUT | `/infra-config/{id:guid}/naming/resources/{resourceType}` | Set a per-resource-type naming template |
| DELETE | `/infra-config/{id:guid}/naming/resources/{resourceType}` | Remove a per-resource-type naming template |
| PUT | `/infra-config/{id:guid}/naming/abbreviations/{resourceType}` | Set a per-resource-type abbreviation override |
| DELETE | `/infra-config/{id:guid}/naming/abbreviations/{resourceType}` | Remove a per-resource-type abbreviation override |
| POST | `/naming/check-availability/{resourceType}` | Check Azure resource name availability across all environments |

### NetworkingProfile

| Verbe | Route | Résumé |
|---|---|---|
| PUT | `/infra-config/{infraConfigId:guid}/resources/{resourceId:guid}/privatization` | Toggle resource privatization |
| PUT | `/infra-config/{infraConfigId:guid}/resources/{resourceId:guid}/private-endpoint-config` | Configure resource private endpoint |
| DELETE | `/infra-config/{infraConfigId:guid}/resources/{resourceId:guid}/private-endpoint-config` | Remove resource private endpoint |

### PersonalAccessToken

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/personal-access-tokens` | List personal access tokens |
| POST | `/personal-access-tokens` | Create a personal access token |
| DELETE | `/personal-access-tokens/{id:guid}` | Revoke a personal access token |

### PipelineGeneration

| Verbe | Route | Résumé |
|---|---|---|
| POST | `/generate-pipeline` |  |
| GET | `/generate-pipeline/{configId:guid}/download` |  |
| GET | `/generate-pipeline/{configId:guid}/files/{*filePath}` |  |
| POST | `/generate-pipeline/{configId:guid}/push-to-git` | Push generated pipeline files to Git |

### PipelineOptionDetection

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/azure-resources/{resourceId:guid}/detect-pipeline-options` |  |

### Project

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/projects` | List my Projects |
| GET | `/projects/{id:guid}` | Get a Project |
| POST | `/projects` | Create a Project |
| POST | `/projects/with-setup` | Create a Project with setup |
| GET | `/projects/{id:guid}/configs` | List configurations for a project |
| PUT | `/projects/{id:guid}/tags` | Set project-level tags |
| DELETE | `/projects/{id:guid}` | Delete a project |
| POST | `/projects/validate-recent` | Validate recently viewed items |
| POST | `/projects/{projectId:guid}/git-config/test` | Test Git repository connection |
| GET | `/projects/{projectId:guid}/git-config/branches` | List Git repository branches |
| GET | `/projects/{projectId:guid}/git-config/code-branches` | List code repository branches |
| GET | `/projects/{projectId:guid}/git-config/code-files` | Search files in code repository |
| GET | `/projects/{projectId:guid}/git-config/code-directories` | List directories in code repository |
| GET | `/projects/{id:guid}/resources` | List all resources across configurations |
| PUT | `/projects/{projectId:guid}/agent-pool` | Set or clear the agent pool for pipeline generation |
| PUT | `/projects/{projectId:guid}/configs/{configId:guid}/layout-mode` | Set or clear the per-configuration layout mode |
| POST | `/projects/{projectId:guid}/configs/{configId:guid}/repositories` | Declare a Git repository on an InfrastructureConfig (MultiRepo only) |
| PUT | `/projects/{projectId:guid}/configs/{configId:guid}/repositories/{repositoryId:guid}` | Update an InfraConfig repository |
| DELETE | `/projects/{projectId:guid}/configs/{configId:guid}/repositories/{repositoryId:guid}` | Delete an InfraConfig repository |
| GET | `/projects/{projectId:guid}/pipeline-variable-groups` | List project-level pipeline variable groups |
| POST | `/projects/{projectId:guid}/pipeline-variable-groups` | Add a project-level pipeline variable group |
| DELETE | `/projects/{projectId:guid}/pipeline-variable-groups/{groupId:guid}` | Remove a project-level pipeline variable group |

### ProjectEnvironment

| Verbe | Route | Résumé |
|---|---|---|
| POST | `/projects/{id:guid}/environments` | Add an environment to a project |
| PUT | `/projects/{id:guid}/environments/{envId:guid}` | Update a project environment |
| DELETE | `/projects/{id:guid}/environments/{envId:guid}` | Remove a project environment |

### ProjectGeneration

| Verbe | Route | Résumé |
|---|---|---|
| POST | `/projects/{projectId:guid}/generate-bicep` | Generate Bicep files for the entire project (mono-repo) |
| GET | `/projects/{projectId:guid}/generate-bicep/download` | Download generated Bicep files for a project |
| GET | `/projects/{projectId:guid}/generate-bicep/files/{*filePath}` | Get generated Bicep file content for a project |
| POST | `/projects/{projectId:guid}/push-to-git` | Push project-level Bicep files to Git (mono-repo) |
| POST | `/projects/{projectId:guid}/generate-pipeline` | Generate pipeline files for the entire project (mono-repo) |
| GET | `/projects/{projectId:guid}/generate-pipeline/download` | Download generated pipeline files for a project |
| GET | `/projects/{projectId:guid}/generate-pipeline/files/{*filePath}` | Get generated pipeline file content for a project |
| POST | `/projects/{projectId:guid}/push-pipeline-to-git` | Push project-level pipeline files to Git (mono-repo) |
| POST | `/projects/{projectId:guid}/generate-bootstrap-pipeline` | Generate the Azure DevOps bootstrap pipeline for a project |
| GET | `/projects/{projectId:guid}/generate-bootstrap-pipeline/download` | Download the latest bootstrap pipeline as a ZIP archive |
| GET | `/projects/{projectId:guid}/generate-bootstrap-pipeline/files/{*filePath}` | Get generated bootstrap pipeline file content for a project |
| POST | `/projects/{projectId:guid}/push-bootstrap-pipeline-to-git` | Push the bootstrap pipeline file to Git (Azure DevOps) |
| POST | `/projects/{projectId:guid}/push-generated-artifacts-to-git` | Push generated project artifacts to Git in a single commit (mono-repo) |
| POST | `/projects/{projectId:guid}/push-split-infra-code-artifacts-to-git` | Push project artifacts to one or two repositories (SplitInfraCode) |
| POST | `/projects/{projectId:guid}/push-multi-repo-artifacts-to-git` | Push project artifacts to configuration-owned repositories (MultiRepo) |
| GET | `/projects/{projectId:guid}/latest-generation` | Get latest generation file listing |

### ProjectMember

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/projects/users` | List registered users |
| POST | `/projects/{id:guid}/members` | Add a member to a project |
| PUT | `/projects/{id:guid}/members/{userId:guid}` | Update a project member's role |
| DELETE | `/projects/{id:guid}/members/{userId:guid}` | Remove a member from a project |

### ProjectNaming

| Verbe | Route | Résumé |
|---|---|---|
| PUT | `/projects/{id:guid}/naming/default` | Set the project default naming template |
| PUT | `/projects/{id:guid}/naming/resources/{resourceType}` | Set a per-resource-type naming template |
| DELETE | `/projects/{id:guid}/naming/resources/{resourceType}` | Remove a per-resource-type naming template |
| PUT | `/projects/{id:guid}/naming/abbreviations/{resourceType}` | Set a per-resource-type abbreviation override |
| DELETE | `/projects/{id:guid}/naming/abbreviations/{resourceType}` | Remove a per-resource-type abbreviation override |

### ProjectRepository

| Verbe | Route | Résumé |
|---|---|---|
| POST | `/projects/{projectId:guid}/repositories` | Add a project-level Git repository declaration |
| PUT | `/projects/{projectId:guid}/repositories/{repoId:guid}` | Update a project-level Git repository declaration |
| POST | `/projects/{projectId:guid}/repositories/verify` | Verify a project repository connection |
| POST | `/projects/{projectId:guid}/repositories/{repoId:guid}/verify` | Verify an existing project repository connection |
| DELETE | `/projects/{projectId:guid}/repositories/{repoId:guid}` | Remove a project-level Git repository declaration |
| POST | `/projects/{projectId:guid}/repositories/{repoId:guid}/test-connection` | Test a specific project repository connection |
| PUT | `/projects/{projectId:guid}/repositories/{repoId:guid}/git-pat` | Set a repository-scoped Git personal access token |
| PUT | `/projects/{projectId:guid}/layout-preset` | Set the project layout preset |

### RedisCache

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/redis-cache/{id:guid}` | Get a Redis Cache |
| POST | `/redis-cache` | Create a Redis Cache |
| PUT | `/redis-cache/{id:guid}` | Update a Redis Cache |
| DELETE | `/redis-cache/{id:guid}` | Delete a Redis Cache |

### ResourceGroup

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/resource-group/{id:guid}` | Get a Resource Group |
| GET | `/resource-group/{id:guid}/resources` | List resources in a Resource Group |
| POST | `/resource-group` | Create a Resource Group |
| PUT | `/resource-group/{id:guid}` | Update a Resource Group |
| DELETE | `/resource-group/{id:guid}` | Delete a Resource Group |

### RoleAssignment

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/azure-resources/{resourceId:guid}/role-assignments` |  |
| GET | `/azure-resources/{resourceId:guid}/role-assignments/available-role-definitions` |  |
| POST | `/azure-resources/{resourceId:guid}/role-assignments` |  |
| DELETE | `/azure-resources/{resourceId:guid}/role-assignments/{roleAssignmentId:guid}` |  |
| GET | `/azure-resources/{resourceId:guid}/role-assignments/{roleAssignmentId:guid}/impact-analysis` |  |
| PUT | `/azure-resources/{resourceId:guid}/role-assignments/{roleAssignmentId:guid}/identity` |  |
| PUT | `/azure-resources/{resourceId:guid}/assigned-identity` |  |
| DELETE | `/azure-resources/{resourceId:guid}/assigned-identity` |  |

### SecureParameterMapping

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/azure-resources/{resourceId:guid}/secure-parameter-mappings` |  |
| PUT | `/azure-resources/{resourceId:guid}/secure-parameter-mappings` |  |

### ServiceBusNamespace

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/service-bus/{id:guid}` | Get a Service Bus Namespace |
| POST | `/service-bus` | Create a Service Bus Namespace |
| PUT | `/service-bus/{id:guid}` | Update a Service Bus Namespace |
| DELETE | `/service-bus/{id:guid}` | Delete a Service Bus Namespace |
| POST | `/service-bus/{id:guid}/queues` | Add a queue |
| DELETE | `/service-bus/{id:guid}/queues/{queueId:guid}` | Remove a queue |
| POST | `/service-bus/{id:guid}/topic-subscriptions` | Add a topic subscription |
| DELETE | `/service-bus/{id:guid}/topic-subscriptions/{subscriptionId:guid}` | Remove a topic subscription |

### SqlDatabase

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/sql-database/{id:guid}` | Get a SQL Database |
| POST | `/sql-database` | Create a SQL Database |
| PUT | `/sql-database/{id:guid}` | Update a SQL Database |
| DELETE | `/sql-database/{id:guid}` | Delete a SQL Database |

### SqlServer

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/sql-server/{id:guid}` | Get a SQL Server |
| POST | `/sql-server` | Create a SQL Server |
| PUT | `/sql-server/{id:guid}` | Update a SQL Server |
| DELETE | `/sql-server/{id:guid}` | Delete a SQL Server |

### StorageAccount

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/storage-accounts/{id:guid}` | Get a Storage Account |
| POST | `/storage-accounts` | Create a Storage Account |
| PUT | `/storage-accounts/{id:guid}` | Update a Storage Account |
| DELETE | `/storage-accounts/{id:guid}` | Delete a Storage Account |
| POST | `/storage-accounts/{id:guid}/blob-containers` | Add a Blob Container |
| DELETE | `/storage-accounts/{id:guid}/blob-containers/{containerId:guid}` | Remove a Blob Container |
| PUT | `/storage-accounts/{id:guid}/blob-containers/{containerId:guid}` | Update Blob Container Public Access |
| POST | `/storage-accounts/{id:guid}/queues` | Add a Storage Queue |
| DELETE | `/storage-accounts/{id:guid}/queues/{queueId:guid}` | Remove a Storage Queue |
| POST | `/storage-accounts/{id:guid}/tables` | Add a Storage Table |
| DELETE | `/storage-accounts/{id:guid}/tables/{tableId:guid}` | Remove a Storage Table |

### UserAssignedIdentity

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/user-assigned-identity/{id:guid}` | Get a User Assigned Identity |
| POST | `/user-assigned-identity` | Create a User Assigned Identity |
| PUT | `/user-assigned-identity/{id:guid}` | Update a User Assigned Identity |
| DELETE | `/user-assigned-identity/{id:guid}` | Delete a User Assigned Identity |
| GET | `/user-assigned-identity/{id:guid}/granted-role-assignments` | List role assignments granted through this identity |
| POST | `/user-assigned-identity/{id:guid}/unlink-resource` | Unlink a resource from this identity |

### VirtualNetwork

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/virtual-network/{id:guid}` | Get a Virtual Network |
| POST | `/virtual-network` | Create a Virtual Network |
| PUT | `/virtual-network/{id:guid}` | Update a Virtual Network |
| DELETE | `/virtual-network/{id:guid}` | Delete a Virtual Network |
| POST | `/virtual-network/{id:guid}/subnets` | Add a subnet |
| PUT | `/virtual-network/{id:guid}/subnets/{subnetId:guid}` | Update a subnet |
| DELETE | `/virtual-network/{id:guid}/subnets/{subnetId:guid}` | Remove a subnet |

### WebApp

| Verbe | Route | Résumé |
|---|---|---|
| GET | `/web-app/{id:guid}` | Get a Web App |
| POST | `/web-app` | Create a Web App |
| PUT | `/web-app/{id:guid}` | Update a Web App |
| DELETE | `/web-app/{id:guid}` | Delete a Web App |
