using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for a <see cref="Entities.StorageQueue"/>.</summary>
public sealed class StorageQueueId(Guid value) : Id<StorageQueueId>(value);
