using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

/// <summary>Suffix appended to generated resource names in naming templates.</summary>
public sealed class Suffix : SingleValueObject<string>
{
    private Suffix() { }

    public Suffix(string value) : base(value)
    {
    }
}
