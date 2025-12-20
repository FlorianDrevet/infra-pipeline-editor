using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;

public sealed class ParameterType : EnumValueObject<ParameterType.Enum>
{
    public enum Enum
    {
        String,
        Int,
        Bool,
        Object,
        Array
    }

    public ParameterType(Enum value) : base(value) { }
}