using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FrontDoorAggregate.ValueObjects;

namespace InfraFlowSculptor.Contracts.FrontDoors.Requests;

/// <summary>Common properties shared by create and update Front Door requests.</summary>
public abstract class FrontDoorRequestBase
{
    /// <summary>Display name for the Front Door resource.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Azure region where the Front Door will be deployed.</summary>
    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }

    /// <summary>Whether WAF policy is enabled on this Front Door profile.</summary>
    public bool WafPolicyEnabled { get; init; }

    /// <summary>Per-environment typed configuration overrides.</summary>
    public List<FrontDoorEnvironmentConfigEntry>? EnvironmentSettings { get; init; }
}

/// <summary>Typed per-environment configuration entry for a Front Door.</summary>
public class FrontDoorEnvironmentConfigEntry
{
    /// <summary>Name of the target environment.</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Front Door pricing tier for this environment.</summary>
    [Required, EnumValidation(typeof(FrontDoorSku.SkuEnum))]
    public required string Sku { get; init; }
}
