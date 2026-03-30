using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ProjectAggregate.Entities;

/// <summary>
/// Maps a pipeline variable from an Azure DevOps Variable Group to a Bicep parameter name.
/// Used to generate <c>overrideParameters</c> in the ARM deployment task.
/// Project-level scope: shared across all configurations in the project.
/// </summary>
public sealed class ProjectPipelineVariableMapping : Entity<ProjectPipelineVariableMappingId>
{
    /// <summary>Gets the owning variable group identifier.</summary>
    public ProjectPipelineVariableGroupId VariableGroupId { get; private set; } = null!;

    /// <summary>Gets the name of the variable in the Azure DevOps Library (e.g. <c>SQL_ADMIN_PASSWORD</c>).</summary>
    public string PipelineVariableName { get; private set; } = null!;

    /// <summary>Gets the target Bicep parameter name in <c>main.bicep</c> (e.g. <c>sqlAdminPassword</c>).</summary>
    public string BicepParameterName { get; private set; } = null!;

    private ProjectPipelineVariableMapping(
        ProjectPipelineVariableMappingId id,
        ProjectPipelineVariableGroupId variableGroupId,
        string pipelineVariableName,
        string bicepParameterName)
        : base(id)
    {
        VariableGroupId = variableGroupId;
        PipelineVariableName = pipelineVariableName;
        BicepParameterName = bicepParameterName;
    }

    /// <summary>
    /// Creates a new <see cref="ProjectPipelineVariableMapping"/> with a generated identifier.
    /// </summary>
    public static ProjectPipelineVariableMapping Create(
        ProjectPipelineVariableGroupId variableGroupId,
        string pipelineVariableName,
        string bicepParameterName)
    {
        return new ProjectPipelineVariableMapping(
            ProjectPipelineVariableMappingId.CreateUnique(),
            variableGroupId,
            pipelineVariableName,
            bicepParameterName);
    }

    /// <summary>EF Core constructor.</summary>
    private ProjectPipelineVariableMapping() { }
}
