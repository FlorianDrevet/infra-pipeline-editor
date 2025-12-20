using BicepGenerator.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;

public sealed class ParameterDefinition : Entity<ParameterDefinitionId>
{
    public InfrastructureConfigId InfraConfigId { get; private set; }
    
    public Name Name { get; private set; }
    public ParameterType Type { get; private set; }
    public IsSecret IsSecret { get; private set; }
    public string? DefaultValue { get; private set; }

    private ParameterDefinition(ParameterDefinitionId id)
        : base(id)
    {

    }

    public static ParameterDefinition Create()
    {
        return new ParameterDefinition(ParameterDefinitionId.CreateUnique());
    }

    private ParameterDefinition()
    {
    }
}