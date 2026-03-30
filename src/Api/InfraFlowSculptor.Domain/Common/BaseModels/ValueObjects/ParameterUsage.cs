using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

/// <summary>
/// Defines how a <see cref="InfrastructureConfigAggregate.Entities.ParameterDefinition"/> is consumed
/// by an Azure resource (e.g. as a secret, app setting, or connection string).
/// </summary>
public sealed class ParameterUsage : ValueObject
{
    /// <summary>Gets the string representation of this usage type.</summary>
    public string Value { get; }

    private ParameterUsage(string value)
    {
        Value = value;
    }

    /// <summary>The parameter is stored as a Key Vault secret.</summary>
    public static ParameterUsage Secret => new("secret");
    /// <summary>The parameter is injected as an application setting / environment variable.</summary>
    public static ParameterUsage AppSetting => new("appSetting");
    /// <summary>The parameter is injected as a connection string.</summary>
    public static ParameterUsage ConnectionString => new("connectionString");
    
    /// <summary>Returns all defined parameter usage types.</summary>
    public static IEnumerable<ParameterUsage> All =>
        [Secret, AppSetting, ConnectionString];

    /// <summary>Parses a string into the corresponding <see cref="ParameterUsage"/>.</summary>
    /// <exception cref="ArgumentException">Thrown when the value does not match any known usage.</exception>
    public static ParameterUsage From(string value)
    {
        var usage = All.FirstOrDefault(x => x.Value == value);
        if (usage is null)
            throw new ArgumentException($"Invalid ParameterUsage '{value}'");

        return usage;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}