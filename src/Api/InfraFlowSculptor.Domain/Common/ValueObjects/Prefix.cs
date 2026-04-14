using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

/// <summary>Prefix prepended to generated resource names in naming templates.</summary>
public sealed class Prefix : SingleValueObject<string>
{
    private Prefix() { }

    public Prefix(string value) : base(value)
    {
    }
}
