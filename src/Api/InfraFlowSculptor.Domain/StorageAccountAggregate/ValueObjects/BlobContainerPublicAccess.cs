using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

public class BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel value) : EnumValueObject<BlobContainerPublicAccess.AccessLevel>(value)
{
    public enum AccessLevel
    {
        None,
        Blob,
        Container
    }
}
