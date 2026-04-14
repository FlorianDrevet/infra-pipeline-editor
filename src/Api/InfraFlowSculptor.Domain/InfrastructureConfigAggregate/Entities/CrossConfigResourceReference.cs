using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;

/// <summary>
/// Represents a reference from this infrastructure configuration to a resource
/// belonging to a different infrastructure configuration within the same project.
/// Used to generate Bicep <c>existing</c> resource declarations for cross-configuration dependencies.
/// </summary>
public sealed class CrossConfigResourceReference : Entity<CrossConfigResourceReferenceId>
{
    /// <summary>Gets the identifier of the target infrastructure configuration that owns the referenced resource.</summary>
    public InfrastructureConfigId TargetConfigId { get; private set; } = null!;

    /// <summary>Gets the identifier of the referenced Azure resource in the target configuration.</summary>
    public AzureResourceId TargetResourceId { get; private set; } = null!;

    /// <summary>Gets the identifier of the infrastructure configuration that owns this reference.</summary>
    public InfrastructureConfigId InfraConfigId { get; private set; } = null!;

    private CrossConfigResourceReference() { }

    private CrossConfigResourceReference(
        CrossConfigResourceReferenceId id,
        InfrastructureConfigId infraConfigId,
        InfrastructureConfigId targetConfigId,
        AzureResourceId targetResourceId)
        : base(id)
    {
        InfraConfigId = infraConfigId;
        TargetConfigId = targetConfigId;
        TargetResourceId = targetResourceId;
    }

    /// <summary>
    /// Creates a new cross-configuration resource reference.
    /// </summary>
    /// <param name="infraConfigId">The owning infrastructure configuration.</param>
    /// <param name="targetConfigId">The target configuration that owns the referenced resource.</param>
    /// <param name="targetResourceId">The referenced Azure resource identifier.</param>
    /// <returns>A new <see cref="CrossConfigResourceReference"/> instance.</returns>
    public static CrossConfigResourceReference Create(
        InfrastructureConfigId infraConfigId,
        InfrastructureConfigId targetConfigId,
        AzureResourceId targetResourceId)
    {
        return new CrossConfigResourceReference(
            CrossConfigResourceReferenceId.CreateUnique(),
            infraConfigId,
            targetConfigId,
            targetResourceId);
    }
}
