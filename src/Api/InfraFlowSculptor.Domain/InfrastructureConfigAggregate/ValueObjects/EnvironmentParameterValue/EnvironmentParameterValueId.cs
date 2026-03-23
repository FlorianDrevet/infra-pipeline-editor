using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.EnvironmentParameterValue;

public sealed class EnvironmentParameterValueId(Guid value) : Id<EnvironmentParameterValueId>(value);