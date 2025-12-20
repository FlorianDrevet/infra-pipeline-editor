using BicepGenerator.Domain.Common.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.EnvironmentParameterValue;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;
using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;

public sealed class EnvironmentParameterValue
    : Entity<EnvironmentParameterValueId>
{
    public EnvironmentDefinitionId EnvironmentId { get; private set; }
    public ParameterDefinitionId ParameterId { get; private set; }

    // null si IsSecret = true
    public string? Value { get; private set; }

    private EnvironmentParameterValue() { }

    internal EnvironmentParameterValue(
        EnvironmentDefinitionId environmentId,
        ParameterDefinitionId parameterId,
        string? value)
        : base(EnvironmentParameterValueId.CreateUnique())
    {
        EnvironmentId = environmentId;
        ParameterId = parameterId;
        Value = value;
    }
}