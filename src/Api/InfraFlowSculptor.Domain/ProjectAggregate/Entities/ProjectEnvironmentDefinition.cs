using System.Diagnostics.CodeAnalysis;
using BicepGenerator.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.ProjectAggregate.Entities;

/// <summary>
/// Represents a project-level environment definition that provides default
/// environments for all infrastructure configurations in the project.
/// </summary>
public sealed class ProjectEnvironmentDefinition : Entity<ProjectEnvironmentDefinitionId>
{
    /// <summary>Gets the identifier of the parent project.</summary>
    public ProjectId ProjectId { get; set; } = null!;

    /// <summary>Navigation property to the parent project.</summary>
    public Project Project { get; set; } = null!;

    /// <summary>Gets the environment display name (e.g. "Development", "Staging", "Production").</summary>
    public required Name Name { get; set; }

    /// <summary>Gets the short prefix prepended to generated resource names.</summary>
    public required Prefix Prefix { get; set; }

    /// <summary>Gets the short suffix appended to generated resource names.</summary>
    public required Suffix Suffix { get; set; }

    /// <summary>Gets the default Azure region for this environment.</summary>
    public required Location Location { get; set; }

    /// <summary>Gets the Azure AD tenant identifier.</summary>
    public required TenantId TenantId { get; set; }

    /// <summary>Gets the Azure subscription identifier.</summary>
    public required SubscriptionId SubscriptionId { get; set; }

    /// <summary>Gets the deployment ordering index (lower = deployed first).</summary>
    public Order Order { get; set; }

    /// <summary>Gets whether deployments to this environment require explicit approval.</summary>
    public RequiresApproval RequiresApproval { get; set; }

    private readonly List<Tag> _tags = new();

    /// <summary>Gets the Azure resource tags for this environment.</summary>
    public IReadOnlyCollection<Tag> Tags => _tags;

    /// <summary>Replaces all tags with the provided collection.</summary>
    public void SetTags(IEnumerable<Tag> tags)
    {
        _tags.Clear();
        _tags.AddRange(tags);
    }

    private ProjectEnvironmentDefinition() { }

    /// <summary>Creates a new project-level environment definition.</summary>
    [SetsRequiredMembers]
    internal ProjectEnvironmentDefinition(ProjectId projectId, EnvironmentDefinitionData data)
        : base(ProjectEnvironmentDefinitionId.CreateUnique())
    {
        ProjectId = projectId;
        Name = data.Name;
        Prefix = data.Prefix;
        Suffix = data.Suffix;
        Location = data.Location;
        TenantId = data.TenantId;
        SubscriptionId = data.SubscriptionId;
        Order = data.Order;
        RequiresApproval = data.RequiresApproval;
        _tags.AddRange(data.Tags);
    }
}
