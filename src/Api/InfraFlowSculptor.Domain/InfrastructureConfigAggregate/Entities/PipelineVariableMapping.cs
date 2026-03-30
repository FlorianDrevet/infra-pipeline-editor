using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.PipelineVariableGroup;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;

/// <summary>
/// Maps a pipeline variable from an Azure DevOps Variable Group to a Bicep parameter name.
/// Used to generate <c>overrideParameters</c> in the ARM deployment task.
/// </summary>
public sealed class PipelineVariableMapping : Entity<PipelineVariableMappingId>
{
    /// <summary>Gets the owning variable group identifier.</summary>
    public PipelineVariableGroupId VariableGroupId { get; private set; } = null!;

    /// <summary>Gets the name of the variable in the Azure DevOps Library (e.g. <c>SQL_ADMIN_PASSWORD</c>).</summary>
    public string PipelineVariableName { get; private set; } = null!;

    /// <summary>Gets the target Bicep parameter name in <c>main.bicep</c> (e.g. <c>sqlAdminPassword</c>).</summary>
    public string BicepParameterName { get; private set; } = null!;

    private PipelineVariableMapping(
        PipelineVariableMappingId id,
        PipelineVariableGroupId variableGroupId,
        string pipelineVariableName,
        string bicepParameterName)
        : base(id)
    {
        VariableGroupId = variableGroupId;
        PipelineVariableName = pipelineVariableName;
        BicepParameterName = bicepParameterName;
    }

    /// <summary>
    /// Creates a new <see cref="PipelineVariableMapping"/> with a generated identifier.
    /// </summary>
    public static PipelineVariableMapping Create(
        PipelineVariableGroupId variableGroupId,
        string pipelineVariableName,
        string bicepParameterName)
    {
        return new PipelineVariableMapping(
            PipelineVariableMappingId.CreateUnique(),
            variableGroupId,
            pipelineVariableName,
            bicepParameterName);
    }

    /// <summary>EF Core constructor.</summary>
    private PipelineVariableMapping() { }
}
