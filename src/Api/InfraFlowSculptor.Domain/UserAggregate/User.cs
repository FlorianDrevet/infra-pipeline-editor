using System.Diagnostics.CodeAnalysis;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.UserAggregate;

public sealed class User : AggregateRoot<UserId>
{
    public required EntraId EntraId { get; init; }
    public required Name Name { get; init; }
    
    [SetsRequiredMembers]
    private User(UserId id, EntraId entraId, Name name)
        : base(id)
    {
        EntraId = entraId;
        Name = name;
    }

    public static User Create(EntraId entraId, Name name)
    {
        return new User(UserId.CreateUnique(), entraId, name);
    }

    public User()
    {
    }
}