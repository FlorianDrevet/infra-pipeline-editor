using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;

/// <summary>
/// Represents a configuration-level override for a resource type abbreviation
/// used in the <c>{resourceAbbr}</c> naming-template placeholder.
/// </summary>
public sealed class ResourceAbbreviationOverride : Entity<ResourceAbbreviationOverrideId>
{
    /// <summary>Gets the identifier of the parent infrastructure configuration.</summary>
    public InfrastructureConfigId InfraConfigId { get; private set; } = null!;

    /// <summary>Gets the parent infrastructure configuration navigation property.</summary>
    public InfrastructureConfig InfraConfig { get; private set; } = null!;

    /// <summary>Gets the Azure resource type this abbreviation applies to.</summary>
    public string ResourceType { get; private set; } = null!;

    /// <summary>Gets the custom abbreviation value.</summary>
    public string Abbreviation { get; private set; } = null!;

    /// <summary>EF Core constructor.</summary>
    private ResourceAbbreviationOverride() { }

    /// <summary>
    /// Creates a new <see cref="ResourceAbbreviationOverride"/> for the specified resource type.
    /// </summary>
    internal ResourceAbbreviationOverride(InfrastructureConfigId infraConfigId, string resourceType, string abbreviation)
        : base(ResourceAbbreviationOverrideId.CreateUnique())
    {
        InfraConfigId = infraConfigId;
        ResourceType = resourceType;
        Abbreviation = abbreviation;
    }

    /// <summary>Updates the abbreviation value.</summary>
    internal void Update(string abbreviation) => Abbreviation = abbreviation;
}
