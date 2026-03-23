using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.Common.Models;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate;

public sealed class InfrastructureConfig : AggregateRoot<InfrastructureConfigId>
{
    public Name Name { get; private set; } = null!;

    /// <summary>Gets the identifier of the parent project.</summary>
    public ProjectId ProjectId { get; private set; } = null!;

    /// <summary>
    /// Default naming template applied to all resource types unless overridden.
    /// Supports placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {resourceAbbr}, {location}.
    /// When null, the resource Name is used as-is.
    /// </summary>
    public NamingTemplate? DefaultNamingTemplate { get; private set; }

    /// <summary>
    /// When <c>true</c>, this configuration inherits environments from the parent project.
    /// When <c>false</c>, it uses its own environment definitions.
    /// </summary>
    public bool UseProjectEnvironments { get; private set; } = true;

    /// <summary>
    /// When <c>true</c>, this configuration inherits naming conventions from the parent project.
    /// When <c>false</c>, it uses its own naming templates.
    /// </summary>
    public bool UseProjectNamingConventions { get; private set; } = true;

    private readonly List<ResourceGroup> _resourceGroups = new();
    public IReadOnlyList<ResourceGroup> ResourceGroups => _resourceGroups.AsReadOnly();

    private readonly List<EnvironmentDefinition> _environmentDefinitions = new();
    public IReadOnlyCollection<EnvironmentDefinition> EnvironmentDefinitions => _environmentDefinitions.AsReadOnly();
    
    private readonly List<ResourceNamingTemplate> _resourceNamingTemplates = new();
    public IReadOnlyCollection<ResourceNamingTemplate> ResourceNamingTemplates => _resourceNamingTemplates.AsReadOnly();

    private readonly List<ParameterDefinition> _parameterDefinitions = new();
    public IReadOnlyCollection<ParameterDefinition> ParameterDefinitions => _parameterDefinitions;

    
    private InfrastructureConfig(InfrastructureConfigId id, Name name, ProjectId projectId): base(id)
    {
        Name = name;
        ProjectId = projectId;
    }

    /// <summary>
    /// Creates a new <see cref="InfrastructureConfig"/> belonging to the specified project.
    /// </summary>
    public static InfrastructureConfig Create(Name name, ProjectId projectId)
    {
        return new InfrastructureConfig(InfrastructureConfigId.CreateUnique(), name, projectId);
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

    // ─── Environment Definitions ────────────────────────────────────────────

    public EnvironmentDefinition AddEnvironment(EnvironmentDefinitionData data)
    {
        ShiftOrdersUp(data.Order.Value);
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

        var oldOrder = env.Order.Value;
        var newOrder = data.Order.Value;

        if (oldOrder != newOrder)
            ReorderEnvironments(envId, oldOrder, newOrder);

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

        var removedOrder = env.Order.Value;
        _environmentDefinitions.Remove(env);
        ShiftOrdersDown(removedOrder);
        return true;
    }

    /// <summary>
    /// Shifts all environments with order &gt;= <paramref name="fromOrder"/> up by one
    /// to make room for a new environment at that position.
    /// </summary>
    private void ShiftOrdersUp(int fromOrder)
    {
        foreach (var env in _environmentDefinitions.Where(e => e.Order.Value >= fromOrder))
            env.Order = new Order(env.Order.Value + 1);
    }

    /// <summary>
    /// Shifts all environments with order &gt; <paramref name="removedOrder"/> down by one
    /// to close the gap left after removal.
    /// </summary>
    private void ShiftOrdersDown(int removedOrder)
    {
        foreach (var env in _environmentDefinitions.Where(e => e.Order.Value > removedOrder))
            env.Order = new Order(env.Order.Value - 1);
    }

    /// <summary>
    /// Reorders sibling environments when an existing environment moves from
    /// <paramref name="oldOrder"/> to <paramref name="newOrder"/>.
    /// </summary>
    private void ReorderEnvironments(EnvironmentDefinitionId movingId, int oldOrder, int newOrder)
    {
        if (newOrder < oldOrder)
        {
            // Moving up: shift environments in [newOrder, oldOrder) up by one
            foreach (var e in _environmentDefinitions
                         .Where(e => e.Id != movingId && e.Order.Value >= newOrder && e.Order.Value < oldOrder))
                e.Order = new Order(e.Order.Value + 1);
        }
        else
        {
            // Moving down: shift environments in (oldOrder, newOrder] down by one
            foreach (var e in _environmentDefinitions
                         .Where(e => e.Id != movingId && e.Order.Value > oldOrder && e.Order.Value <= newOrder))
                e.Order = new Order(e.Order.Value - 1);
        }
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

    // ─── Inheritance Toggles ────────────────────────────────────────────────

    /// <summary>Sets whether this configuration inherits environments from the parent project.</summary>
    public void SetUseProjectEnvironments(bool value)
    {
        UseProjectEnvironments = value;
    }

    /// <summary>Sets whether this configuration inherits naming conventions from the parent project.</summary>
    public void SetUseProjectNamingConventions(bool value)
    {
        UseProjectNamingConventions = value;
    }
}