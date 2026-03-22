using BicepGenerator.Domain.Common.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ProjectAggregate.Entities;

/// <summary>
/// Stores a per-resource-type naming template at the project level.
/// Provides defaults for all configurations in the project unless overridden.
/// </summary>
public sealed class ProjectResourceNamingTemplate : Entity<ProjectResourceNamingTemplateId>
{
    /// <summary>Gets the identifier of the parent project.</summary>
    public ProjectId ProjectId { get; private set; } = null!;

    /// <summary>Navigation property to the parent project.</summary>
    public Project Project { get; private set; } = null!;

    /// <summary>
    /// The Azure resource type this template applies to (e.g. "KeyVault", "RedisCache", "StorageAccount").
    /// </summary>
    public string ResourceType { get; private set; } = null!;

    /// <summary>
    /// The naming template, e.g. "{prefix}-{name}-kv-{env}".
    /// </summary>
    public NamingTemplate Template { get; private set; } = null!;

    private ProjectResourceNamingTemplate() { }

    internal ProjectResourceNamingTemplate(
        ProjectId projectId,
        string resourceType,
        NamingTemplate template)
        : base(ProjectResourceNamingTemplateId.CreateUnique())
    {
        ProjectId = projectId;
        ResourceType = resourceType;
        Template = template;
    }

    /// <summary>Updates the naming template.</summary>
    internal void Update(NamingTemplate template)
    {
        Template = template;
    }
}
