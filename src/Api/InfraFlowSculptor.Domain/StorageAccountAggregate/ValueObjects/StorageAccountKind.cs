using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

/// <summary>Kind of Azure Storage Account (e.g. StorageV2, BlobStorage).</summary>
public sealed class StorageAccountKind(StorageAccountKind.Kind value) : EnumValueObject<StorageAccountKind.Kind>(value)
{
    /// <summary>Available Storage Account kinds.</summary>
    public enum Kind
    {
        BlobStorage,
        BlockBlobStorage,
        FileStorage,
        Storage,
        StorageV2
    }
}
