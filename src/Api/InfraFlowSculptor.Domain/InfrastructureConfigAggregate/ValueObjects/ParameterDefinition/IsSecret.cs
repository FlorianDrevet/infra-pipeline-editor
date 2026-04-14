using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;

/// <summary>Indicates whether a parameter contains sensitive data that should be stored in Key Vault.</summary>
public sealed class IsSecret : SingleValueObject<bool>
{
    private IsSecret() { }

    public IsSecret(bool value) : base(value)
    {
    }
}