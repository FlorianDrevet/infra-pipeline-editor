using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using InfraFlowSculptor.Application.AppServicePlans.Common;
using InfraFlowSculptor.Application.AppServicePlans.Queries;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Application.ContainerApps.Queries.GetContainerApp;
using InfraFlowSculptor.Application.ContainerRegistries.Common;
using InfraFlowSculptor.Application.ContainerRegistries.Queries.GetContainerRegistry;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Application.KeyVaults.Queries;
using InfraFlowSculptor.Application.SqlDatabases.Common;
using InfraFlowSculptor.Application.SqlDatabases.Queries;
using InfraFlowSculptor.Application.SqlServers.Common;
using InfraFlowSculptor.Application.SqlServers.Queries;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Application.StorageAccounts.Queries;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.Mcp.Common;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides MCP tools for configuring per-environment settings on existing resources.
/// This enables post-creation setup of SKUs, capacity, replicas, etc. per environment.
/// </summary>
[McpServerToolType]
public sealed class ResourceConfigurationTools
{
    private ResourceConfigurationTools() { }

    /// <summary>
    /// Sets per-environment configuration for a resource (SKU, capacity, replicas, etc.).
    /// Fetches the current resource state, merges the environment settings, and updates.
    /// </summary>
    [McpServerTool(Name = "set_resource_environment_settings")]
    [Description(
        "Sets per-environment configuration for a resource. " +
        "Each entry in the environmentSettings array must include 'environmentName' plus the fields for that resource type. " +
        "Supported resource types: KeyVault (sku), ContainerApp (cpuCores, memoryGi, minReplicas, maxReplicas, ingressEnabled, ingressTargetPort, ingressExternal), " +
        "StorageAccount (sku), SqlServer (minimalTlsVersion), SqlDatabase (sku, maxSizeGb, zoneRedundant), " +
        "ContainerRegistry (sku, adminUserEnabled, publicNetworkAccess, zoneRedundancy), AppServicePlan (sku, capacity). " +
        "Only non-null fields are applied as overrides.")]
    public static async Task<string> SetResourceEnvironmentSettings(
        ISender mediator,
        [Description("The resource ID (GUID).")] string resourceId,
        [Description("The resource type (e.g. 'KeyVault', 'ContainerApp', 'SqlDatabase').")] string resourceType,
        [Description("JSON array of per-environment config entries. Each entry must have 'environmentName' plus type-specific fields.")] string environmentSettings)
    {
        if (!Guid.TryParse(resourceId, out var id))
        {
            return McpJsonDefaults.Error("invalid_resource_id", "The resourceId must be a valid GUID.");
        }

        var azureResourceId = AzureResourceId.Create(id);

        return resourceType switch
        {
            AzureResourceTypes.KeyVault => await UpdateKeyVaultAsync(mediator, azureResourceId, environmentSettings),
            AzureResourceTypes.ContainerApp => await UpdateContainerAppAsync(mediator, azureResourceId, environmentSettings),
            AzureResourceTypes.StorageAccount => await UpdateStorageAccountAsync(mediator, azureResourceId, environmentSettings),
            AzureResourceTypes.SqlServer => await UpdateSqlServerAsync(mediator, azureResourceId, environmentSettings),
            AzureResourceTypes.SqlDatabase => await UpdateSqlDatabaseAsync(mediator, azureResourceId, environmentSettings),
            AzureResourceTypes.ContainerRegistry => await UpdateContainerRegistryAsync(mediator, azureResourceId, environmentSettings),
            AzureResourceTypes.AppServicePlan => await UpdateAppServicePlanAsync(mediator, azureResourceId, environmentSettings),
            _ => McpJsonDefaults.Error("unsupported_resource_type", $"Resource type '{resourceType}' does not support environment settings via this tool."),
        };
    }

    private static async Task<string> UpdateKeyVaultAsync(ISender mediator, AzureResourceId id, string settingsJson)
    {
        var current = await mediator.Send(new GetKeyVaultQuery(id));
        if (current.IsError)
            return McpJsonDefaults.Error("resource_not_found", string.Join("; ", current.Errors.Select(e => e.Description)));

        var settings = DeserializeSettings<KeyVaultEnvEntry>(settingsJson);
        if (settings is null)
            return McpJsonDefaults.Error("invalid_settings", "Failed to parse environmentSettings JSON array.");

        var envConfigData = settings.Select(s => new KeyVaultEnvironmentConfigData(s.EnvironmentName, s.Sku)).ToList();

        var command = new Application.KeyVaults.Commands.UpdateKeyVault.UpdateKeyVaultCommand(
            id, current.Value.Name, current.Value.Location,
            current.Value.EnableRbacAuthorization, current.Value.EnabledForDeployment,
            current.Value.EnabledForDiskEncryption, current.Value.EnabledForTemplateDeployment,
            current.Value.EnablePurgeProtection, current.Value.EnableSoftDelete,
            envConfigData);

        var result = await mediator.Send(command);
        return result.Match(
            _ => SuccessResponse("KeyVault", envConfigData.Count),
            errors => McpJsonDefaults.Error("update_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    private static async Task<string> UpdateContainerAppAsync(ISender mediator, AzureResourceId id, string settingsJson)
    {
        var current = await mediator.Send(new GetContainerAppQuery(id));
        if (current.IsError)
            return McpJsonDefaults.Error("resource_not_found", string.Join("; ", current.Errors.Select(e => e.Description)));

        var settings = DeserializeSettings<ContainerAppEnvEntry>(settingsJson);
        if (settings is null)
            return McpJsonDefaults.Error("invalid_settings", "Failed to parse environmentSettings JSON array.");

        var envConfigData = settings.Select(s => new ContainerAppEnvironmentConfigData(
            s.EnvironmentName, s.CpuCores, s.MemoryGi, s.MinReplicas, s.MaxReplicas,
            s.IngressEnabled, s.IngressTargetPort, s.IngressExternal, s.TransportMethod,
            s.ReadinessProbePath, s.ReadinessProbePort,
            s.LivenessProbePath, s.LivenessProbePort,
            s.StartupProbePath, s.StartupProbePort)).ToList();

        var command = new Application.ContainerApps.Commands.UpdateContainerApp.UpdateContainerAppCommand(
            id, current.Value.Name, current.Value.Location,
            current.Value.ContainerAppEnvironmentId,
            current.Value.ContainerRegistryId,
            current.Value.AcrAuthMode,
            current.Value.DockerImageName,
            current.Value.DockerfilePath,
            current.Value.ApplicationName,
            envConfigData);

        var result = await mediator.Send(command);
        return result.Match(
            _ => SuccessResponse("ContainerApp", envConfigData.Count),
            errors => McpJsonDefaults.Error("update_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    private static async Task<string> UpdateStorageAccountAsync(ISender mediator, AzureResourceId id, string settingsJson)
    {
        var current = await mediator.Send(new GetStorageAccountQuery(id));
        if (current.IsError)
            return McpJsonDefaults.Error("resource_not_found", string.Join("; ", current.Errors.Select(e => e.Description)));

        var settings = DeserializeSettings<StorageAccountEnvEntry>(settingsJson);
        if (settings is null)
            return McpJsonDefaults.Error("invalid_settings", "Failed to parse environmentSettings JSON array.");

        var envConfigData = settings.Select(s => new StorageAccountEnvironmentConfigData(s.EnvironmentName, s.Sku)).ToList();

        var command = new Application.StorageAccounts.Commands.UpdateStorageAccount.UpdateStorageAccountCommand(
            id, current.Value.Name, current.Value.Location,
            current.Value.Kind, current.Value.AccessTier,
            current.Value.AllowBlobPublicAccess, current.Value.EnableHttpsTrafficOnly,
            current.Value.MinimumTlsVersion,
            envConfigData);

        var result = await mediator.Send(command);
        return result.Match(
            _ => SuccessResponse("StorageAccount", envConfigData.Count),
            errors => McpJsonDefaults.Error("update_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    private static async Task<string> UpdateSqlServerAsync(ISender mediator, AzureResourceId id, string settingsJson)
    {
        var current = await mediator.Send(new GetSqlServerQuery(id));
        if (current.IsError)
            return McpJsonDefaults.Error("resource_not_found", string.Join("; ", current.Errors.Select(e => e.Description)));

        var settings = DeserializeSettings<SqlServerEnvEntry>(settingsJson);
        if (settings is null)
            return McpJsonDefaults.Error("invalid_settings", "Failed to parse environmentSettings JSON array.");

        var envConfigData = settings.Select(s => new SqlServerEnvironmentConfigData(s.EnvironmentName, s.MinimalTlsVersion)).ToList();

        var command = new Application.SqlServers.Commands.UpdateSqlServer.UpdateSqlServerCommand(
            id, current.Value.Name, current.Value.Location,
            current.Value.Version, current.Value.AdministratorLogin,
            envConfigData);

        var result = await mediator.Send(command);
        return result.Match(
            _ => SuccessResponse("SqlServer", envConfigData.Count),
            errors => McpJsonDefaults.Error("update_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    private static async Task<string> UpdateSqlDatabaseAsync(ISender mediator, AzureResourceId id, string settingsJson)
    {
        var current = await mediator.Send(new GetSqlDatabaseQuery(id));
        if (current.IsError)
            return McpJsonDefaults.Error("resource_not_found", string.Join("; ", current.Errors.Select(e => e.Description)));

        var settings = DeserializeSettings<SqlDatabaseEnvEntry>(settingsJson);
        if (settings is null)
            return McpJsonDefaults.Error("invalid_settings", "Failed to parse environmentSettings JSON array.");

        var envConfigData = settings.Select(s => new SqlDatabaseEnvironmentConfigData(s.EnvironmentName, s.Sku, s.MaxSizeGb, s.ZoneRedundant)).ToList();

        var command = new Application.SqlDatabases.Commands.UpdateSqlDatabase.UpdateSqlDatabaseCommand(
            id, current.Value.Name, current.Value.Location,
            current.Value.SqlServerId,
            current.Value.Collation,
            envConfigData);

        var result = await mediator.Send(command);
        return result.Match(
            _ => SuccessResponse("SqlDatabase", envConfigData.Count),
            errors => McpJsonDefaults.Error("update_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    private static async Task<string> UpdateContainerRegistryAsync(ISender mediator, AzureResourceId id, string settingsJson)
    {
        var current = await mediator.Send(new GetContainerRegistryQuery(id));
        if (current.IsError)
            return McpJsonDefaults.Error("resource_not_found", string.Join("; ", current.Errors.Select(e => e.Description)));

        var settings = DeserializeSettings<ContainerRegistryEnvEntry>(settingsJson);
        if (settings is null)
            return McpJsonDefaults.Error("invalid_settings", "Failed to parse environmentSettings JSON array.");

        var envConfigData = settings.Select(s => new ContainerRegistryEnvironmentConfigData(
            s.EnvironmentName, s.Sku, s.AdminUserEnabled, s.PublicNetworkAccess, s.ZoneRedundancy)).ToList();

        var command = new Application.ContainerRegistries.Commands.UpdateContainerRegistry.UpdateContainerRegistryCommand(
            id, current.Value.Name, current.Value.Location,
            envConfigData);

        var result = await mediator.Send(command);
        return result.Match(
            _ => SuccessResponse("ContainerRegistry", envConfigData.Count),
            errors => McpJsonDefaults.Error("update_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    private static async Task<string> UpdateAppServicePlanAsync(ISender mediator, AzureResourceId id, string settingsJson)
    {
        var current = await mediator.Send(new GetAppServicePlanQuery(id));
        if (current.IsError)
            return McpJsonDefaults.Error("resource_not_found", string.Join("; ", current.Errors.Select(e => e.Description)));

        var settings = DeserializeSettings<AppServicePlanEnvEntry>(settingsJson);
        if (settings is null)
            return McpJsonDefaults.Error("invalid_settings", "Failed to parse environmentSettings JSON array.");

        var envConfigData = settings.Select(s => new AppServicePlanEnvironmentConfigData(s.EnvironmentName, s.Sku, s.Capacity)).ToList();

        var command = new Application.AppServicePlans.Commands.UpdateAppServicePlan.UpdateAppServicePlanCommand(
            id, current.Value.Name, current.Value.Location,
            current.Value.OsType,
            envConfigData);

        var result = await mediator.Send(command);
        return result.Match(
            _ => SuccessResponse("AppServicePlan", envConfigData.Count),
            errors => McpJsonDefaults.Error("update_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    private static string SuccessResponse(string resourceType, int envCount)
    {
        return JsonSerializer.Serialize(new
        {
            status = "success",
            message = $"Environment settings updated for {resourceType} ({envCount} environment(s) configured).",
        }, McpJsonDefaults.SerializerOptions);
    }

    private static List<T>? DeserializeSettings<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<T>>(json, EnvSettingsSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static readonly JsonSerializerOptions EnvSettingsSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // ── Per-resource-type JSON entry models ────────────────────────────

    private sealed record KeyVaultEnvEntry(string EnvironmentName, string? Sku);
    private sealed record ContainerAppEnvEntry(
        string EnvironmentName, string? CpuCores, string? MemoryGi,
        int? MinReplicas, int? MaxReplicas,
        bool? IngressEnabled, int? IngressTargetPort, bool? IngressExternal, string? TransportMethod,
        string? ReadinessProbePath = null, int? ReadinessProbePort = null,
        string? LivenessProbePath = null, int? LivenessProbePort = null,
        string? StartupProbePath = null, int? StartupProbePort = null);
    private sealed record StorageAccountEnvEntry(string EnvironmentName, string? Sku);
    private sealed record SqlServerEnvEntry(string EnvironmentName, string? MinimalTlsVersion);
    private sealed record SqlDatabaseEnvEntry(string EnvironmentName, string? Sku, int? MaxSizeGb, bool? ZoneRedundant);
    private sealed record ContainerRegistryEnvEntry(string EnvironmentName, string? Sku, bool? AdminUserEnabled, string? PublicNetworkAccess, bool? ZoneRedundancy);
    private sealed record AppServicePlanEnvEntry(string EnvironmentName, string? Sku, int? Capacity);
}
