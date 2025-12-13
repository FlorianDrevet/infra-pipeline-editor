using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Shared.Domain.Domain.Models;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate;

public sealed class InfrastructureConfig : AggregateRoot<InfrastructureConfigId>
{
    public Name Name { get; private set; } = null!;
    private List<ResourceGroup> _resourceGroups { get; set; } = new();
    public IReadOnlyList<ResourceGroup> ResourceGroups => _resourceGroups.AsReadOnly();
    
    private readonly List<Member> _members = new();
    public IReadOnlyCollection<Member> Members => _members.AsReadOnly();
    
    //public List<EnvironmentVariable> Variables { get; set; } = new();

    private InfrastructureConfig(InfrastructureConfigId id, Name name, UserId ownerId): base(id)
    {
        Name = name;
        _members.Add(Member.CreateOwner(id, ownerId));
    }

    public static InfrastructureConfig Create(Name name, UserId ownerId)
    {
        return new InfrastructureConfig(InfrastructureConfigId.CreateUnique(), name, ownerId);
    }

    public InfrastructureConfig()
    {
    }
    
    public bool AddResourceGroup(ResourceGroup resourceGroup)
    {
        if (_resourceGroups.Any(rg => rg.Name == resourceGroup.Name))
            return false;
        
        _resourceGroups.Add(resourceGroup);
        return true;
    }
    
    public bool RemoveResourceGroup(ResourceGroup resourceGroup)
    {
        return _resourceGroups.Remove(resourceGroup);
    }
    
    public void Rename(Name name)
    {
        Name = name;
    }

    public void AddMember(UserId userId, Role role)
    {
        /*
        if (_members.Any(m => m.UserId == userId))
            throw new DomainException("User already member of project");
        */

        _members.Add(new Member(Id, userId, role));
    }

    public void ChangeRole(UserId userId, Role newRole)
    {
        var member = GetMember(userId);
        member.ChangeRole(newRole);
    }

    public void RemoveMember(UserId userId)
    {
        var member = GetMember(userId);

        /*
        if (member.Role.Value == Role.RoleEnum.Owner)
            throw new DomainException("Cannot remove project owner");
        */

        _members.Remove(member);
    }

    private Member? GetMember(UserId userId)
    {
        return _members.FirstOrDefault(m => m.UserId == userId);/*
               ?? throw new DomainException("User is not a member of this project");*/
    }
}