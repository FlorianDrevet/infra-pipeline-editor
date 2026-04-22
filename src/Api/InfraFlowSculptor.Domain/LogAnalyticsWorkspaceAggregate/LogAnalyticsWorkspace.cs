using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate.Entities;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;

/// <summary>
/// Represents an Azure Log Analytics Workspace resource aggregate root.
/// </summary>
public sealed class LogAnalyticsWorkspace : AzureResource
{
    private readonly List<LogAnalyticsWorkspaceEnvironmentSettings> _environmentSettings = [];

    /// <summary>Gets the typed per-environment configuration overrides for this Log Analytics Workspace.</summary>
    public IReadOnlyCollection<LogAnalyticsWorkspaceEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    /// <inheritdoc />
    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        Array.Empty<ParameterUsage>();

    private LogAnalyticsWorkspace()
    {
    }

    /// <summary>
    /// Updates the mutable properties of this Log Analytics Workspace resource.
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
    /// <exception cref="InvalidOperationException">Thrown when the resource is an existing (pre-deployed) resource.</exception>
    public void SetEnvironmentSettings(
        string environmentName,
        string? sku,
        int? retentionInDays,
        decimal? dailyQuotaGb)
    {
        if (IsExisting)
            return;
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(sku, retentionInDays, dailyQuotaGb);
        }
        else
        {
            _environmentSettings.Add(
                LogAnalyticsWorkspaceEnvironmentSettings.Create(
                    Id, environmentName, sku, retentionInDays, dailyQuotaGb));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the resource is an existing (pre-deployed) resource.</exception>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, string? Sku, int? RetentionInDays, decimal? DailyQuotaGb)> settings)
    {
        if (IsExisting)
            return;

        _environmentSettings.Clear();
        foreach (var (envName, sku, retentionInDays, dailyQuotaGb) in settings)
        {
            _environmentSettings.Add(
                LogAnalyticsWorkspaceEnvironmentSettings.Create(
                    Id, envName, sku, retentionInDays, dailyQuotaGb));
        }
    }

    /// <summary>
    /// Creates a new <see cref="LogAnalyticsWorkspace"/> instance with a generated identifier.
    /// </summary>
    /// <param name="resourceGroupId">The parent resource group identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="location">The Azure region.</param>
    /// <param name="environmentSettings">Optional per-environment configuration overrides.</param>
    /// <param name="isExisting">When <c>true</c>, this resource already exists in Azure and is not deployed by this project.</param>
    /// <returns>A new <see cref="LogAnalyticsWorkspace"/> aggregate root.</returns>
    public static LogAnalyticsWorkspace Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyList<(string EnvironmentName, string? Sku, int? RetentionInDays, decimal? DailyQuotaGb)>? environmentSettings = null,
        bool isExisting = false)
    {
        var logAnalyticsWorkspace = new LogAnalyticsWorkspace
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            IsExisting = isExisting
        };

        if (!isExisting && environmentSettings is not null)
            logAnalyticsWorkspace.SetAllEnvironmentSettings(environmentSettings);

        return logAnalyticsWorkspace;
    }
}
