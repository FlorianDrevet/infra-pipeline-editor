using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

/// <summary>Public access level for a blob container (None, Blob, or Container).</summary>
public sealed class BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel value) : EnumValueObject<BlobContainerPublicAccess.AccessLevel>(value)
{
    /// <summary>Available public access levels.</summary>
    public enum AccessLevel
    {
        None,
        Blob,
        Container
    }
}
