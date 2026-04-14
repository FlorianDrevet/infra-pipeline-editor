using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for a <see cref="Entities.BlobLifecycleRule"/>.</summary>
public sealed class BlobLifecycleRuleId(Guid value) : Id<BlobLifecycleRuleId>(value);
