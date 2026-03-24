using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

public sealed class BlobContainerId(Guid value) : Id<BlobContainerId>(value);
