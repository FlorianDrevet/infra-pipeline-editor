using System.Diagnostics.CodeAnalysis;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.UserAggregate;

/// <summary>
/// Represents an authenticated user identified by their Microsoft Entra ID principal.
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    /// <summary>Gets the Microsoft Entra ID object identifier.</summary>
    public required EntraId EntraId { get; init; }

    /// <summary>Gets the user's display name.</summary>
    public required Name Name { get; init; }

    [SetsRequiredMembers]
    private User(UserId id, EntraId entraId, Name name)
        : base(id)
    {
        EntraId = entraId;
        Name = name;
    }

    /// <summary>Creates a new <see cref="User"/> with a generated identifier.</summary>
    public static User Create(EntraId entraId, Name name)
    {
        return new User(UserId.CreateUnique(), entraId, name);
    }

    /// <summary>EF Core constructor.</summary>
    public User()
    {
    }
}