using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

public sealed class CorsServiceType : EnumValueObject<CorsServiceType.Service>
{
    public enum Service
    {
        Blob,
        Table
    }

    public CorsServiceType(Service value)
        : base(value)
    {
    }
}