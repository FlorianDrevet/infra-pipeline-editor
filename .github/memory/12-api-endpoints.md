# API Endpoints Reference

> 27 controllers — 18 Azure resource types + InfrastructureConfig + Project + ResourceGroup + BicepGeneration + PipelineGeneration + NamingTemplate + AppConfigurationKey + AppSetting + RoleAssignment

## Core Aggregates

| Group | Method | Route | Command/Query |
|---|---|---|---|
| `/infra-config` | GET | `` | `ListMyInfrastructureConfigsQuery` |
| `/infra-config` | GET | `/{id:guid}` | `GetInfrastructureConfigQuery` |
| `/infra-config` | POST | `` | `CreateInfrastructureConfigCommand` |
| `/infra-config` | DELETE | `/{id:guid}` | `DeleteInfrastructureConfigCommand` |
| `/infra-config` | POST/PUT/DELETE | `/{id:guid}/members/{...}` | Member CRUD |
| `/infra-configs` | PUT | `/{id:guid}/agent-pool` | `SetAgentPoolCommand` [2026-04-04] |
| `/projects` | GET | `` | `ListMyProjectsQuery` |
| `/projects` | GET | `/{id:guid}` | `GetProjectQuery` |
| `/projects` | POST | `` | `CreateProjectCommand` |
| `/projects` | DELETE | `/{id:guid}` | `DeleteProjectCommand` |
| `/projects` | POST | `/validate-recent` | `ValidateRecentItemsQuery` |
| `/projects` | PUT/DELETE | `/{id:guid}/git-config` | Git config CRUD |
| `/projects` | POST | `/{id:guid}/git-config/test` | `TestGitConnectionCommand` |
| `/projects` | GET | `/{id:guid}/git-config/branches` | `ListGitBranchesQuery` |
| `/projects` | GET | `/{id:guid}/generate-bicep/download` | `DownloadProjectBicepCommand` |
| `/projects` | POST | `/{id:guid}/generate-pipeline` | `GenerateProjectPipelineCommand` |
| `/projects` | GET | `/{id:guid}/generate-pipeline/download` | `DownloadProjectPipelineCommand` |
| `/projects` | GET | `/{id:guid}/generate-pipeline/files/{*filePath}` | `GetProjectPipelineFileContentQuery` |
| `/projects` | POST | `/{id:guid}/push-pipeline-to-git` | `PushProjectPipelineToGitCommand` |
| `/resource-group` | GET/POST/PUT/DELETE | `/{id:guid}` | ResourceGroup CRUD (DELETE added [2026-04-04]) |

## Azure Resource CRUD (18 types — standard GET/POST/PUT/DELETE pattern)

| Group | Extra endpoints |
|---|---|
| `/keyvault` | Standard CRUD |
| `/redis-cache` | Standard CRUD |
| `/storage-accounts` | + blob-containers (POST/DELETE/PUT), queues (POST/DELETE), tables (POST/DELETE) |
| `/app-service-plan` | Standard CRUD |
| `/web-app` | Standard CRUD |
| `/function-app` | Standard CRUD |
| `/user-assigned-identity` | + `/{id}/granted-role-assignments` (GET), `/{id}/unlink-resource` (POST) |
| `/app-configuration` | Standard CRUD |
| `/container-app-environment` | Standard CRUD |
| `/container-app` | Standard CRUD |
| `/log-analytics-workspace` | + `/{id}/dependents` (GET) |
| `/application-insights` | Standard CRUD |
| `/cosmos-db` | Standard CRUD |
| `/sql-server` | Standard CRUD |
| `/sql-database` | Standard CRUD |
| `/service-bus-namespace` | + queues (POST/DELETE), topic-subscriptions (POST/DELETE) |
| `/container-registry` | + `/check-acr-pull-access` (GET) |
| `/event-hubs` | + event-hubs (POST/DELETE), consumer-groups (POST/DELETE) |

## Sub-resource / Cross-cutting Controllers

| Group | Method | Route | Command/Query |
|---|---|---|---|
| `/azure-resources/{resourceId}/role-assignments` | GET | `` | `ListRoleAssignmentsQuery` |
| `/azure-resources/{resourceId}/role-assignments` | GET | `/available-role-definitions` | `ListAvailableRoleDefinitionsQuery` |
| `/azure-resources/{resourceId}/role-assignments` | POST | `` | `AddRoleAssignmentCommand` |
| `/azure-resources/{resourceId}/role-assignments` | DELETE | `/{roleAssignmentId}` | `RemoveRoleAssignmentCommand` |
| `/azure-resources/{resourceId}/role-assignments` | GET | `/{roleAssignmentId}/impact-analysis` | `AnalyzeRoleAssignmentImpactQuery` |
| `/azure-resources/{resourceId}/role-assignments` | PUT | `/{roleAssignmentId}/identity` | `UpdateRoleAssignmentIdentityCommand` |
| `/azure-resources/{resourceId}/app-settings` | GET/POST/PUT/DELETE | various | AppSetting CRUD |
| `/azure-resources/{resourceId}/available-outputs` | GET | `` | `GetAvailableOutputsQuery` |
| `/azure-resources/{resourceId}/check-keyvault-access` | GET | `/{keyVaultId}` | `CheckKeyVaultAccessQuery` |
| `/azure-resources/{resourceId}/configuration-keys` | GET/POST/DELETE | various | AppConfigurationKey CRUD |
| `/azure-resources/{id}/assigned-identity` | PUT | `` | `AssignIdentityToResourceCommand` |
| `/azure-resources/{id}/assigned-identity` | DELETE | `` | `UnassignIdentityFromResourceCommand` |
| `/infra-config/{id}/naming` | PUT | `/default` | `SetDefaultNamingTemplateCommand` |
| `/infra-config/{id}/naming` | PUT | `/resources/{resourceType}` | `SetResourceNamingTemplateCommand` |
| `/infra-config/{id}/naming` | DELETE | `/resources/{resourceType}` | `RemoveResourceNamingTemplateCommand` |

## Generation Controllers

| Group | Method | Route | Command/Query |
|---|---|---|---|
| `/generate-bicep` | POST | `` | `GenerateBicepCommand` |
| `/generate-bicep` | GET | `/{configId}/download` | `DownloadBicepCommand` |
| `/generate-bicep` | GET | `/{configId}/files/{*filePath}` | `GetBicepFileContentQuery` |
| `/generate-bicep` | POST | `/{configId}/push-to-git` | `PushBicepToGitCommand` |
| `/generate-pipeline` | POST | `` | `GeneratePipelineCommand` |
| `/generate-pipeline` | GET | `/{configId}/download` | `DownloadPipelineCommand` |
| `/generate-pipeline` | GET | `/{configId}/files/{*filePath}` | `GetPipelineFileContentQuery` |
| `/generate-pipeline` | POST | `/{configId}/push-to-git` | `PushPipelineToGitCommand` |
