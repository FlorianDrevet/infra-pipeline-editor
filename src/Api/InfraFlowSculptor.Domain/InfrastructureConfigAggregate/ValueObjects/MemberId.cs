using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

public sealed class MemberId(Guid value) : Id<MemberId>(value);