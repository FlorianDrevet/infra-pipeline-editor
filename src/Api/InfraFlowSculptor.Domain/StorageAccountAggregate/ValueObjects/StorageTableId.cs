using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

public sealed class StorageTableId(Guid value) : Id<StorageTableId>(value);
