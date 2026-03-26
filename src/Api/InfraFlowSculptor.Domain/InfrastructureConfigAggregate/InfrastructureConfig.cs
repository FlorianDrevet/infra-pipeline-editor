using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
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
    /// When <c>true</c>, this configuration inherits naming conventions from the parent project.
    /// When <c>false</c>, it uses its own naming templates.
    /// </summary>
    public bool UseProjectNamingConventions { get; private set; } = true;

    private readonly List<ResourceGroup> _resourceGroups = new();
    public IReadOnlyList<ResourceGroup> ResourceGroups => _resourceGroups.AsReadOnly();

    private readonly List<ResourceNamingTemplate> _resourceNamingTemplates = new();
    public IReadOnlyCollection<ResourceNamingTemplate> ResourceNamingTemplates => _resourceNamingTemplates.AsReadOnly();

    private readonly List<ParameterDefinition> _parameterDefinitions = new();
    public IReadOnlyCollection<ParameterDefinition> ParameterDefinitions => _parameterDefinitions;

    private readonly List<CrossConfigResourceReference> _crossConfigReferences = [];
    /// <summary>Gets the cross-configuration resource references owned by this configuration.</summary>
    public IReadOnlyCollection<CrossConfigResourceReference> CrossConfigReferences => _crossConfigReferences.AsReadOnly();

    
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

    /// <summary>Sets whether this configuration inherits naming conventions from the parent project.</summary>
    public void SetUseProjectNamingConventions(bool value)
    {
        UseProjectNamingConventions = value;
    }

    // ─── Cross-Config References ────────────────────────────────────────────

    /// <summary>
    /// Adds a reference to a resource belonging to another infrastructure configuration in the same project.
    /// </summary>
    /// <param name="targetConfigId">The target configuration that owns the resource.</param>
    /// <param name="targetResourceId">The resource to reference.</param>
    /// <returns>The created reference or an error if a duplicate exists.</returns>
    public ErrorOr<CrossConfigResourceReference> AddCrossConfigReference(
        InfrastructureConfigId targetConfigId,
        AzureResourceId targetResourceId)
    {
        if (targetConfigId == Id)
            return Domain.Common.Errors.Errors.InfrastructureConfig.CannotReferenceSameConfig();

        if (_crossConfigReferences.Any(r => r.TargetResourceId == targetResourceId))
            return Domain.Common.Errors.Errors.InfrastructureConfig.DuplicateCrossConfigReference(targetResourceId);

        var reference = CrossConfigResourceReference.Create(Id, targetConfigId, targetResourceId);
        _crossConfigReferences.Add(reference);
        return reference;
    }

    /// <summary>
    /// Removes a cross-configuration resource reference by its identifier.
    /// </summary>
    /// <param name="referenceId">The reference to remove.</param>
    /// <returns><see cref="Result.Deleted"/> on success, or a not-found error.</returns>
    public ErrorOr<Deleted> RemoveCrossConfigReference(CrossConfigResourceReferenceId referenceId)
    {
        var reference = _crossConfigReferences.FirstOrDefault(r => r.Id == referenceId);
        if (reference is null)
            return Domain.Common.Errors.Errors.InfrastructureConfig.CrossConfigReferenceNotFound(referenceId);

        _crossConfigReferences.Remove(reference);
        return Result.Deleted;
    }
}