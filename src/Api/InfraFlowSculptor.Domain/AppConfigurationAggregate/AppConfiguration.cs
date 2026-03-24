using InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.AppConfigurationAggregate;

/// <summary>
/// Represents an Azure App Configuration resource aggregate root.
/// </summary>
public class AppConfiguration : AzureResource
{
    private readonly List<AppConfigurationEnvironmentSettings> _environmentSettings = new();

    /// <summary>Gets the typed per-environment configuration overrides for this App Configuration.</summary>
    public IReadOnlyCollection<AppConfigurationEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

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
