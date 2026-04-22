<#
.SYNOPSIS
    Seeds the InfraFlowSculptor database from the project snapshot fb8699ea-ifs-project.md.

.DESCRIPTION
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
    Write-Host "  Acquiring Azure AD token via 'az account get-access-token'..." -ForegroundColor Cyan
    try {
        $tokenJson = az account get-access-token --resource "api://4f7f2dbd-8a11-42c4-bedc-acbb127e9394" 2>$null | ConvertFrom-Json
        if ($null -eq $tokenJson -or $tokenJson.accessToken -eq "") {
            throw "Empty token returned."
        }
        return $tokenJson.accessToken
    }
    catch {
        Write-Host ""
        Write-Host "  Could not acquire token automatically." -ForegroundColor Yellow
        Write-Host "  Options:" -ForegroundColor Yellow
        Write-Host "    1. Run: az login" -ForegroundColor Yellow
        Write-Host "    2. Run: .\scripts\seed-project-snapshot.ps1 -BearerToken '<your-token>'" -ForegroundColor Yellow
        Write-Host ""
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
        Write-Host "  ERROR $statusCode on $Method $Path" -ForegroundColor Red
        if ($detail) { Write-Host "  $detail" -ForegroundColor Red }
        throw
    }
}

function Step {
    param([string]$Label)
    Write-Host ""
    Write-Host ">> $Label" -ForegroundColor Green
}

# ─────────────────────────────────────────────────────────────────────────────
# 0. Auth & connectivity check
# ─────────────────────────────────────────────────────────────────────────────
Step "Authentication"
$script:Token = Get-AccessToken
Write-Host "  Token acquired." -ForegroundColor Gray

Step "Connectivity check"
try {
    $null = Invoke-WebRequest -Uri "$ApiBaseUrl/health" -UseBasicParsing -TimeoutSec 5 2>$null
    Write-Host "  API is reachable at $ApiBaseUrl" -ForegroundColor Gray
}
catch {
    Write-Host "  WARNING: /health returned an error or is unreachable." -ForegroundColor Yellow
    Write-Host "  Make sure the API is running before continuing." -ForegroundColor Yellow
}

# ─────────────────────────────────────────────────────────────────────────────
# 1. Create Project
# ─────────────────────────────────────────────────────────────────────────────
Step "1 / 13 — Create Project: Infra Flow Sculptor"
$project = Invoke-Api -Method POST -Path "/projects" -Body @{
    Name = "Infra Flow Sculptor"
}
$projectId = $project.id
Write-Host "  Created. ID = $projectId" -ForegroundColor Gray

# ─────────────────────────────────────────────────────────────────────────────
# 2. Repository mode
# ─────────────────────────────────────────────────────────────────────────────
Step "2 / 13 — Set Repository Mode: MonoRepo"
$null = Invoke-Api -Method PUT -Path "/projects/$projectId/repository-mode" -Body @{
    RepositoryMode = "MonoRepo"
}
Write-Host "  Done." -ForegroundColor Gray

# ─────────────────────────────────────────────────────────────────────────────
# 3. Agent pool
# ─────────────────────────────────────────────────────────────────────────────
Step "3 / 13 — Set Agent Pool: Default"
$null = Invoke-Api -Method PUT -Path "/projects/$projectId/agent-pool" -Body @{
    AgentPoolName = "Default"
}
Write-Host "  Done." -ForegroundColor Gray

# ─────────────────────────────────────────────────────────────────────────────
# 4. Project-level naming templates
# ─────────────────────────────────────────────────────────────────────────────
Step "4 / 13 — Set Project Naming Templates"

$null = Invoke-Api -Method PUT -Path "/projects/$projectId/naming/default" -Body @{
    Template = "{name}-{resourceAbbr}{suffix}"
}
Write-Host "  Default template set." -ForegroundColor Gray

$null = Invoke-Api -Method PUT -Path "/projects/$projectId/naming/resources/ResourceGroup" -Body @{
    Template = "{resourceAbbr}-{name}{suffix}"
}
Write-Host "  ResourceGroup template set." -ForegroundColor Gray

$null = Invoke-Api -Method PUT -Path "/projects/$projectId/naming/resources/StorageAccount" -Body @{
    Template = "{name}{resourceAbbr}{envShort}"
}
Write-Host "  StorageAccount template set." -ForegroundColor Gray

$null = Invoke-Api -Method PUT -Path "/projects/$projectId/naming/resources/ContainerRegistry" -Body @{
    Template = "{name}{resourceAbbr}{envShort}"
}
Write-Host "  ContainerRegistry template set." -ForegroundColor Gray

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
Write-Host "  Created. ID = $envId" -ForegroundColor Gray

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
Write-Host "  Tags applied." -ForegroundColor Gray

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
    Write-Host "  Git config set." -ForegroundColor Gray
}
else {
    Step "6 / 13 — Git Config: SKIPPED (no -GitPat provided)"
    Write-Host "  Add git config manually via the UI or re-run with -GitPat <token>." -ForegroundColor Yellow
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
Write-Host "  Created. ID = $coreConfigId" -ForegroundColor Gray

# Resource Group: ifs-core
$rgCore = Invoke-Api -Method POST -Path "/resource-group" -Body @{
    InfraConfigId = [Guid]$coreConfigId
    Name          = "ifs-core"
    Location      = "FranceCentral"
}
$rgCoreId = $rgCore.id
Write-Host "  ResourceGroup 'ifs-core' created. ID = $rgCoreId" -ForegroundColor Gray

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
Write-Host "  ContainerRegistry 'ifs' created. ID = $crId" -ForegroundColor Gray

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
Write-Host "  LogAnalyticsWorkspace 'ifs' created. ID = $lawId" -ForegroundColor Gray

# ─────────────────────────────────────────────────────────────────────────────
# 8. InfraConfig: Infra Flow Sculptor
# ─────────────────────────────────────────────────────────────────────────────
Step "8 / 13 — Create InfraConfig: Infra Flow Sculptor"
$ifsConfig = Invoke-Api -Method POST -Path "/infra-config" -Body @{
    Name      = "Infra Flow Sculptor"
    ProjectId = $projectId
}
$ifsConfigId = $ifsConfig.id
Write-Host "  Created. ID = $ifsConfigId" -ForegroundColor Gray

# Cross-config reference to LAW in Core (needed by ApplicationInsights & CAE)
$null = Invoke-Api -Method POST -Path "/infra-config/$ifsConfigId/cross-config-references" -Body @{
    TargetResourceId = [Guid]$lawId
}
Write-Host "  Cross-config reference to LogAnalyticsWorkspace added." -ForegroundColor Gray

# Resource Group: ifs
$rgIfs = Invoke-Api -Method POST -Path "/resource-group" -Body @{
    InfraConfigId = [Guid]$ifsConfigId
    Name          = "ifs"
    Location      = "FranceCentral"
}
$rgIfsId = $rgIfs.id
Write-Host "  ResourceGroup 'ifs' created. ID = $rgIfsId" -ForegroundColor Gray

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
Write-Host "  KeyVault 'ifs' created. ID = $kvId" -ForegroundColor Gray

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
Write-Host "  StorageAccount 'ifs' created. ID = $saId" -ForegroundColor Gray

# Blob Container: bicep-output
$null = Invoke-Api -Method POST -Path "/storage-accounts/$saId/blob-containers" -Body @{
    Name         = "bicep-output"
    PublicAccess = "None"
}
Write-Host "  BlobContainer 'bicep-output' added." -ForegroundColor Gray

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
Write-Host "  SqlServer 'ifs' created. ID = $sqlSrvId" -ForegroundColor Gray

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
Write-Host "  SqlDatabase 'ifs' created. ID = $sqlDbId" -ForegroundColor Gray

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
Write-Host "  ApplicationInsights 'ifs' created. ID = $appInsightsId" -ForegroundColor Gray

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
Write-Host "  ContainerAppEnvironment 'ifs' created. ID = $caeId" -ForegroundColor Gray

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
Write-Host "  ContainerApp 'ifs-api' created. ID = $caApiId" -ForegroundColor Gray

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
Write-Host "  ContainerApp 'ifs-frontend' created. ID = $caFrontId" -ForegroundColor Gray

# ─────────────────────────────────────────────────────────────────────────────
# 10. Role Assignment: ifs-api -> KeyVault (Key Vault Secrets User)
# ─────────────────────────────────────────────────────────────────────────────
Step "10 / 13 — Role Assignment: ifs-api -> KeyVault (Secrets User)"
$null = Invoke-Api -Method POST -Path "/azure-resources/$caApiId/role-assignments" -Body @{
    TargetResourceId     = [Guid]$kvId
    ManagedIdentityType  = "SystemAssigned"
    RoleDefinitionId     = "4633458b-17de-408a-b874-0445c86b69e6"
}
Write-Host "  Role assignment added." -ForegroundColor Gray

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
Write-Host "  App setting 'APPLICATIONINSIGHTS_CONNECTION_STRING' added." -ForegroundColor Gray

# JwtSettings__Secret — Key Vault secret reference
$null = Invoke-Api -Method POST -Path "/azure-resources/$caApiId/app-settings" -Body @{
    Name                   = "JwtSettings__Secret"
    KeyVaultResourceId     = [Guid]$kvId
    SecretName             = "JWT_SECRET"
    SecretValueAssignment  = "ViaBicepparam"
}
Write-Host "  App setting 'JwtSettings__Secret' added." -ForegroundColor Gray

# ─────────────────────────────────────────────────────────────────────────────
# 12. Summary
# ─────────────────────────────────────────────────────────────────────────────
Step "12 / 13 — Seeding complete!"
Write-Host ""
Write-Host "  Seeded IDs (for reference / snapshot update):" -ForegroundColor Cyan
Write-Host "  Project              : $projectId"
Write-Host "  Environment (dev)    : $envId"
Write-Host "  InfraConfig Core     : $coreConfigId"
Write-Host "    RG ifs-core        : $rgCoreId"
Write-Host "    ContainerRegistry  : $crId"
Write-Host "    LogAnalyticsWS     : $lawId"
Write-Host "  InfraConfig IFS      : $ifsConfigId"
Write-Host "    RG ifs             : $rgIfsId"
Write-Host "    KeyVault           : $kvId"
Write-Host "    StorageAccount     : $saId"
Write-Host "    SqlServer          : $sqlSrvId"
Write-Host "    SqlDatabase        : $sqlDbId"
Write-Host "    ApplicationInsights: $appInsightsId"
Write-Host "    CAE ifs            : $caeId"
Write-Host "    ContainerApp api   : $caApiId"
Write-Host "    ContainerApp front : $caFrontId"
Write-Host ""

if ($GitPat -eq "") {
    Write-Host "  REMINDER: Git config was skipped. Run:" -ForegroundColor Yellow
    Write-Host "    .\scripts\seed-project-snapshot.ps1 -GitPat '<your-azure-devops-pat>'" -ForegroundColor Yellow
    Write-Host "  Or configure it via the InfraFlowSculptor UI." -ForegroundColor Yellow
    Write-Host ""
}

Step "13 / 13 — Done"
