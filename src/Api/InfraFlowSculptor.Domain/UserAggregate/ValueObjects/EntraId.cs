using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

/// <summary>Microsoft Entra ID (Azure AD) object identifier for a user.</summary>
public sealed class EntraId : SingleValueObject<Guid>
{
    private EntraId() { }

    /// <summary>Initializes a new <see cref="EntraId"/> with the given Guid.</summary>
    public EntraId(Guid value) : base(Validate(value))
    {
    }

    private static Guid Validate(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("EntraId cannot be empty.", nameof(value));
        }

        return value;
    }
}
