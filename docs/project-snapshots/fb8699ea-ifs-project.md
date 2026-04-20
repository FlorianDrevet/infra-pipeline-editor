# Project Snapshot — Infra Flow Sculptor

> **Project ID:** `fb8699ea-f568-4afb-864b-e82d2efd0905`
> **Generated:** 2026-04-20
> **Purpose:** Reference file for Copilot context — load this instead of querying the database.

---

## Project

| Field | Value |
|-------|-------|
| **Name** | Infra Flow Sculptor |
| **ID** | `fb8699ea-f568-4afb-864b-e82d2efd0905` |
| **DefaultNamingTemplate** | `{name}-{resourceAbbr}{suffix}` |
| **RepositoryMode** | MonoRepo |
| **AgentPoolName** | Default (self-hosted) |

### Git Configuration

| Field | Value |
|-------|-------|
| **Provider** | AzureDevOps |
| **RepositoryUrl** | `https://dev.azure.com/floriandrevet0332/Infra%20Flow%20Sculptor/_git/ifs` |
| **Owner** | `floriandrevet0332/Infra Flow Sculptor` |
| **RepositoryName** | `ifs` |
| **DefaultBranch** | `main` |
| **BasePath** | `infra` |
| **PipelineBasePath** | *(null — defaults to same as BasePath)* |

### Project-Level Naming Templates

| ResourceType | Template |
|-------------|----------|
| ResourceGroup | `{resourceAbbr}-{name}{suffix}` |
| StorageAccount | `{name}{resourceAbbr}{envShort}` |

---

## Environments

### Development

| Field | Value |
|-------|-------|
| **ID** | `16e70ad4-a8da-434f-80c5-3000a2bca95b` |
| **Name** | Development |
| **ShortName** | dev |
| **Prefix** | `dev-` |
| **Suffix** | `-dev` |
| **Location** | FranceCentral |
| **SubscriptionId** | `83d0b022-c10f-40e7-8eb9-72f37ae7e283` |
| **AzureResourceManagerConnection** | `ifs` |
| **RequiresApproval** | false |
| **Tags** | `environment: dev` |

---

## Infrastructure Configurations

### 1. Core

| Field | Value |
|-------|-------|
| **ID** | `4ce4be5e-e7d6-4084-94c8-77ff21be42ef` |
| **Name** | Core |
| **UseProjectNamingConventions** | true |
| **AppPipelineMode** | Isolated |

#### Resource Group: `ifs-core`

| ID | Name | Location |
|----|------|----------|
| `5c87b514-a237-4040-a047-af431df4aa46` | ifs-core | FranceCentral |

#### Resources in `ifs-core`

| Resource | Type | ID | Location |
|----------|------|----|----------|
| ifs | ContainerRegistry | `36ba74cb-c0a1-40a3-b224-e3be1c1b4b46` | FranceCentral |
| ifs | LogAnalyticsWorkspace | `83b21c4d-584c-4e2e-8362-3ddf6aee73c2` | FranceCentral |

##### ContainerRegistry — `ifs` (`36ba74cb`)

*No extra properties.*

| Env | Sku | AdminUserEnabled | PublicNetworkAccess | ZoneRedundancy |
|-----|-----|------------------|---------------------|----------------|
| Development | Basic | false | Enabled | false |

##### LogAnalyticsWorkspace — `ifs` (`83b21c4d`)

*No extra properties.*

| Env | Sku | RetentionInDays | DailyQuotaGb |
|-----|-----|-----------------|-------------|
| Development | Free | 30 | 2 |

---

### 2. Infra Flow Sculptor

| Field | Value |
|-------|-------|
| **ID** | `3afe7f4f-e113-4ba8-9d83-999dd7dc22b4` |
| **Name** | Infra Flow Sculptor |
| **UseProjectNamingConventions** | true |
| **AppPipelineMode** | Isolated |

#### Resource Group: `ifs`

| ID | Name | Location |
|----|------|----------|
| `8c99a6b4-94d1-4979-bfe3-e8ba22d711d9` | ifs | FranceCentral |

#### Resources in `ifs`

| Resource | Type | ID | Location | AssignedIdentity |
|----------|------|----|----------|------------------|
| ifs | KeyVault | `fc210d60-9d8a-4899-9f9e-07dced5871c5` | FranceCentral | — |
| ifs | StorageAccount | `efe669ac-71a9-4884-a6b1-cf16583bbf37` | FranceCentral | — |
| ifs | SqlServer | `9700666f-4771-46f9-aaab-74d9370eee59` | FranceCentral | — |
| ifs | SqlDatabase | `0ae0ab1e-a076-468d-a7af-41fd6b4d3d20` | FranceCentral | — |
| ifs | ApplicationInsights | `e8bdb228-bc60-4de6-9d88-4d549b5a64bb` | FranceCentral | — |
| ifs | ContainerAppEnvironment | `37cfd530-1f07-442a-849c-4c030bb147a4` | FranceCentral | — |
| ifs-api | ContainerApp | `4615c4e9-1584-472d-b158-bdb41c49e4ed` | FranceCentral | — |
| ifs-frontend | ContainerApp | `dda2e846-de85-4739-ba0c-ec15f63e48c7` | FranceCentral | — |

##### KeyVault — `ifs` (`fc210d60`)

| Property | Value |
|----------|-------|
| EnablePurgeProtection | true |
| EnableRbacAuthorization | true |
| EnableSoftDelete | true |
| EnabledForDeployment | false |
| EnabledForDiskEncryption | false |
| EnabledForTemplateDeployment | false |

| Env | Sku |
|-----|-----|
| Development | Standard |

##### StorageAccount — `ifs` (`efe669ac`)

| Property | Value |
|----------|-------|
| AccessTier | Hot |
| AllowBlobPublicAccess | false |
| EnableHttpsTrafficOnly | true |
| Kind | StorageV2 |
| MinimumTlsVersion | Tls12 |

| Env | Sku |
|-----|-----|
| Development | Standard_LRS |

**Blob Containers:**
- `bicep-output` (PublicAccess: None)

**Blob Lifecycle Rules:**
- `clean-ifs` → containers: `[bicep-output]`, TTL: 1 day

##### SqlServer — `ifs` (`9700666f`)

| Property | Value |
|----------|-------|
| Version | V12 |
| AdministratorLogin | sqladmin |

| Env | MinimalTlsVersion |
|-----|-------------------|
| Development | 1.2 |

##### SqlDatabase — `ifs` (`0ae0ab1e`)

| Property | Value |
|----------|-------|
| SqlServerId | `9700666f-4771-46f9-aaab-74d9370eee59` (ifs SqlServer) |
| Collation | SQL_Latin1_General_CP1_CI_AS |

| Env | Sku | MaxSizeGb | ZoneRedundant |
|-----|-----|-----------|---------------|
| Development | Basic | 5 | false |

##### ApplicationInsights — `ifs` (`e8bdb228`)

| Property | Value |
|----------|-------|
| LogAnalyticsWorkspaceId | `83b21c4d` (ifs LAW in Core config) |

| Env | SamplingPercentage | RetentionInDays | DisableIpMasking | DisableLocalAuth | IngestionMode |
|-----|-------------------|-----------------|------------------|------------------|---------------|
| Development | 100 | 30 | true | false | LogAnalytics |

##### ContainerAppEnvironment — `ifs` (`37cfd530`)

| Property | Value |
|----------|-------|
| LogAnalyticsWorkspaceId | `83b21c4d` (ifs LAW in Core config) |

| Env | Sku | WorkloadProfileType | InternalLB | ZoneRedundancy |
|-----|-----|---------------------|------------|----------------|
| Development | Consumption | Consumption | false | false |

##### ContainerApp — `ifs-api` (`4615c4e9`)

| Property | Value |
|----------|-------|
| ContainerAppEnvironmentId | `37cfd530` (ifs CAE) |
| ContainerRegistryId | *(null)* |
| DockerImageName | *(null)* |
| DockerfilePath | *(null)* |
| ApplicationName | *(null)* |

| Env | CpuCores | MemoryGi | MinReplicas | MaxReplicas | IngressEnabled | IngressPort | External | Transport |
|-----|----------|----------|-------------|-------------|----------------|-------------|----------|-----------|
| Development | 0.25 | 0.5Gi | 0 | 1 | true | 80 | true | auto |

##### ContainerApp — `ifs-frontend` (`dda2e846`)

| Property | Value |
|----------|-------|
| ContainerAppEnvironmentId | `37cfd530` (ifs CAE) |
| ContainerRegistryId | *(null)* |
| DockerImageName | *(null)* |
| DockerfilePath | *(null)* |
| ApplicationName | *(null)* |

| Env | CpuCores | MemoryGi | MinReplicas | MaxReplicas | IngressEnabled | IngressPort | External | Transport |
|-----|----------|----------|-------------|-------------|----------------|-------------|----------|-----------|
| Development | 0.25 | 0.5Gi | 0 | 1 | true | 80 | true | auto |

---

## Cross-Config References

| From Config | → Target Config | Target Resource |
|-------------|-----------------|-----------------|
| Infra Flow Sculptor (`3afe7f4f`) | Core (`4ce4be5e`) | LogAnalyticsWorkspace `83b21c4d` |

*ApplicationInsights and ContainerAppEnvironment in "Infra Flow Sculptor" reference the LogAnalyticsWorkspace in "Core".*

---

## Role Assignments

| Source Resource | → Target Resource | Role | ManagedIdentityType |
|-----------------|-------------------|------|---------------------|
| ifs-api (ContainerApp `4615c4e9`) | ifs (KeyVault `fc210d60`) | `4633458b-17de-408a-b874-0445c86b69e6` (Key Vault Secrets User) | SystemAssigned |

---

## App Settings

### ifs-api (ContainerApp `4615c4e9`)

| Name | Source | Details |
|------|--------|---------|
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Output ref: ApplicationInsights (`e8bdb228`) → `connectionString` | Direct output reference |
| `JwtSettings__Secret` | KeyVault secret | KV: `fc210d60`, SecretName: `JWT_SECRET`, Assignment: `ViaBicepparam`, PipelineVar: `jwt-secret` |

---

## Generated File Structure (MonoRepo)

The bicep and pipeline generation produces the following structure in the `infra/` base path:

```
infra/
├── .azuredevops/
│   ├── pipelines/
│   │   ├── ci.pipeline.yml          # Shared CI template
│   │   └── pr.pipeline.yml          # Shared PR template
│   ├── jobs/
│   │   └── deploy.job.yml           # Shared deploy job
│   ├── steps/
│   │   ├── checkout.step.yml        # Shared checkout step
│   │   └── deploy-template.step.yml # ARM deployment step
│   └── variables/
│       └── dev.variables.yml        # Per-env variables
├── Common/                          # Shared Bicep modules
│   ├── types.bicep
│   ├── functions.bicep
│   ├── constants.bicep              # (if role assignments exist)
│   └── modules/
│       ├── KeyVault/
│       ├── StorageAccount/
│       ├── SqlServer/
│       ├── SqlDatabase/
│       ├── ApplicationInsights/
│       ├── ContainerAppEnvironment/
│       ├── ContainerApp/
│       ├── ContainerRegistry/
│       └── LogAnalyticsWorkspace/
├── Core/                            # Config: Core
│   ├── main.bicep
│   ├── parameters/
│   │   └── main.dev.bicepparam
│   ├── ci.pipeline.yml
│   ├── pr.pipeline.yml
│   └── release.pipeline.yml
└── Infra Flow Sculptor/             # Config: Infra Flow Sculptor
    ├── main.bicep
    ├── parameters/
    │   └── main.dev.bicepparam
    ├── ci.pipeline.yml
    ├── pr.pipeline.yml
    └── release.pipeline.yml
```

---

## Azure Subscription

| Field | Value |
|-------|-------|
| SubscriptionId | `83d0b022-c10f-40e7-8eb9-72f37ae7e283` |
| ARM Connection | `ifs` |
