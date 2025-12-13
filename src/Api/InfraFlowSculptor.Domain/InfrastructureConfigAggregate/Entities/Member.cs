using BicepGenerator.Domain.Common.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;

public sealed class Member : Entity<MemberId>
{
    public UserId UserId { get; private set; }
    public Role Role { get; private set; }
    
    public InfrastructureConfigId InfraConfigId { get; set; } = null!;
    public InfrastructureConfig InfraConfig { get; set; } = null!;

    private Member() { }

    internal Member(InfrastructureConfigId infraConfigId, UserId userId, Role role)
        : base(MemberId.CreateUnique())
    {
        UserId = userId;
        Role = role;
        InfraConfigId = infraConfigId;
    }

    internal static Member CreateOwner(InfrastructureConfigId infraConfigId, UserId userId)
        => new(infraConfigId, userId, new Role(Role.RoleEnum.Owner));

    internal void ChangeRole(Role role)
    {
        Role = role;
    }
}