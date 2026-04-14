using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for a <see cref="Entities.CorsRule"/>.</summary>
public sealed class CorsRuleId(Guid value) : Id<CorsRuleId>(value);