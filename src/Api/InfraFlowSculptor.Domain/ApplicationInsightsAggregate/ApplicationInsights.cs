using InfraFlowSculptor.Domain.ApplicationInsightsAggregate.Entities;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ApplicationInsightsAggregate;

/// <summary>
/// Represents an Azure Application Insights resource aggregate root.
/// </summary>
public class ApplicationInsights : AzureResource
{
    private readonly List<ApplicationInsightsEnvironmentSettings> _environmentSettings = [];

    /// <summary>Gets the typed per-environment configuration overrides for this Application Insights resource.</summary>
    public IReadOnlyCollection<ApplicationInsightsEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    /// <summary>Gets the identifier of the Log Analytics Workspace linked to this Application Insights resource.</summary>
    public AzureResourceId LogAnalyticsWorkspaceId { get; private set; } = null!;

    /// <inheritdoc />
    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        Array.Empty<ParameterUsage>();

    private ApplicationInsights()
    {
    }

    /// <summary>
    /// Updates the mutable properties of this Application Insights resource.
    /// </summary>
    /// <param name="name">The new display name.</param>
    /// <param name="location">The new Azure region.</param>
    /// <param name="logAnalyticsWorkspaceId">The identifier of the linked Log Analytics Workspace.</param>
    public void Update(Name name, Location location, AzureResourceId logAnalyticsWorkspaceId)
    {
        Name = name;
        Location = location;
        LogAnalyticsWorkspaceId = logAnalyticsWorkspaceId;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        decimal? samplingPercentage,
        int? retentionInDays,
        bool? disableIpMasking,
        bool? disableLocalAuth,
        string? ingestionMode)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(samplingPercentage, retentionInDays, disableIpMasking, disableLocalAuth, ingestionMode);
        }
        else
        {
            _environmentSettings.Add(
                ApplicationInsightsEnvironmentSettings.Create(
                    Id, environmentName, samplingPercentage, retentionInDays, disableIpMasking, disableLocalAuth, ingestionMode));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, decimal? SamplingPercentage, int? RetentionInDays, bool? DisableIpMasking, bool? DisableLocalAuth, string? IngestionMode)> settings)
    {
        _environmentSettings.Clear();
        foreach (var (envName, samplingPercentage, retentionInDays, disableIpMasking, disableLocalAuth, ingestionMode) in settings)
        {
            _environmentSettings.Add(
                ApplicationInsightsEnvironmentSettings.Create(
                    Id, envName, samplingPercentage, retentionInDays, disableIpMasking, disableLocalAuth, ingestionMode));
        }
    }

    /// <summary>
    /// Creates a new <see cref="ApplicationInsights"/> instance with a generated identifier.
    /// </summary>
    /// <param name="resourceGroupId">The parent resource group identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="location">The Azure region.</param>
    /// <param name="logAnalyticsWorkspaceId">The identifier of the linked Log Analytics Workspace.</param>
    /// <param name="environmentSettings">Optional per-environment configuration overrides.</param>
    /// <returns>A new <see cref="ApplicationInsights"/> aggregate root.</returns>
    public static ApplicationInsights Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        AzureResourceId logAnalyticsWorkspaceId,
        IReadOnlyList<(string EnvironmentName, decimal? SamplingPercentage, int? RetentionInDays, bool? DisableIpMasking, bool? DisableLocalAuth, string? IngestionMode)>? environmentSettings = null)
    {
        var applicationInsights = new ApplicationInsights
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            LogAnalyticsWorkspaceId = logAnalyticsWorkspaceId
        };

        if (environmentSettings is not null)
            applicationInsights.SetAllEnvironmentSettings(environmentSettings);

        return applicationInsights;
    }
}
