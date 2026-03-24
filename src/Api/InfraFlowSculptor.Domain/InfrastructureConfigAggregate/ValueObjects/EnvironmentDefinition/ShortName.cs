using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

/// <summary>
/// Short identifier for an environment without separators (e.g. "dev", "qa", "prod").
/// Used in Bicep naming templates and environment variable maps.
/// </summary>
public sealed class ShortName : SingleValueObject<string>
{
    private ShortName() { }

    public ShortName(string value) : base(value)
    {
    }
}
