using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.ApplicationInsights.Requests;

/// <summary>Common properties shared by create and update Application Insights requests.</summary>
public abstract class ApplicationInsightsRequestBase
{
    /// <summary>Display name for the Application Insights resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Application Insights resource will be deployed.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Identifier of the Log Analytics Workspace linked to this Application Insights.</summary>
    [Required, GuidValidation]
    public required Guid LogAnalyticsWorkspaceId { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<ApplicationInsightsEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for Application Insights.</summary>
public class ApplicationInsightsEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional adaptive sampling rate (0-100).</summary>
    public decimal? SamplingPercentage { get; init; }

    /// <summary>Optional data retention in days (30, 60, 90, 120, 180, 270, 365, 550, 730).</summary>
    public int? RetentionInDays { get; init; }

    /// <summary>Optional flag to disable IP masking.</summary>
    public bool? DisableIpMasking { get; init; }

    /// <summary>Optional flag to disable local authentication.</summary>
    public bool? DisableLocalAuth { get; init; }

    /// <summary>Optional ingestion mode (e.g., "ApplicationInsights", "LogAnalytics", "ApplicationInsightsWithDiagnosticSettings").</summary>
    public string? IngestionMode { get; init; }
}

/// <summary>Response DTO for a typed per-environment Application Insights configuration.</summary>
public record ApplicationInsightsEnvironmentConfigResponse(
    string EnvironmentName,
    decimal? SamplingPercentage,
    int? RetentionInDays,
    bool? DisableIpMasking,
    bool? DisableLocalAuth,
    string? IngestionMode);
