using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.AppServicePlans.Requests;

/// <summary>Common properties shared by create and update App Service Plan requests.</summary>
public abstract class AppServicePlanRequestBase
{
    /// <summary>Display name for the App Service Plan resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the App Service Plan will be deployed.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Operating system type (Windows or Linux).</summary>
    [Required, EnumValidation(typeof(AppServicePlanOsType.AppServicePlanOsTypeEnum))]
    public required string OsType { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<AppServicePlanEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for an App Service Plan.</summary>
public class AppServicePlanEnvironmentConfigEntry
{
    /// <summary>Name of the target environment (e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Optional pricing tier override (e.g., "S1", "P1v3").</summary>
    [EnumValidation(typeof(AppServicePlanSku.AppServicePlanSkuEnum))]
    public string? Sku { get; init; }

    /// <summary>Optional number of instances for this environment.</summary>
    public int? Capacity { get; init; }
}

/// <summary>Response DTO for a typed per-environment App Service Plan configuration.</summary>
public record AppServicePlanEnvironmentConfigResponse(
    string EnvironmentName,
    string? Sku,
    int? Capacity);
