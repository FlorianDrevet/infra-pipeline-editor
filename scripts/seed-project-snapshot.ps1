<#
.SYNOPSIS
    Legacy API-based helper kept for historical context; the authoritative local snapshot seed is scripts/seed-project-snapshot.sql.

.DESCRIPTION
    This script is not the authoritative snapshot seed anymore.
    The canonical direct-db seed pair is:
      - docs/project-snapshots/fb8699ea-ifs-project.md
      - scripts/seed-project-snapshot.sql

    When a task updates the snapshot or the seed, both files must be updated together.

    Calls the InfraFlowSculptor API in the correct order to recreate the full project
    configuration described in docs/project-snapshots/fb8699ea-ifs-project.md.

    Prerequisites:
      - API is running (Aspire or standalone on http://localhost:5257)
      - Azure CLI installed and logged in  OR  a Bearer token provided via -BearerToken

.PARAMETER ApiBaseUrl
    Base URL of the InfraFlowSculptor API. Defaults to http://localhost:5257.

.PARAMETER BearerToken
    Pre-acquired Bearer token. If omitted, the script will call `az account get-access-token`.

.PARAMETER GitPat
    Personal Access Token for the Azure DevOps repository.
    If omitted, git config is skipped — you can add it manually via the UI.

.EXAMPLE
    .\scripts\seed-project-snapshot.ps1

.EXAMPLE
    .\scripts\seed-project-snapshot.ps1 -ApiBaseUrl "https://localhost:7246" -GitPat "mypat123"
#>
param(
    [string]$ApiBaseUrl = "http://localhost:5257",
    [string]$BearerToken = "",
    [string]$GitPat = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ─────────────────────────────────────────────────────────────────────────────
# Helpers
# ─────────────────────────────────────────────────────────────────────────────

function Get-AccessToken {
    if ($BearerToken -ne "") {
        return $BearerToken
    }
    Write-Output "  Acquiring Azure AD token via 'az account get-access-token'..."
    try {
        $tokenJson = az account get-access-token --resource "api://4f7f2dbd-8a11-42c4-bedc-acbb127e9394" 2>$null | ConvertFrom-Json
        if ($null -eq $tokenJson -or $tokenJson.accessToken -eq "") {
            throw "Empty token returned."
        }
        return $tokenJson.accessToken
    }
    catch {
        Write-Output ""
        Write-Output "  Could not acquire token automatically."
        Write-Output "  Options:"
        Write-Output "    1. Run: az login"
        Write-Output "    2. Run: .\scripts\seed-project-snapshot.ps1 -BearerToken '<your-token>'"
        Write-Output ""
        throw "Authentication failed. See above for options."
    }
}

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        [object]$Body = $null
    )

    $uri = "$ApiBaseUrl$Path"
    $headers = @{
        "Authorization" = "Bearer $script:Token"
        "Content-Type"  = "application/json"
    }

    $params = @{
        Method  = $Method
        Uri     = $uri
        Headers = $headers
    }

    if ($null -ne $Body) {
        $params["Body"] = ($Body | ConvertTo-Json -Depth 20 -Compress)
    }

    try {
        $response = Invoke-RestMethod @params
        return $response
    }
    catch {
        $statusCode = $_.Exception.Response?.StatusCode
        $detail     = $_.ErrorDetails?.Message
        Write-Output "  ERROR $statusCode on $Method $Path"
        if ($detail) { Write-Output "  $detail" }
        throw
    }
}

function Step {
    param([string]$Label)
    Write-Output ""
    Write-Output ">> $Label"
}

# ─────────────────────────────────────────────────────────────────────────────
# 0. Auth & connectivity check
# ─────────────────────────────────────────────────────────────────────────────
Step "Authentication"
$script:Token = Get-AccessToken
Write-Output "  Token acquired."

Step "Connectivity check"
try {
    $null = Invoke-WebRequest -Uri "$ApiBaseUrl/health" -UseBasicParsing -TimeoutSec 5 2>$null
    Write-Output "  API is reachable at $ApiBaseUrl"
}
catch {
    Write-Output "  WARNING: /health returned an error or is unreachable."
    Write-Output "  Make sure the API is running before continuing."
}

# ─────────────────────────────────────────────────────────────────────────────
# 1. Create Project
# ─────────────────────────────────────────────────────────────────────────────
Step "1 / 13 — Create Project: Infra Flow Sculptor"
$project = Invoke-Api -Method POST -Path "/projects" -Body @{
    Name = "Infra Flow Sculptor"
}
$projectId = $project.id
Write-Output "  Created. ID = $projectId"

# ─────────────────────────────────────────────────────────────────────────────
# 2. Repository mode
# ─────────────────────────────────────────────────────────────────────────────
Step "2 / 13 — Set Repository Mode: MonoRepo"
$null = Invoke-Api -Method PUT -Path "/projects/$projectId/repository-mode" -Body @{
    RepositoryMode = "MonoRepo"
}
Write-Output "  Done."

# ─────────────────────────────────────────────────────────────────────────────
# 3. Agent pool
# ─────────────────────────────────────────────────────────────────────────────
Step "3 / 13 — Set Agent Pool: Default"
$null = Invoke-Api -Method PUT -Path "/projects/$projectId/agent-pool" -Body @{
    AgentPoolName = "Default"
}
Write-Output "  Done."

# ─────────────────────────────────────────────────────────────────────────────
# 4. Project-level naming templates
# ─────────────────────────────────────────────────────────────────────────────
Step "4 / 13 — Set Project Naming Templates"

$null = Invoke-Api -Method PUT -Path "/projects/$projectId/naming/default" -Body @{
    Template = "{name}-{resourceAbbr}{suffix}"
}
Write-Output "  Default template set."

$null = Invoke-Api -Method PUT -Path "/projects/$projectId/naming/resources/ResourceGroup" -Body @{
    Template = "{resourceAbbr}-{name}{suffix}"
}
Write-Output "  ResourceGroup template set."

$null = Invoke-Api -Method PUT -Path "/projects/$projectId/naming/resources/StorageAccount" -Body @{
    Template = "{name}{resourceAbbr}{envShort}"
}
Write-Output "  StorageAccount template set."

$null = Invoke-Api -Method PUT -Path "/projects/$projectId/naming/resources/ContainerRegistry" -Body @{
    Template = "{name}{resourceAbbr}{envShort}"
}
Write-Output "  ContainerRegistry template set."

# ─────────────────────────────────────────────────────────────────────────────
# 5. Environment: Development
# ─────────────────────────────────────────────────────────────────────────────
Step "5 / 13 — Add Environment: Development"
$envCreated = Invoke-Api -Method POST -Path "/projects/$projectId/environments" -Body @{
    Name                          = "Development"
    ShortName                     = "dev"
    Prefix                        = "dev-"
    Suffix                        = "-dev"
    Location                      = "FranceCentral"
    SubscriptionId                = [Guid]"83d0b022-c10f-40e7-8eb9-72f37ae7e283"
    Order                         = 0
    RequiresApproval              = $false
    AzureResourceManagerConnection = "ifs"
}
$envId = $envCreated.id
Write-Output "  Created. ID = $envId"

# Update to add tags (tags not available on create)
$null = Invoke-Api -Method PUT -Path "/projects/$projectId/environments/$envId" -Body @{
    Name                          = "Development"
    ShortName                     = "dev"
    Prefix                        = "dev-"
    Suffix                        = "-dev"
    Location                      = "FranceCentral"
    SubscriptionId                = [Guid]"83d0b022-c10f-40e7-8eb9-72f37ae7e283"
    Order                         = 0
    RequiresApproval              = $false
    AzureResourceManagerConnection = "ifs"
    Tags                          = @( @{ Name = "environment"; Value = "dev" } )
}
Write-Output "  Tags applied."

# ─────────────────────────────────────────────────────────────────────────────
# 6. Git config  (optional — requires PAT)
# ─────────────────────────────────────────────────────────────────────────────
if ($GitPat -ne "") {
    Step "6 / 13 — Set Git Config: AzureDevOps"
    $null = Invoke-Api -Method PUT -Path "/projects/$projectId/git-config" -Body @{
        ProviderType        = "AzureDevOps"
        RepositoryUrl       = "https://dev.azure.com/floriandrevet0332/Infra%20Flow%20Sculptor/_git/ifs"
        DefaultBranch       = "main"
        BasePath            = "infra"
        PipelineBasePath    = $null
        PersonalAccessToken = $GitPat
    }
    Write-Output "  Git config set."
}
else {
    Step "6 / 13 — Git Config: SKIPPED (no -GitPat provided)"
    Write-Output "  Add git config manually via the UI or re-run with -GitPat <token>."
}

# ─────────────────────────────────────────────────────────────────────────────
# 7. InfraConfig: Core
# ─────────────────────────────────────────────────────────────────────────────
Step "7 / 13 — Create InfraConfig: Core"
$coreConfig = Invoke-Api -Method POST -Path "/infra-config" -Body @{
    Name      = "Core"
    ProjectId = $projectId
}
$coreConfigId = $coreConfig.id
Write-Output "  Created. ID = $coreConfigId"

# Resource Group: ifs-core
$rgCore = Invoke-Api -Method POST -Path "/resource-group" -Body @{
    InfraConfigId = [Guid]$coreConfigId
    Name          = "ifs-core"
    Location      = "FranceCentral"
}
$rgCoreId = $rgCore.id
Write-Output "  ResourceGroup 'ifs-core' created. ID = $rgCoreId"

# ContainerRegistry: ifs
$cr = Invoke-Api -Method POST -Path "/container-registry" -Body @{
    ResourceGroupId     = [Guid]$rgCoreId
    Name                = "ifs"
    Location            = "FranceCentral"
    EnvironmentSettings = @(
        @{
            EnvironmentName      = "Development"
            Sku                  = "Basic"
            AdminUserEnabled     = $false
            PublicNetworkAccess  = "Enabled"
            ZoneRedundancy       = $false
        }
    )
}
$crId = $cr.id
Write-Output "  ContainerRegistry 'ifs' created. ID = $crId"

# LogAnalyticsWorkspace: ifs
$law = Invoke-Api -Method POST -Path "/log-analytics-workspace" -Body @{
    ResourceGroupId     = [Guid]$rgCoreId
    Name                = "ifs"
    Location            = "FranceCentral"
    EnvironmentSettings = @(
        @{
            EnvironmentName  = "Development"
            Sku              = "Free"
            RetentionInDays  = 30
            DailyQuotaGb     = 2
        }
    )
}
$lawId = $law.id
Write-Output "  LogAnalyticsWorkspace 'ifs' created. ID = $lawId"

# ─────────────────────────────────────────────────────────────────────────────
# 8. InfraConfig: Infra Flow Sculptor
# ─────────────────────────────────────────────────────────────────────────────
Step "8 / 13 — Create InfraConfig: Infra Flow Sculptor"
$ifsConfig = Invoke-Api -Method POST -Path "/infra-config" -Body @{
    Name      = "Infra Flow Sculptor"
    ProjectId = $projectId
}
$ifsConfigId = $ifsConfig.id
Write-Output "  Created. ID = $ifsConfigId"

# Cross-config reference to LAW in Core (needed by ApplicationInsights & CAE)
$null = Invoke-Api -Method POST -Path "/infra-config/$ifsConfigId/cross-config-references" -Body @{
    TargetResourceId = [Guid]$lawId
}
Write-Output "  Cross-config reference to LogAnalyticsWorkspace added."

# Resource Group: ifs
$rgIfs = Invoke-Api -Method POST -Path "/resource-group" -Body @{
    InfraConfigId = [Guid]$ifsConfigId
    Name          = "ifs"
    Location      = "FranceCentral"
}
$rgIfsId = $rgIfs.id
Write-Output "  ResourceGroup 'ifs' created. ID = $rgIfsId"

# ─────────────────────────────────────────────────────────────────────────────
# 9. Resources in ifs
# ─────────────────────────────────────────────────────────────────────────────
Step "9 / 13 — Create Resources in 'ifs'"

# KeyVault
$kv = Invoke-Api -Method POST -Path "/keyvault" -Body @{
    ResourceGroupId             = [Guid]$rgIfsId
    Name                        = "ifs"
    Location                    = "FranceCentral"
    EnablePurgeProtection       = $true
    EnableRbacAuthorization     = $true
    EnableSoftDelete            = $true
    EnabledForDeployment        = $false
    EnabledForDiskEncryption    = $false
    EnabledForTemplateDeployment = $false
    EnvironmentSettings         = @(
        @{ EnvironmentName = "Development"; Sku = "Standard" }
    )
}
$kvId = $kv.id
Write-Output "  KeyVault 'ifs' created. ID = $kvId"

# StorageAccount
$sa = Invoke-Api -Method POST -Path "/storage-accounts" -Body @{
    ResourceGroupId        = [Guid]$rgIfsId
    Name                   = "ifs"
    Location               = "FranceCentral"
    Kind                   = "StorageV2"
    AccessTier             = "Hot"
    AllowBlobPublicAccess  = $false
    EnableHttpsTrafficOnly = $true
    MinimumTlsVersion      = "Tls12"
    EnvironmentSettings    = @(
        @{ EnvironmentName = "Development"; Sku = "Standard_LRS" }
    )
    LifecycleRules         = @(
        @{
            RuleName          = "clean-ifs"
            ContainerNames    = @("bicep-output")
            TimeToLiveInDays  = 1
        }
    )
}
$saId = $sa.id
Write-Output "  StorageAccount 'ifs' created. ID = $saId"

# Blob Container: bicep-output
$null = Invoke-Api -Method POST -Path "/storage-accounts/$saId/blob-containers" -Body @{
    Name         = "bicep-output"
    PublicAccess = "None"
}
Write-Output "  BlobContainer 'bicep-output' added."

# SqlServer
$sqlSrv = Invoke-Api -Method POST -Path "/sql-server" -Body @{
    ResourceGroupId    = [Guid]$rgIfsId
    Name               = "ifs"
    Location           = "FranceCentral"
    Version            = "V12"
    AdministratorLogin = "sqladmin"
    EnvironmentSettings = @(
        @{ EnvironmentName = "Development"; MinimalTlsVersion = "1.2" }
    )
}
$sqlSrvId = $sqlSrv.id
Write-Output "  SqlServer 'ifs' created. ID = $sqlSrvId"

# SqlDatabase
$sqlDb = Invoke-Api -Method POST -Path "/sql-database" -Body @{
    ResourceGroupId = [Guid]$rgIfsId
    Name            = "ifs"
    Location        = "FranceCentral"
    SqlServerId     = [Guid]$sqlSrvId
    Collation       = "SQL_Latin1_General_CP1_CI_AS"
    EnvironmentSettings = @(
        @{
            EnvironmentName = "Development"
            Sku             = "Basic"
            MaxSizeGb       = 5
            ZoneRedundant   = $false
        }
    )
}
$sqlDbId = $sqlDb.id
Write-Output "  SqlDatabase 'ifs' created. ID = $sqlDbId"

# ApplicationInsights (references LAW in Core)
$appInsights = Invoke-Api -Method POST -Path "/application-insights" -Body @{
    ResourceGroupId          = [Guid]$rgIfsId
    Name                     = "ifs"
    Location                 = "FranceCentral"
    LogAnalyticsWorkspaceId  = [Guid]$lawId
    EnvironmentSettings      = @(
        @{
            EnvironmentName   = "Development"
            SamplingPercentage = 100
            RetentionInDays   = 30
            DisableIpMasking  = $true
            DisableLocalAuth  = $false
            IngestionMode     = "LogAnalytics"
        }
    )
}
$appInsightsId = $appInsights.id
Write-Output "  ApplicationInsights 'ifs' created. ID = $appInsightsId"

# ContainerAppEnvironment (references LAW in Core)
$cae = Invoke-Api -Method POST -Path "/container-app-environment" -Body @{
    ResourceGroupId          = [Guid]$rgIfsId
    Name                     = "ifs"
    Location                 = "FranceCentral"
    LogAnalyticsWorkspaceId  = [Guid]$lawId
    EnvironmentSettings      = @(
        @{
            EnvironmentName          = "Development"
            Sku                      = "Consumption"
            WorkloadProfileType      = "Consumption"
            InternalLoadBalancerEnabled = $false
            ZoneRedundancyEnabled    = $false
        }
    )
}
$caeId = $cae.id
Write-Output "  ContainerAppEnvironment 'ifs' created. ID = $caeId"

# ContainerApp: ifs-api
$caApi = Invoke-Api -Method POST -Path "/container-app" -Body @{
    ResourceGroupId            = [Guid]$rgIfsId
    Name                       = "ifs-api"
    Location                   = "FranceCentral"
    ContainerAppEnvironmentId  = [Guid]$caeId
    ContainerRegistryId        = $null
    DockerImageName            = $null
    DockerfilePath             = $null
    ApplicationName            = $null
    EnvironmentSettings        = @(
        @{
            EnvironmentName  = "Development"
            CpuCores         = "0.25"
            MemoryGi         = "0.5Gi"
            MinReplicas      = 0
            MaxReplicas      = 1
            IngressEnabled   = $true
            IngressTargetPort = 80
            IngressExternal  = $true
            TransportMethod  = "auto"
        }
    )
}
$caApiId = $caApi.id
Write-Output "  ContainerApp 'ifs-api' created. ID = $caApiId"

# ContainerApp: ifs-frontend
$caFront = Invoke-Api -Method POST -Path "/container-app" -Body @{
    ResourceGroupId            = [Guid]$rgIfsId
    Name                       = "ifs-frontend"
    Location                   = "FranceCentral"
    ContainerAppEnvironmentId  = [Guid]$caeId
    ContainerRegistryId        = $null
    DockerImageName            = $null
    DockerfilePath             = $null
    ApplicationName            = $null
    EnvironmentSettings        = @(
        @{
            EnvironmentName  = "Development"
            CpuCores         = "0.25"
            MemoryGi         = "0.5Gi"
            MinReplicas      = 0
            MaxReplicas      = 1
            IngressEnabled   = $true
            IngressTargetPort = 80
            IngressExternal  = $true
            TransportMethod  = "auto"
        }
    )
}
$caFrontId = $caFront.id
Write-Output "  ContainerApp 'ifs-frontend' created. ID = $caFrontId"

# ─────────────────────────────────────────────────────────────────────────────
# 10. Role Assignment: ifs-api -> KeyVault (Key Vault Secrets User)
# ─────────────────────────────────────────────────────────────────────────────
Step "10 / 13 — Role Assignment: ifs-api -> KeyVault (Secrets User)"
$null = Invoke-Api -Method POST -Path "/azure-resources/$caApiId/role-assignments" -Body @{
    TargetResourceId     = [Guid]$kvId
    ManagedIdentityType  = "SystemAssigned"
    RoleDefinitionId     = "4633458b-17de-408a-b874-0445c86b69e6"
}
Write-Output "  Role assignment added."

# ─────────────────────────────────────────────────────────────────────────────
# 11. App Settings on ifs-api
# ─────────────────────────────────────────────────────────────────────────────
Step "11 / 13 — App Settings: ifs-api"

# APPLICATIONINSIGHTS_CONNECTION_STRING — output reference to ApplicationInsights
$null = Invoke-Api -Method POST -Path "/azure-resources/$caApiId/app-settings" -Body @{
    Name             = "APPLICATIONINSIGHTS_CONNECTION_STRING"
    SourceResourceId = [Guid]$appInsightsId
    SourceOutputName = "connectionString"
}
Write-Output "  App setting 'APPLICATIONINSIGHTS_CONNECTION_STRING' added."

# JwtSettings__Secret — Key Vault secret reference
$null = Invoke-Api -Method POST -Path "/azure-resources/$caApiId/app-settings" -Body @{
    Name                   = "JwtSettings__Secret"
    KeyVaultResourceId     = [Guid]$kvId
    SecretName             = "JWT_SECRET"
    SecretValueAssignment  = "ViaBicepparam"
}
Write-Output "  App setting 'JwtSettings__Secret' added."

# ─────────────────────────────────────────────────────────────────────────────
# 12. Summary
# ─────────────────────────────────────────────────────────────────────────────
Step "12 / 13 — Seeding complete!"
Write-Output ""
Write-Output "  Seeded IDs (for reference / snapshot update):"
Write-Output "  Project              : $projectId"
Write-Output "  Environment (dev)    : $envId"
Write-Output "  InfraConfig Core     : $coreConfigId"
Write-Output "    RG ifs-core        : $rgCoreId"
Write-Output "    ContainerRegistry  : $crId"
Write-Output "    LogAnalyticsWS     : $lawId"
Write-Output "  InfraConfig IFS      : $ifsConfigId"
Write-Output "    RG ifs             : $rgIfsId"
Write-Output "    KeyVault           : $kvId"
Write-Output "    StorageAccount     : $saId"
Write-Output "    SqlServer          : $sqlSrvId"
Write-Output "    SqlDatabase        : $sqlDbId"
Write-Output "    ApplicationInsights: $appInsightsId"
Write-Output "    CAE ifs            : $caeId"
Write-Output "    ContainerApp api   : $caApiId"
Write-Output "    ContainerApp front : $caFrontId"
Write-Output ""

if ($GitPat -eq "") {
    Write-Output "  REMINDER: Git config was skipped. Run:"
    Write-Output "    .\scripts\seed-project-snapshot.ps1 -GitPat '<your-azure-devops-pat>'"
    Write-Output "  Or configure it via the InfraFlowSculptor UI."
    Write-Output ""
}

Step "13 / 13 — Done"
