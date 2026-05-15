using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.NsgRule"/>.</summary>
public sealed class NsgRuleId(Guid value) : Id<NsgRuleId>(value);
