using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

/// <summary>Minimum TLS version for Redis Cache client connections.</summary>
public class TlsVersion(TlsVersion.Version value) : EnumValueObject<TlsVersion.Version>(value)
{
    /// <summary>Supported TLS versions.</summary>
    public enum Version
    {
        Tls10,
        Tls11,
        Tls12
    }
}
