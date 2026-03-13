using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

public class TlsVersion(TlsVersion.TlsVersionEnum value) : EnumValueObject<TlsVersion.TlsVersionEnum>(value)
{
    public enum TlsVersionEnum
    {
        Tls10,
        Tls11,
        Tls12
    }
}
