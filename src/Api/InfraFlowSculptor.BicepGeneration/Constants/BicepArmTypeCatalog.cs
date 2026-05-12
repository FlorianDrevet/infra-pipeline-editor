using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Constants;

/// <summary>
/// Centralizes ARM API versions and fully qualified Bicep resource types used by the generation layer.
/// </summary>
internal static class BicepArmTypeCatalog
{
    internal const string DefaultExistingResourceApiVersion = "2023-01-01";
    internal const string KeyVaultApiVersion = "2023-07-01";
    internal const string RedisCacheApiVersion = "2023-08-01";
    internal const string RedisCacheExistingApiVersion = "2024-03-01";
    internal const string StorageAccountApiVersion = "2025-06-01";
    internal const string StorageAccountExistingApiVersion = "2023-05-01";
    internal const string StorageAccountRoleAssignmentApiVersion = "2023-01-01";
    internal const string AppServicePlanApiVersion = "2023-12-01";
    internal const string WebAppApiVersion = "2023-12-01";
    internal const string UserAssignedIdentityApiVersion = "2023-01-31";
    internal const string AppConfigurationApiVersion = "2023-03-01";
    internal const string ContainerAppEnvironmentApiVersion = "2024-03-01";
    internal const string ContainerAppApiVersion = "2024-03-01";
    internal const string LogAnalyticsWorkspaceApiVersion = "2023-09-01";
    internal const string ApplicationInsightsApiVersion = "2020-02-02";
    internal const string CosmosDbApiVersion = "2024-05-15";
    internal const string SqlServerApiVersion = "2023-08-01-preview";
    internal const string SqlDatabaseApiVersion = "2023-08-01-preview";
    internal const string ServiceBusNamespaceApiVersion = "2022-10-01-preview";
    internal const string ContainerRegistryApiVersion = "2023-07-01";
    internal const string EventHubNamespaceApiVersion = "2024-01-01";
    internal const string ResourceGroupApiVersion = "2024-07-01";
    internal const string DiagnosticSettingsApiVersion = "2021-05-01-preview";
    internal const string RoleAssignmentsApiVersion = "2022-04-01";

    private const string StorageBlobServicesType = AzureResourceTypes.ArmTypes.StorageAccountType + "/blobServices";
    private const string StorageBlobContainersType = StorageBlobServicesType + "/containers";
    private const string StorageManagementPoliciesType = AzureResourceTypes.ArmTypes.StorageAccountType + "/managementPolicies";
    private const string StorageTableServicesType = AzureResourceTypes.ArmTypes.StorageAccountType + "/tableServices";
    private const string StorageTablesType = StorageTableServicesType + "/tables";
    private const string StorageQueueServicesType = AzureResourceTypes.ArmTypes.StorageAccountType + "/queueServices";
    private const string StorageQueuesType = StorageQueueServicesType + "/queues";
    private const string HostNameBindingsType = AzureResourceTypes.ArmTypes.WebAppType + "/hostNameBindings";
    private const string KeyVaultSecretsType = AzureResourceTypes.ArmTypes.KeyVaultType + "/secrets";
    private const string DiagnosticSettingsType = "Microsoft.Insights/diagnosticSettings";
    private const string RoleAssignmentsType = "Microsoft.Authorization/roleAssignments";
    private const string ResourceGroupsType = "Microsoft.Resources/resourceGroups";

    internal const string KeyVaultArmType = AzureResourceTypes.ArmTypes.KeyVaultType + "@" + KeyVaultApiVersion;
    internal const string KeyVaultSecretsArmType = KeyVaultSecretsType + "@" + KeyVaultApiVersion;
    internal const string RedisCacheArmType = AzureResourceTypes.ArmTypes.RedisCacheType + "@" + RedisCacheApiVersion;
    internal const string StorageAccountArmType = AzureResourceTypes.ArmTypes.StorageAccountType + "@" + StorageAccountApiVersion;
    internal const string StorageBlobServicesArmType = StorageBlobServicesType + "@" + StorageAccountApiVersion;
    internal const string StorageBlobContainersArmType = StorageBlobContainersType + "@" + StorageAccountApiVersion;
    internal const string StorageManagementPoliciesArmType = StorageManagementPoliciesType + "@" + StorageAccountApiVersion;
    internal const string StorageTableServicesArmType = StorageTableServicesType + "@" + StorageAccountApiVersion;
    internal const string StorageTablesArmType = StorageTablesType + "@" + StorageAccountApiVersion;
    internal const string StorageQueueServicesArmType = StorageQueueServicesType + "@" + StorageAccountApiVersion;
    internal const string StorageQueuesArmType = StorageQueuesType + "@" + StorageAccountApiVersion;
    internal const string AppServicePlanArmType = AzureResourceTypes.ArmTypes.AppServicePlanType + "@" + AppServicePlanApiVersion;
    internal const string WebAppArmType = AzureResourceTypes.ArmTypes.WebAppType + "@" + WebAppApiVersion;
    internal const string HostNameBindingsArmType = HostNameBindingsType + "@" + WebAppApiVersion;
    internal const string UserAssignedIdentityArmType = AzureResourceTypes.ArmTypes.UserAssignedIdentityType + "@" + UserAssignedIdentityApiVersion;
    internal const string AppConfigurationArmType = AzureResourceTypes.ArmTypes.AppConfigurationType + "@" + AppConfigurationApiVersion;
    internal const string ContainerAppEnvironmentArmType = AzureResourceTypes.ArmTypes.ContainerAppEnvironmentType + "@" + ContainerAppEnvironmentApiVersion;
    internal const string ContainerAppArmType = AzureResourceTypes.ArmTypes.ContainerAppType + "@" + ContainerAppApiVersion;
    internal const string LogAnalyticsWorkspaceArmType = AzureResourceTypes.ArmTypes.LogAnalyticsWorkspaceType + "@" + LogAnalyticsWorkspaceApiVersion;
    internal const string ApplicationInsightsArmType = AzureResourceTypes.ArmTypes.ApplicationInsightsType + "@" + ApplicationInsightsApiVersion;
    internal const string CosmosDbArmType = AzureResourceTypes.ArmTypes.CosmosDbType + "@" + CosmosDbApiVersion;
    internal const string SqlServerArmType = AzureResourceTypes.ArmTypes.SqlServerType + "@" + SqlServerApiVersion;
    internal const string SqlDatabaseArmType = AzureResourceTypes.ArmTypes.SqlDatabaseType + "@" + SqlDatabaseApiVersion;
    internal const string ServiceBusNamespaceArmType = AzureResourceTypes.ArmTypes.ServiceBusNamespaceType + "@" + ServiceBusNamespaceApiVersion;
    internal const string ContainerRegistryArmType = AzureResourceTypes.ArmTypes.ContainerRegistryType + "@" + ContainerRegistryApiVersion;
    internal const string EventHubNamespaceArmType = AzureResourceTypes.ArmTypes.EventHubNamespaceType + "@" + EventHubNamespaceApiVersion;
    internal const string DiagnosticSettingsArmType = DiagnosticSettingsType + "@" + DiagnosticSettingsApiVersion;
    internal const string RoleAssignmentsArmType = RoleAssignmentsType + "@" + RoleAssignmentsApiVersion;
    internal const string ResourceGroupsArmType = ResourceGroupsType + "@" + ResourceGroupApiVersion;

    internal static string GetExistingResourceApiVersion(string armResourceType) =>
        armResourceType switch
        {
            AzureResourceTypes.ArmTypes.KeyVaultType => KeyVaultApiVersion,
            AzureResourceTypes.ArmTypes.RedisCacheType => RedisCacheExistingApiVersion,
            AzureResourceTypes.ArmTypes.StorageAccountType => StorageAccountExistingApiVersion,
            AzureResourceTypes.ArmTypes.AppServicePlanType => AppServicePlanApiVersion,
            AzureResourceTypes.ArmTypes.WebAppType => WebAppApiVersion,
            AzureResourceTypes.ArmTypes.FunctionAppType => WebAppApiVersion,
            AzureResourceTypes.ArmTypes.UserAssignedIdentityType => UserAssignedIdentityApiVersion,
            AzureResourceTypes.ArmTypes.AppConfigurationType => AppConfigurationApiVersion,
            AzureResourceTypes.ArmTypes.ContainerAppEnvironmentType => ContainerAppEnvironmentApiVersion,
            AzureResourceTypes.ArmTypes.ContainerAppType => ContainerAppApiVersion,
            AzureResourceTypes.ArmTypes.LogAnalyticsWorkspaceType => LogAnalyticsWorkspaceApiVersion,
            AzureResourceTypes.ArmTypes.ApplicationInsightsType => ApplicationInsightsApiVersion,
            AzureResourceTypes.ArmTypes.CosmosDbType => CosmosDbApiVersion,
            AzureResourceTypes.ArmTypes.SqlServerType => SqlServerApiVersion,
            AzureResourceTypes.ArmTypes.SqlDatabaseType => SqlDatabaseApiVersion,
            AzureResourceTypes.ArmTypes.ServiceBusNamespaceType => ServiceBusNamespaceApiVersion,
            _ => DefaultExistingResourceApiVersion,
        };
}