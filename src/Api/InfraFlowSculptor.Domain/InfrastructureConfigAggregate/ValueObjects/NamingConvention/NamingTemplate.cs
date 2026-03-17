using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

/// <summary>
/// A naming template string using placeholders:
/// {name}, {prefix}, {suffix}, {env}, {resourceType}, {location}
/// Example: "{prefix}-{name}-kv"  or  "{name}-{resourceType}-{env}"
/// </summary>
public sealed class NamingTemplate : SingleValueObject<string>
{
    private NamingTemplate() { }

    public NamingTemplate(string value) : base(value)
    {
    }
}
