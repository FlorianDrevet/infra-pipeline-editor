# API Endpoints Reference

> 29 controllers — 18 Azure resource types + InfrastructureConfig + Project + ResourceGroup + BicepGeneration + PipelineGeneration + NamingTemplate + AppConfigurationKey + AppSetting + RoleAssignment + SecureParameterMapping + CustomDomain

## Core Aggregates

| Group | Method | Route | Command/Query |
|---|---|---|---|
| `/infra-config` | GET | `` | `ListMyInfrastructureConfigsQuery` |
| `/infra-config` | GET | `/{id:guid}` | `GetInfrastructureConfigQuery` |
| `/infra-config` | POST | `` | `CreateInfrastructureConfigCommand` |
| `/infra-config` | DELETE | `/{id:guid}` | `DeleteInfrastructureConfigCommand` |
| `/infra-config` | POST/PUT/DELETE | `/{id:guid}/members/{...}` | Member CRUD |
| `/projects` | GET | `` | `ListMyProjectsQuery` |
| `/projects` | GET | `/{id:guid}` | `GetProjectQuery` |
| `/projects` | POST | `` | `CreateProjectCommand` |
| `/projects` | DELETE | `/{id:guid}` | `DeleteProjectCommand` |
| `/projects` | PUT | `/{id:guid}/agent-pool` | `SetAgentPoolCommand` (moved from /infra-configs) [2026-04-04] |
| `/projects` | POST/PUT/DELETE | `/{id:guid}/repositories/{repoId?}` | Project repository CRUD |
| `/projects` | PUT | `/{id:guid}/layout-preset` | `SetProjectLayoutPresetCommand` |
| `/projects` | PUT/POST/PUT/DELETE | `/{id:guid}/configs/{configId:guid}/{layout-mode|repositories/...}` | Config layout mode + config repository CRUD |
| `/projects` | POST | `/validate-recent` | `ValidateRecentItemsQuery` |
| `/projects` | POST | `/{id:guid}/git-config/test` | `TestGitConnectionCommand` (resolver-backed, V3) |
| `/projects` | GET | `/{id:guid}/git-config/branches` | `ListGitBranchesQuery` (resolver-backed, V3) |
| `/projects` | POST | `/{id:guid}/generate-bicep` | `GenerateProjectBicepCommand` |
| `/projects` | GET | `/{id:guid}/generate-bicep/download` | `DownloadProjectBicepCommand` |
| `/projects` | GET | `/{id:guid}/generate-bicep/files/{*filePath}` | `GetProjectBicepFileContentQuery` |
| `/projects` | POST | `/{id:guid}/push-to-git` | `PushProjectBicepToGitCommand` |
| `/projects` | POST | `/{id:guid}/generate-pipeline` | `GenerateProjectPipelineCommand` |
| `/projects` | GET | `/{id:guid}/generate-pipeline/download` | `DownloadProjectPipelineCommand` |
| `/projects` | GET | `/{id:guid}/generate-pipeline/files/{*filePath}` | `GetProjectPipelineFileContentQuery` |
| `/projects` | POST | `/{id:guid}/push-pipeline-to-git` | `PushProjectPipelineToGitCommand` |
| `/projects` | POST | `/{id:guid}/generate-bootstrap-pipeline` | `GenerateProjectBootstrapPipelineCommand` |
| `/projects` | GET | `/{id:guid}/generate-bootstrap-pipeline/download` | `DownloadProjectBootstrapPipelineCommand` |
| `/projects` | GET | `/{id:guid}/generate-bootstrap-pipeline/files/{*filePath}` | `GetProjectBootstrapPipelineFileContentQuery` |
| `/projects` | POST | `/{id:guid}/push-bootstrap-pipeline-to-git` | `PushProjectBootstrapPipelineToGitCommand` |
| `/projects` | POST | `/{id:guid}/push-generated-artifacts-to-git` | `PushProjectGeneratedArtifactsToGitCommand` |
| `/projects` | POST | `/{id:guid}/push-multi-repo-artifacts-to-git` | `PushProjectArtifactsToMultiRepoCommand` (SplitInfraCode infra-only, code-only, or dual push; validator requires at least one target; always 200 with per-repo `RepoPushResult`) |
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
| `/azure-resources/{resourceId}/secure-parameter-mappings` | GET | `` | `GetSecureParameterMappingsQuery` |
| `/azure-resources/{resourceId}/secure-parameter-mappings` | PUT | `` | `SetSecureParameterMappingCommand` |
| `/azure-resources/{resourceId}/custom-domains` | GET | `` | `ListCustomDomainsQuery` |
| `/azure-resources/{resourceId}/custom-domains` | POST | `` | `AddCustomDomainCommand` |
| `/azure-resources/{resourceId}/custom-domains` | DELETE | `/{customDomainId}` | `RemoveCustomDomainCommand` |
| `/azure-resources/{resourceId}/app-settings` | GET/POST/PUT/DELETE | various | AppSetting CRUD |
| `/azure-resources/{resourceId}/available-outputs` | GET | `` | `GetAvailableOutputsQuery` |
| `/azure-resources/{resourceId}/check-keyvault-access` | GET | `/{keyVaultId}` | `CheckKeyVaultAccessQuery` |
| `/azure-resources/{resourceId}/configuration-keys` | GET/POST/DELETE | various | AppConfigurationKey CRUD |
| `/azure-resources/{id}/assigned-identity` | PUT | `` | `AssignIdentityToResourceCommand` |
| `/azure-resources/{id}/assigned-identity` | DELETE | `` | `UnassignIdentityFromResourceCommand` |
| `/infra-config/{id}/naming` | PUT | `/default` | `SetDefaultNamingTemplateCommand` |
| `/infra-config/{id}/naming` | PUT | `/resources/{resourceType}` | `SetResourceNamingTemplateCommand` |
| `/infra-config/{id}/naming` | DELETE | `/resources/{resourceType}` | `RemoveResourceNamingTemplateCommand` |
| `/infra-config/{id}/naming` | PUT | `/abbreviations/{resourceType}` | `SetResourceAbbreviationOverrideCommand` |
| `/infra-config/{id}/naming` | DELETE | `/abbreviations/{resourceType}` | `RemoveResourceAbbreviationOverrideCommand` |
| `/projects/{id}/naming` | PUT | `/templates/default` | `SetProjectDefaultNamingTemplateCommand` |
| `/projects/{id}/naming` | PUT | `/templates/{resourceType}` | `SetProjectResourceNamingTemplateCommand` |
| `/projects/{id}/naming` | DELETE | `/templates/{resourceType}` | `RemoveProjectResourceNamingTemplateCommand` |
| `/projects/{id}/naming` | PUT | `/abbreviations/{resourceType}` | `SetProjectResourceAbbreviationCommand` |
| `/projects/{id}/naming` | DELETE | `/abbreviations/{resourceType}` | `RemoveProjectResourceAbbreviationCommand` |

## Generation Controllers

| Group | Method | Route | Command/Query |
|---|---|---|---|
| `/naming` | POST | `/check-availability/{resourceType}` | `CheckResourceNameAvailabilityQuery` |
| `/generate-bicep` | POST | `` | `GenerateBicepCommand` |
| `/generate-bicep` | GET | `/{configId}/download` | `DownloadBicepCommand` |
| `/generate-bicep` | GET | `/{configId}/files/{*filePath}` | `GetBicepFileContentQuery` |
| `/generate-bicep` | POST | `/{configId}/push-to-git` | `PushBicepToGitCommand` |
| `/generate-pipeline` | POST | `` | `GeneratePipelineCommand` |
| `/generate-pipeline` | GET | `/{configId}/download` | `DownloadPipelineCommand` |
| `/generate-pipeline` | GET | `/{configId}/files/{*filePath}` | `GetPipelineFileContentQuery` |
| `/generate-pipeline` | POST | `/{configId}/push-to-git` | `PushPipelineToGitCommand` |
