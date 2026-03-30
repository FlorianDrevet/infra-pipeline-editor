using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for a <see cref="Entities.BlobContainer"/>.</summary>
public sealed class BlobContainerId(Guid value) : Id<BlobContainerId>(value);
