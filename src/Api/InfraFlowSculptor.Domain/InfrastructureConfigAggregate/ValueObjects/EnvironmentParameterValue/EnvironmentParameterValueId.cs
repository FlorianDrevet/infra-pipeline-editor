using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.EnvironmentParameterValue;

public sealed class EnvironmentParameterValueId(Guid value) : Id<EnvironmentParameterValueId>(value);