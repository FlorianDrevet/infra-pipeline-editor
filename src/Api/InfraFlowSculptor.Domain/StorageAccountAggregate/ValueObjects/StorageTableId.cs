using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for a <see cref="Entities.StorageTable"/>.</summary>
public sealed class StorageTableId(Guid value) : Id<StorageTableId>(value);
