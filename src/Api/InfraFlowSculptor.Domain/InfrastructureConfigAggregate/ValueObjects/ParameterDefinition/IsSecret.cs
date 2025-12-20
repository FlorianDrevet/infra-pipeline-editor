using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;

public sealed class IsSecret: SingleValueObject<bool>
{
    private IsSecret() { }

    public IsSecret(bool value) : base(value)
    {
    }
}