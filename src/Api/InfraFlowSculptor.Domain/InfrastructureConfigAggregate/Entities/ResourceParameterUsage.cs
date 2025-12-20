using BicepGenerator.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.Common.Models;

public sealed class ResourceParameterUsage
    : Entity<ResourceParameterUsageId>
{
    public AzureResourceId ResourceId { get; private set; }
    public ParameterDefinitionId ParameterId { get; private set; }

    public ParameterUsage Purpose { get; private set; }

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