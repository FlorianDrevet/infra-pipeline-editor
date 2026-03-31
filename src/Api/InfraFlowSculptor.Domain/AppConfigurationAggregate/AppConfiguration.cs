using InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.AppConfigurationAggregate;

/// <summary>
/// Represents an Azure App Configuration resource aggregate root.
/// </summary>
public class AppConfiguration : AzureResource
{
    private readonly List<AppConfigurationEnvironmentSettings> _environmentSettings = [];
    private readonly List<AppConfigurationKey> _configurationKeys = [];

    /// <summary>Gets the typed per-environment configuration overrides for this App Configuration.</summary>
    public IReadOnlyCollection<AppConfigurationEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    /// <summary>Gets the key-value configuration entries for this App Configuration store.</summary>
    public IReadOnlyCollection<AppConfigurationKey> ConfigurationKeys => _configurationKeys.AsReadOnly();

    /// <inheritdoc />
    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        new[]
        {
            ParameterUsage.Secret
        };

    private AppConfiguration()
    {
    }

    /// <summary>
    /// Updates the mutable properties of this App Configuration resource.
    /// </summary>
    /// <param name="name">The new display name.</param>
    /// <param name="location">The new Azure region.</param>
    public void Update(Name name, Location location)
    {
        Name = name;
        Location = location;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        string? sku,
        int? softDeleteRetentionInDays,
        bool? purgeProtectionEnabled,
        bool? disableLocalAuth,
        string? publicNetworkAccess)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(sku, softDeleteRetentionInDays, purgeProtectionEnabled, disableLocalAuth, publicNetworkAccess);
        }
        else
        {
            _environmentSettings.Add(
                AppConfigurationEnvironmentSettings.Create(
                    Id, environmentName, sku, softDeleteRetentionInDays, purgeProtectionEnabled, disableLocalAuth, publicNetworkAccess));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, string? Sku, int? SoftDeleteRetentionInDays, bool? PurgeProtectionEnabled, bool? DisableLocalAuth, string? PublicNetworkAccess)> settings)
    {
        _environmentSettings.Clear();
        foreach (var (envName, sku, softDeleteRetentionInDays, purgeProtectionEnabled, disableLocalAuth, publicNetworkAccess) in settings)
        {
            _environmentSettings.Add(
                AppConfigurationEnvironmentSettings.Create(
                    Id, envName, sku, softDeleteRetentionInDays, purgeProtectionEnabled, disableLocalAuth, publicNetworkAccess));
        }
    }

    /// <summary>Adds a static configuration key with per-environment values.</summary>
    /// <param name="key">The configuration key name.</param>
    /// <param name="label">Optional label for the key.</param>
    /// <param name="environmentValues">A dictionary of environment name → value.</param>
    /// <returns>The created <see cref="AppConfigurationKey"/>.</returns>
    public AppConfigurationKey AddStaticConfigurationKey(
        string key,
        string? label,
        IReadOnlyDictionary<string, string> environmentValues)
    {
        var configKey = AppConfigurationKey.CreateStatic(Id, key, label, environmentValues);
        _configurationKeys.Add(configKey);
        return configKey;
    }

    /// <summary>Adds a configuration key that references a Key Vault secret.</summary>
    /// <param name="key">The configuration key name.</param>
    /// <param name="label">Optional label for the key.</param>
    /// <param name="keyVaultResourceId">The Key Vault resource identifier.</param>
    /// <param name="secretName">The secret name in the Key Vault.</param>
    /// <param name="assignment">Determines how the secret value is assigned.</param>
    /// <returns>The created <see cref="AppConfigurationKey"/>.</returns>
    public AppConfigurationKey AddKeyVaultReferenceConfigurationKey(
        string key,
        string? label,
        AzureResourceId keyVaultResourceId,
        string secretName,
        SecretValueAssignment assignment)
    {
        var configKey = AppConfigurationKey.CreateKeyVaultReference(
            Id, key, label, keyVaultResourceId, secretName, assignment);
        _configurationKeys.Add(configKey);
        return configKey;
    }

    /// <summary>Adds a configuration key whose value comes from a pipeline variable group.</summary>
    /// <param name="key">The configuration key name.</param>
    /// <param name="label">Optional label for the key.</param>
    /// <param name="variableGroupId">The pipeline variable group identifier.</param>
    /// <param name="pipelineVariableName">The pipeline variable name within the group.</param>
    /// <returns>The created <see cref="AppConfigurationKey"/>.</returns>
    public AppConfigurationKey AddViaVariableGroupConfigurationKey(
        string key,
        string? label,
        ProjectPipelineVariableGroupId variableGroupId,
        string pipelineVariableName)
    {
        var configKey = AppConfigurationKey.CreateViaVariableGroup(
            Id, key, label, variableGroupId, pipelineVariableName);
        _configurationKeys.Add(configKey);
        return configKey;
    }

    /// <summary>Adds a configuration key whose value comes from a pipeline variable group and is stored as a Key Vault secret.</summary>
    /// <param name="key">The configuration key name.</param>
    /// <param name="label">Optional label for the key.</param>
    /// <param name="variableGroupId">The pipeline variable group identifier.</param>
    /// <param name="pipelineVariableName">The pipeline variable name within the group.</param>
    /// <param name="keyVaultResourceId">The Key Vault resource identifier.</param>
    /// <param name="secretName">The secret name in the Key Vault.</param>
    /// <param name="assignment">Determines how the secret value is assigned.</param>
    /// <returns>The created <see cref="AppConfigurationKey"/>.</returns>
    public AppConfigurationKey AddViaVariableGroupKeyVaultReferenceConfigurationKey(
        string key,
        string? label,
        ProjectPipelineVariableGroupId variableGroupId,
        string pipelineVariableName,
        AzureResourceId keyVaultResourceId,
        string secretName,
        SecretValueAssignment assignment)
    {
        var configKey = AppConfigurationKey.CreateViaVariableGroupKeyVaultReference(
            Id, key, label, variableGroupId, pipelineVariableName,
            keyVaultResourceId, secretName, assignment);
        _configurationKeys.Add(configKey);
        return configKey;
    }

    /// <summary>Adds a configuration key that references an output from a sibling resource.</summary>
    /// <param name="key">The configuration key name.</param>
    /// <param name="label">Optional label for the key.</param>
    /// <param name="sourceResourceId">The source resource identifier.</param>
    /// <param name="sourceOutputName">The output name on the source resource.</param>
    /// <returns>The created <see cref="AppConfigurationKey"/>.</returns>
    public AppConfigurationKey AddOutputReferenceConfigurationKey(
        string key,
        string? label,
        AzureResourceId sourceResourceId,
        string sourceOutputName)
    {
        var configKey = AppConfigurationKey.CreateOutputReference(
            Id, key, label, sourceResourceId, sourceOutputName);
        _configurationKeys.Add(configKey);
        return configKey;
    }

    /// <summary>Adds a configuration key for a sensitive output exported as a Key Vault secret.</summary>
    /// <param name="key">The configuration key name.</param>
    /// <param name="label">Optional label for the key.</param>
    /// <param name="sourceResourceId">The source resource identifier.</param>
    /// <param name="sourceOutputName">The output name on the source resource.</param>
    /// <param name="keyVaultResourceId">The Key Vault resource identifier.</param>
    /// <param name="secretName">The secret name in the Key Vault.</param>
    /// <returns>The created <see cref="AppConfigurationKey"/>.</returns>
    public AppConfigurationKey AddSensitiveOutputKeyVaultReferenceConfigurationKey(
        string key,
        string? label,
        AzureResourceId sourceResourceId,
        string sourceOutputName,
        AzureResourceId keyVaultResourceId,
        string secretName)
    {
        var configKey = AppConfigurationKey.CreateSensitiveOutputKeyVaultReference(
            Id, key, label, sourceResourceId, sourceOutputName,
            keyVaultResourceId, secretName);
        _configurationKeys.Add(configKey);
        return configKey;
    }

    /// <summary>Removes a configuration key from this App Configuration.</summary>
    /// <param name="configurationKeyId">The identifier of the configuration key to remove.</param>
    public void RemoveConfigurationKey(AppConfigurationKeyId configurationKeyId)
    {
        var configKey = _configurationKeys.FirstOrDefault(k => k.Id == configurationKeyId);
        if (configKey is not null)
            _configurationKeys.Remove(configKey);
    }

    /// <summary>
    /// Creates a new <see cref="AppConfiguration"/> instance with a generated identifier.
    /// </summary>
    /// <param name="resourceGroupId">The parent resource group identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="location">The Azure region.</param>
    /// <param name="environmentSettings">Optional per-environment configuration overrides.</param>
    /// <returns>A new <see cref="AppConfiguration"/> aggregate root.</returns>
    public static AppConfiguration Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyList<(string EnvironmentName, string? Sku, int? SoftDeleteRetentionInDays, bool? PurgeProtectionEnabled, bool? DisableLocalAuth, string? PublicNetworkAccess)>? environmentSettings = null)
    {
        var appConfiguration = new AppConfiguration
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location
        };

        if (environmentSettings is not null)
            appConfiguration.SetAllEnvironmentSettings(environmentSettings);

        return appConfiguration;
    }
}
