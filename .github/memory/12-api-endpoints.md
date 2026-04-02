# API Endpoints Reference

| Group | Method | Route | Command/Query |
|---|---|---|---|
| `/infra-config` | GET | `` | `ListMyInfrastructureConfigsQuery` |
| `/infra-config` | GET | `/{id:guid}` | `GetInfrastructureConfigQuery` |
| `/infra-config` | POST | `` | `CreateInfrastructureConfigCommand` |
| `/infra-config` | DELETE | `/{id:guid}` | `DeleteInfrastructureConfigCommand` |
| `/infra-config` | POST | `/generate-bicep` | `GenerateBicepCommand` |
| `/infra-config` | POST/PUT/DELETE | `/{id:guid}/members/{...}` | Member CRUD |
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
| `/keyvault` | GET/POST/PUT/DELETE | `/{id:guid}` | Key Vault CRUD |
| `/resource-group` | GET/POST | `/{id:guid}` | Resource Group CRUD |
| `/redis-cache` | GET/POST/PUT/DELETE | `/{id:guid}` | Redis Cache CRUD |
| `/app-service-plan` | GET/POST/PUT/DELETE | `/{id:guid}` | App Service Plan CRUD |
| `/web-app` | GET/POST/PUT/DELETE | `/{id:guid}` | Web App CRUD |
| `/function-app` | GET/POST/PUT/DELETE | `/{id:guid}` | Function App CRUD |
| `/generate-bicep` | POST/GET | various | Bicep generation, download, file content, push-to-git |
| `/generate-pipeline` | POST/GET | various | Pipeline generation, download, file content, push-to-git |
| `/sql-server` | GET/POST/PUT/DELETE | `/{id:guid}` | SQL Server CRUD |
| `/sql-database` | GET/POST/PUT/DELETE | `/{id:guid}` | SQL Database CRUD |
| `/azure-resources` | GET | `/{id}/role-assignments/{raId}/impact-analysis` | `AnalyzeRoleAssignmentImpactQuery` |
| `/event-hubs` | 8 endpoints | various | EventHubNamespace CRUD + sub-resources |
