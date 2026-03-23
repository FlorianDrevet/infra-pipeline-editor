using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

public class TlsVersion(TlsVersion.Version value) : EnumValueObject<TlsVersion.Version>(value)
{
    public enum Version
    {
        Tls10,
        Tls11,
        Tls12
    }
}
