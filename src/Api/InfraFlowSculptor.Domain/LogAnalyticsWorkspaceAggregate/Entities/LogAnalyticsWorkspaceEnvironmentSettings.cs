using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for a <see cref="LogAnalyticsWorkspace"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class LogAnalyticsWorkspaceEnvironmentSettings : Entity<LogAnalyticsWorkspaceEnvironmentSettingsId>
{
    /// <summary>Gets the parent Log Analytics Workspace identifier.</summary>
    public AzureResourceId LogAnalyticsWorkspaceId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the pricing tier override (e.g., "Free", "PerGB2018", "PerNode", "Premium").</summary>
    public string? Sku { get; private set; }

    /// <summary>Gets or sets the data retention in days (30-730).</summary>
    public int? RetentionInDays { get; private set; }

    /// <summary>Gets or sets the daily ingestion cap in GB (-1 = unlimited).</summary>
    public decimal? DailyQuotaGb { get; private set; }

    private LogAnalyticsWorkspaceEnvironmentSettings() { }

    internal LogAnalyticsWorkspaceEnvironmentSettings(
        AzureResourceId logAnalyticsWorkspaceId,
        string environmentName,
        string? sku,
        int? retentionInDays,
        decimal? dailyQuotaGb)
        : base(LogAnalyticsWorkspaceEnvironmentSettingsId.CreateUnique())
    {
        LogAnalyticsWorkspaceId = logAnalyticsWorkspaceId;
        EnvironmentName = environmentName;
        Sku = sku;
        RetentionInDays = retentionInDays;
        DailyQuotaGb = dailyQuotaGb;
    }

    /// <summary>
    /// Creates a new <see cref="LogAnalyticsWorkspaceEnvironmentSettings"/> for the specified workspace and environment.
    /// </summary>
    public static LogAnalyticsWorkspaceEnvironmentSettings Create(
        AzureResourceId logAnalyticsWorkspaceId,
        string environmentName,
        string? sku,
        int? retentionInDays,
        decimal? dailyQuotaGb)
        => new(logAnalyticsWorkspaceId, environmentName, sku, retentionInDays, dailyQuotaGb);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(
        string? sku,
        int? retentionInDays,
        decimal? dailyQuotaGb)
    {
        Sku = sku;
        RetentionInDays = retentionInDays;
        DailyQuotaGb = dailyQuotaGb;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (Sku is not null) dict["sku"] = Sku;
        if (RetentionInDays is not null) dict["retentionInDays"] = RetentionInDays.Value.ToString();
        if (DailyQuotaGb is not null) dict["dailyQuotaGb"] = DailyQuotaGb.Value.ToString();
        return dict;
    }
}
