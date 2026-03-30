using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;

namespace InfraFlowSculptor.Domain.Common.Models;

/// <summary>
/// Tracks how a <see cref="InfrastructureConfigAggregate.Entities.ParameterDefinition"/>
/// is consumed by a specific Azure resource (e.g. as a secret or app setting).
/// </summary>
public sealed class ResourceParameterUsage
    : Entity<ResourceParameterUsageId>
{
    /// <summary>Gets the Azure resource that consumes this parameter.</summary>
    public AzureResourceId ResourceId { get; private set; } = null!;

    /// <summary>Gets the parameter definition being consumed.</summary>
    public ParameterDefinitionId ParameterId { get; private set; } = null!;

    /// <summary>Gets the purpose of this parameter usage (secret, appSetting, connectionString).</summary>
    public ParameterUsage Purpose { get; private set; } = null!;

    private ResourceParameterUsage() { }

    internal ResourceParameterUsage(
        AzureResourceId resourceId,
        ParameterDefinitionId parameterId,
        ParameterUsage purpose)
        : base(ResourceParameterUsageId.CreateUnique())
    {
        ResourceId = resourceId;
        ParameterId = parameterId;
        Purpose = purpose;
    }
}