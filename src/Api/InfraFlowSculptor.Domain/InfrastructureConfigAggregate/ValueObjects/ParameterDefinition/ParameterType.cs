using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;

/// <summary>Bicep parameter type for an infrastructure configuration parameter.</summary>
public sealed class ParameterType : EnumValueObject<ParameterType.Enum>
{
    /// <summary>Supported Bicep parameter types.</summary>
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