using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

/// <summary>Display name for a domain entity (project, infrastructure config, resource, etc.).</summary>
public sealed class Name : SingleValueObject<string>
{
    private Name() { }

    public Name(string value) : base(value)
    {
    }
}