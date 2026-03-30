using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;

/// <summary>
/// Represents a Bicep parameter declared in an infrastructure configuration.
/// Parameters can be typed, flagged as secret, and optionally carry a default value.
/// </summary>
public sealed class ParameterDefinition : Entity<ParameterDefinitionId>
{
    /// <summary>Gets the owning infrastructure configuration identifier.</summary>
    public InfrastructureConfigId InfraConfigId { get; private set; }

    /// <summary>Gets the parameter display name.</summary>
    public Name Name { get; private set; }

    /// <summary>Gets the Bicep type of this parameter.</summary>
    public ParameterType Type { get; private set; }

    /// <summary>Gets whether this parameter is a secret (stored in Key Vault).</summary>
    public IsSecret IsSecret { get; private set; }

    /// <summary>Gets the optional default value for this parameter.</summary>
    public string? DefaultValue { get; private set; }

    private ParameterDefinition(ParameterDefinitionId id)
        : base(id)
    {
    }

    /// <summary>Creates a new <see cref="ParameterDefinition"/> with a generated identifier.</summary>
    public static ParameterDefinition Create()
    {
        return new ParameterDefinition(ParameterDefinitionId.CreateUnique());
    }

    private ParameterDefinition()
    {
    }
}