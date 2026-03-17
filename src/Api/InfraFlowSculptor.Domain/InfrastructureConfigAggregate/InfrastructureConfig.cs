using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Shared.Domain.Domain.Models;
using Location = InfraFlowSculptor.Domain.Common.ValueObjects.Location;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate;

public sealed class InfrastructureConfig : AggregateRoot<InfrastructureConfigId>
{
    public Name Name { get; private set; } = null!;

    /// <summary>
    /// Default naming template applied to all resource types unless overridden.
    /// Supports placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {location}.
    /// When null, the resource Name is used as-is.
    /// </summary>
    public NamingTemplate? DefaultNamingTemplate { get; private set; }

    private readonly List<ResourceGroup> _resourceGroups = new();
    public IReadOnlyList<ResourceGroup> ResourceGroups => _resourceGroups.AsReadOnly();
    
    private readonly List<Member> _members = new();
    public IReadOnlyCollection<Member> Members => _members.AsReadOnly();
    
    private readonly List<EnvironmentDefinition> _environmentDefinitions = new();
    public IReadOnlyCollection<EnvironmentDefinition> EnvironmentDefinitions => _environmentDefinitions.AsReadOnly();
    
    private readonly List<ResourceNamingTemplate> _resourceNamingTemplates = new();
    public IReadOnlyCollection<ResourceNamingTemplate> ResourceNamingTemplates => _resourceNamingTemplates.AsReadOnly();

    private readonly List<ParameterDefinition> _parameterDefinitions = new();
    public IReadOnlyCollection<ParameterDefinition> ParameterDefinitions => _parameterDefinitions;

    
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

    // ─── Environment Definitions ────────────────────────────────────────────

    public EnvironmentDefinition AddEnvironment(EnvironmentDefinitionData data)
    {
        var env = new EnvironmentDefinition(Id, data);
        _environmentDefinitions.Add(env);
        return env;
    }

    public EnvironmentDefinition? UpdateEnvironment(
        EnvironmentDefinitionId envId,
        EnvironmentDefinitionData data)
    {
        var env = _environmentDefinitions.FirstOrDefault(e => e.Id == envId);
        if (env is null)
            return null;

        env.Name = data.Name;
        env.Prefix = data.Prefix;
        env.Suffix = data.Suffix;
        env.Location = data.Location;
        env.TenantId = data.TenantId;
        env.SubscriptionId = data.SubscriptionId;
        env.Order = data.Order;
        env.RequiresApproval = data.RequiresApproval;
        env.SetTags(data.Tags);
        return env;
    }

    public bool RemoveEnvironment(EnvironmentDefinitionId envId)
    {
        var env = _environmentDefinitions.FirstOrDefault(e => e.Id == envId);
        if (env is null)
            return false;
        _environmentDefinitions.Remove(env);
        return true;
    }

    // ─── Naming Convention ───────────────────────────────────────────────────

    public void SetDefaultNamingTemplate(NamingTemplate? template)
    {
        DefaultNamingTemplate = template;
    }

    public ResourceNamingTemplate SetResourceNamingTemplate(string resourceType, NamingTemplate template)
    {
        var existing = _resourceNamingTemplates.FirstOrDefault(t => t.ResourceType == resourceType);
        if (existing is not null)
        {
            existing.Update(template);
            return existing;
        }

        var entry = new ResourceNamingTemplate(Id, resourceType, template);
        _resourceNamingTemplates.Add(entry);
        return entry;
    }

    public bool RemoveResourceNamingTemplate(string resourceType)
    {
        var existing = _resourceNamingTemplates.FirstOrDefault(t => t.ResourceType == resourceType);
        if (existing is null)
            return false;
        _resourceNamingTemplates.Remove(existing);
        return true;
    }
}