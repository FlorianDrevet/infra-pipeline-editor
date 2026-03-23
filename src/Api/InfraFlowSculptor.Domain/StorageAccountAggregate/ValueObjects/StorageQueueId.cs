using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

public sealed class StorageQueueId(Guid value) : Id<StorageQueueId>(value);
