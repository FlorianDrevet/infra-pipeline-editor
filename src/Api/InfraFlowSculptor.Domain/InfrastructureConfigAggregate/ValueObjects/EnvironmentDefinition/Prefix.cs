using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

public sealed class Prefix: SingleValueObject<string>
{
    private Prefix() { }

    public Prefix(string value) : base(value)
    {
    }
}