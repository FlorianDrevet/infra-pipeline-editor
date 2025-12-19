using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

public sealed class Suffix: SingleValueObject<string>
{
    private Suffix() { }

    public Suffix(string value) : base(value)
    {
    }
}