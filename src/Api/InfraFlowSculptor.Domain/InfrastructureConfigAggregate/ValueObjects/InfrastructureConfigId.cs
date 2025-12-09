using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

public sealed class InfrastructureConfigId(Guid value) : Id<InfrastructureConfigId>(value);