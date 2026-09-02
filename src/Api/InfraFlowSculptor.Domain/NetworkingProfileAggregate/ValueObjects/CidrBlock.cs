using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;

/// <summary>
/// Represents a CIDR address block (e.g., <c>10.0.0.0/16</c>).
/// Validates format on construction.
/// </summary>
public sealed class CidrBlock : ValueObject
{
    /// <summary>Gets the CIDR notation string value.</summary>
    public string Value { get; }

    /// <summary>Initializes a new <see cref="CidrBlock"/> with validation.</summary>
    /// <param name="value">A CIDR block in notation like <c>10.0.0.0/16</c>.</param>
    /// <exception cref="ArgumentException">Thrown when the value is not a valid CIDR block.</exception>
    public CidrBlock(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!IsValidCidr(value))
            throw new ArgumentException($"'{value}' is not a valid CIDR block.", nameof(value));

        Value = value;
    }

    /// <inheritdoc />
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    private static bool IsValidCidr(string cidr)
    {
        var parts = cidr.Split('/');
        if (parts.Length != 2)
            return false;

        if (!System.Net.IPAddress.TryParse(parts[0], out _))
            return false;

        if (!int.TryParse(parts[1], out var prefix))
            return false;

        return prefix is >= 0 and <= 32;
    }
}
