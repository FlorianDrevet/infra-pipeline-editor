using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

/// <summary>
/// A naming template string using placeholders:
/// {name}, {prefix}, {suffix}, {env}, {resourceType}, {resourceAbbr}, {location}
/// Example: "{name}-{resourceAbbr}{suffix}"  or  "{prefix}-{name}-{resourceAbbr}-{env}"
/// </summary>
public sealed class NamingTemplate : SingleValueObject<string>
{
    private NamingTemplate() { }

    public NamingTemplate(string value) : base(value)
    {
    }
}
