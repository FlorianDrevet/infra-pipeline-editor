using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

public sealed class StorageQueueId(Guid value) : Id<StorageQueueId>(value);
