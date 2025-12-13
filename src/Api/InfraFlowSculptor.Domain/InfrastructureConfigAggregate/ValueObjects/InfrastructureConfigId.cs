using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

public sealed class InfrastructureConfigId(Guid value) : Id<InfrastructureConfigId>(value);