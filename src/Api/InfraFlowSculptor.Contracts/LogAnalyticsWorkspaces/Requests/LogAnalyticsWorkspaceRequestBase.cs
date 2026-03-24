using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.LogAnalyticsWorkspaces.Requests;

/// <summary>Common properties shared by create and update Log Analytics Workspace requests.</summary>
public abstract class LogAnalyticsWorkspaceRequestBase
{
    /// <summary>Display name for the Log Analytics Workspace resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Log Analytics Workspace will be deployed (e.g. "westeurope").</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<LogAnalyticsWorkspaceEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a Log Analytics Workspace.</summary>
public class LogAnalyticsWorkspaceEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional pricing tier override (e.g., "Free", "PerGB2018", "PerNode", "Premium", "Standard", "Standalone", "CapacityReservation").</summary>
    public string? Sku { get; init; }

    /// <summary>Optional data retention in days (30-730).</summary>
    public int? RetentionInDays { get; init; }

    /// <summary>Optional daily ingestion cap in GB (-1 = unlimited).</summary>
    public decimal? DailyQuotaGb { get; init; }
}

/// <summary>Response DTO for a typed per-environment Log Analytics Workspace configuration.</summary>
public record LogAnalyticsWorkspaceEnvironmentConfigResponse(
    string EnvironmentName,
    string? Sku,
    int? RetentionInDays,
    decimal? DailyQuotaGb);
