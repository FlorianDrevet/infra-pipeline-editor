using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;

public sealed class ParameterDefinitionId(Guid value) : Id<ParameterDefinitionId>(value);