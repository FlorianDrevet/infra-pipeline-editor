using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

/// <summary>Minimum TLS version for Storage Account client connections.</summary>
public class StorageAccountTlsVersion(StorageAccountTlsVersion.Version value) : EnumValueObject<StorageAccountTlsVersion.Version>(value)
{
    /// <summary>Supported TLS versions.</summary>
    public enum Version
    {
        Tls10,
        Tls11,
        Tls12
    }
}
