using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

/// <summary>Storage service type that a CORS rule applies to (Blob or Table).</summary>
public sealed class CorsServiceType(CorsServiceType.Service value)
    : EnumValueObject<CorsServiceType.Service>(value)
{
    /// <summary>Available storage service types for CORS.</summary>
    public enum Service
    {
        Blob,
        Table
    }
}