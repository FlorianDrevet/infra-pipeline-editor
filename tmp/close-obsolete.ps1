$repo = 'FlorianDrevet/infra-pipeline-editor'
$obsolete = @(
  @{ Num=162; Reason="9 projets de tests existent desormais sous tests/ (Domain.Tests, Application.Tests, Infrastructure.Tests, Contracts.Tests, BicepGeneration.Tests, GenerationCore.Tests, GenerationParity.Tests, Mcp.Tests, PipelineGeneration.Tests) totalisant ~325 fichiers de test. Le finding 'aucun projet *Tests*' n'est plus valide. Les besoins residuels (couverture domaine/handlers critiques) seront traites issue par issue." },
  @{ Num=171; Reason="Verifie dans src/Api/InfraFlowSculptor.Domain/Common/Models/SingleValueObject.cs L11 : 'public T Value { get; private set; }'. Le setter n'est plus public, le finding est obsolete." },
  @{ Num=198; Reason="Verifie sur 18 sous-classes EnumValueObject<T> du domaine (Location, GitProviderType, LayoutPreset, ConfigLayoutMode, AcrAuthMode, DeploymentMode, ManagedIdentityType, AppServicePlanSku/OsType, StorageAccountSku/Kind/TlsVersion/AccessTier/CorsServiceType/BlobContainerPublicAccess, FunctionAppRuntimeStack, WebAppRuntimeStack, SqlServerVersion, SqlDatabaseSku, RedisCacheSku, TlsVersion). Toutes sont sealed. Finding obsolete." },
  @{ Num=241; Reason="Verifie sur 18 agregats AzureResource (WebApp, FunctionApp, StorageAccount, KeyVault, AppConfiguration, ApplicationInsights, ContainerApp, ContainerAppEnvironment, ContainerRegistry, CosmosDb, EventHubNamespace, LogAnalyticsWorkspace, RedisCache, ServiceBusNamespace, SqlDatabase, SqlServer, AppServicePlan, UserAssignedIdentity). Toutes declarees sealed. Finding obsolete." }
)
foreach ($o in $obsolete) {
  Write-Host "Closing #$($o.Num) (obsolete)"
  & gh issue close $o.Num --repo $repo --reason 'completed' --comment $o.Reason
  if ($LASTEXITCODE -ne 0) { Write-Host "FAIL #$($o.Num)" -ForegroundColor Red }
}
"Done"
