using ErrorOr;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ProjectAggregate.Entities;

/// <summary>
/// Represents a reference to an Azure DevOps Pipeline Library (Variable Group)
/// and its variable-to-Bicep-parameter mappings.
/// Project-level scope: shared across all configurations in the project.
/// </summary>
public sealed class ProjectPipelineVariableGroup : Entity<ProjectPipelineVariableGroupId>
{
    /// <summary>Gets the owning project identifier.</summary>
    public ProjectId ProjectId { get; private set; } = null!;

    /// <summary>Gets the name of the Azure DevOps Variable Group (e.g. <c>MyApp-Secrets</c>).</summary>
    public string GroupName { get; private set; } = null!;

    private readonly List<ProjectPipelineVariableMapping> _mappings = [];

    /// <summary>Gets the variable-to-Bicep-parameter mappings in this group.</summary>
    public IReadOnlyCollection<ProjectPipelineVariableMapping> Mappings => _mappings.AsReadOnly();

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

    /// <summary>
    /// Adds a variable mapping to this group.
    /// </summary>
    /// <param name="pipelineVariableName">The variable name in the Azure DevOps Library.</param>
    /// <param name="bicepParameterName">The target Bicep parameter name.</param>
    /// <returns>The created mapping or an error if duplicate.</returns>
    public ErrorOr<ProjectPipelineVariableMapping> AddMapping(
        string pipelineVariableName,
        string bicepParameterName)
    {
        if (_mappings.Any(m =>
                string.Equals(m.BicepParameterName, bicepParameterName, StringComparison.OrdinalIgnoreCase)))
        {
            return Domain.Common.Errors.Errors.Project.DuplicateVariableMappingError(bicepParameterName);
        }

        var mapping = ProjectPipelineVariableMapping.Create(Id, pipelineVariableName, bicepParameterName);
        _mappings.Add(mapping);
        return mapping;
    }

    /// <summary>
    /// Removes a variable mapping from this group.
    /// </summary>
    /// <param name="mappingId">The identifier of the mapping to remove.</param>
    /// <returns>Success or a not-found error.</returns>
    public ErrorOr<Deleted> RemoveMapping(ProjectPipelineVariableMappingId mappingId)
    {
        var mapping = _mappings.FirstOrDefault(m => m.Id == mappingId);
        if (mapping is null)
            return Domain.Common.Errors.Errors.Project.VariableMappingNotFoundError(mappingId);

        _mappings.Remove(mapping);
        return Result.Deleted;
    }

    /// <summary>EF Core constructor.</summary>
    private ProjectPipelineVariableGroup() { }
}
