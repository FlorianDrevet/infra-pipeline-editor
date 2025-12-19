using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

public sealed class EnvironmentDefinitionId(Guid value) : Id<EnvironmentDefinitionId>(value);