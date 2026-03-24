using InfraFlowSculptor.Domain.ApplicationInsightsAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ApplicationInsightsAggregate.Entities;

/// <summary>
/// Typed per-environment configuration overrides for an <see cref="ApplicationInsights"/>.
/// Only non-null properties are applied as overrides for the target environment.
/// </summary>
public sealed class ApplicationInsightsEnvironmentSettings : Entity<ApplicationInsightsEnvironmentSettingsId>
{
    /// <summary>Gets the parent Application Insights identifier.</summary>
    public AzureResourceId ApplicationInsightsId { get; private set; } = null!;

    /// <summary>Gets the environment name this configuration applies to (e.g., "dev", "staging", "prod").</summary>
    public string EnvironmentName { get; private set; } = string.Empty;

    /// <summary>Gets or sets the adaptive sampling rate (0-100).</summary>
    public decimal? SamplingPercentage { get; private set; }

    /// <summary>Gets or sets the data retention in days (30, 60, 90, 120, 180, 270, 365, 550, 730).</summary>
    public int? RetentionInDays { get; private set; }

    /// <summary>Gets or sets whether IP masking is disabled.</summary>
    public bool? DisableIpMasking { get; private set; }

    /// <summary>Gets or sets whether local authentication is disabled.</summary>
    public bool? DisableLocalAuth { get; private set; }

    /// <summary>Gets or sets the ingestion mode (e.g., "ApplicationInsights", "LogAnalytics").</summary>
    public string? IngestionMode { get; private set; }

    private ApplicationInsightsEnvironmentSettings() { }

    internal ApplicationInsightsEnvironmentSettings(
        AzureResourceId applicationInsightsId,
        string environmentName,
        decimal? samplingPercentage,
        int? retentionInDays,
        bool? disableIpMasking,
        bool? disableLocalAuth,
        string? ingestionMode)
        : base(ApplicationInsightsEnvironmentSettingsId.CreateUnique())
    {
        ApplicationInsightsId = applicationInsightsId;
        EnvironmentName = environmentName;
        SamplingPercentage = samplingPercentage;
        RetentionInDays = retentionInDays;
        DisableIpMasking = disableIpMasking;
        DisableLocalAuth = disableLocalAuth;
        IngestionMode = ingestionMode;
    }

    /// <summary>
    /// Creates a new <see cref="ApplicationInsightsEnvironmentSettings"/> for the specified Application Insights resource and environment.
    /// </summary>
    public static ApplicationInsightsEnvironmentSettings Create(
        AzureResourceId applicationInsightsId,
        string environmentName,
        decimal? samplingPercentage,
        int? retentionInDays,
        bool? disableIpMasking,
        bool? disableLocalAuth,
        string? ingestionMode)
        => new(applicationInsightsId, environmentName, samplingPercentage, retentionInDays, disableIpMasking, disableLocalAuth, ingestionMode);

    /// <summary>Updates the configuration overrides for this environment.</summary>
    public void Update(
        decimal? samplingPercentage,
        int? retentionInDays,
        bool? disableIpMasking,
        bool? disableLocalAuth,
        string? ingestionMode)
    {
        SamplingPercentage = samplingPercentage;
        RetentionInDays = retentionInDays;
        DisableIpMasking = disableIpMasking;
        DisableLocalAuth = disableLocalAuth;
        IngestionMode = ingestionMode;
    }

    /// <summary>
    /// Converts the non-null overrides to a dictionary for Bicep generation compatibility.
    /// </summary>
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (SamplingPercentage is not null) dict["samplingPercentage"] = SamplingPercentage.Value.ToString();
        if (RetentionInDays is not null) dict["retentionInDays"] = RetentionInDays.Value.ToString();
        if (DisableIpMasking is not null) dict["disableIpMasking"] = DisableIpMasking.Value.ToString().ToLower();
        if (DisableLocalAuth is not null) dict["disableLocalAuth"] = DisableLocalAuth.Value.ToString().ToLower();
        if (IngestionMode is not null) dict["ingestionMode"] = IngestionMode;
        return dict;
    }
}
