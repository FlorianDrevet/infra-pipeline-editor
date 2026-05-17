using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.ValueObjects;

/// <summary>
/// Strongly-typed value object wrapping the Azure resource type discriminator string
/// (e.g. "KeyVault", "WebApp"). Replaces raw <c>string</c> usage to enforce type safety.
/// </summary>
public sealed class ResourceTypeName : SingleValueObject<string>
{
    /// <summary>EF Core / serialization constructor.</summary>
    private ResourceTypeName() { }

    /// <summary>Creates a new <see cref="ResourceTypeName"/> from a raw string value.</summary>
    public ResourceTypeName(string value) : base(value) { }

    /// <summary>Implicit conversion from <see cref="string"/> to <see cref="ResourceTypeName"/>.</summary>
    public static implicit operator ResourceTypeName(string value) => new(value);
}
