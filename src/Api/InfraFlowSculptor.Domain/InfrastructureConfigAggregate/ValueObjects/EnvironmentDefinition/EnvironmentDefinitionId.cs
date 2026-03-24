using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

public sealed class EnvironmentDefinitionId(Guid value) : Id<EnvironmentDefinitionId>(value);