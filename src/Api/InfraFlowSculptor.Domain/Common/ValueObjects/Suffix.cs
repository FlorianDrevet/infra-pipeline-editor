using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

public sealed class Suffix : SingleValueObject<string>
{
    private Suffix() { }

    public Suffix(string value) : base(value)
    {
    }
}
