using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

public class StorageAccountKind(StorageAccountKind.Kind value) : EnumValueObject<StorageAccountKind.Kind>(value)
{
    public enum Kind
    {
        BlobStorage,
        BlockBlobStorage,
        FileStorage,
        Storage,
        StorageV2
    }
}
