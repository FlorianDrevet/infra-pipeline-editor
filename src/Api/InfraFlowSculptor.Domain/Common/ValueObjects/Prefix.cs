using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

public sealed class Prefix : SingleValueObject<string>
{
    private Prefix() { }

    public Prefix(string value) : base(value)
    {
    }
}
