using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

public sealed class BlobContainerId(Guid value) : Id<BlobContainerId>(value);
