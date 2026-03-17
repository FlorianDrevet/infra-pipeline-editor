using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

public class StorageAccountTlsVersion(StorageAccountTlsVersion.Version value) : EnumValueObject<StorageAccountTlsVersion.Version>(value)
{
    public enum Version
    {
        Tls10,
        Tls11,
        Tls12
    }
}
