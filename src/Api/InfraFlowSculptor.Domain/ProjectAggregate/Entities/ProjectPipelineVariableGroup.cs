using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ProjectAggregate.Entities;

/// <summary>
/// Represents a reference to an Azure DevOps Pipeline Library (Variable Group).
/// Project-level scope: shared across all configurations in the project.
/// Variables are now derived from app settings linked to this group.
/// </summary>
public sealed class ProjectPipelineVariableGroup : Entity<ProjectPipelineVariableGroupId>
{
    /// <summary>Gets the owning project identifier.</summary>
    public ProjectId ProjectId { get; private set; } = null!;

    /// <summary>Gets the name of the Azure DevOps Variable Group (e.g. <c>MyApp-Secrets</c>).</summary>
    public string GroupName { get; private set; } = null!;

    private ProjectPipelineVariableGroup(
        ProjectPipelineVariableGroupId id,
        ProjectId projectId,
        string groupName)
        : base(id)
    {
        ProjectId = projectId;
        GroupName = groupName;
    }

    /// <summary>
    /// Creates a new <see cref="ProjectPipelineVariableGroup"/> with a generated identifier.
    /// </summary>
    public static ProjectPipelineVariableGroup Create(
        ProjectId projectId,
        string groupName)
    {
        return new ProjectPipelineVariableGroup(
            ProjectPipelineVariableGroupId.CreateUnique(),
            projectId,
            groupName);
    }

    /// <summary>EF Core constructor.</summary>
    private ProjectPipelineVariableGroup() { }
}
