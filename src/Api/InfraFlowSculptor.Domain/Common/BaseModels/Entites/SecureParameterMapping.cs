using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.BaseModels.Entites;

/// <summary>
/// Maps a secure Bicep parameter (e.g. <c>administratorLoginPassword</c>) to a specific
/// Azure DevOps pipeline variable group and variable name, allowing users to control
/// how secrets are injected at deployment time.
/// </summary>
public sealed class SecureParameterMapping : Entity<SecureParameterMappingId>
{
    /// <summary>Gets the identifier of the parent Azure resource.</summary>
    public AzureResourceId ResourceId { get; private set; } = null!;

    /// <summary>Gets the name of the secure Bicep parameter (e.g. <c>administratorLoginPassword</c>).</summary>
    public string SecureParameterName { get; private set; } = null!;

    /// <summary>Gets the optional pipeline variable group identifier. <c>null</c> when no custom mapping is set.</summary>
    public ProjectPipelineVariableGroupId? VariableGroupId { get; private set; }

    /// <summary>Gets the optional pipeline variable name within the group. <c>null</c> when no custom mapping is set.</summary>
    public string? PipelineVariableName { get; private set; }

    /// <summary>Gets a value indicating whether this mapping has a custom variable group assignment.</summary>
    public bool HasCustomMapping => VariableGroupId is not null;

    /// <summary>EF Core constructor.</summary>
    private SecureParameterMapping() { }

    private SecureParameterMapping(
        AzureResourceId resourceId,
        string secureParameterName,
        ProjectPipelineVariableGroupId variableGroupId,
        string pipelineVariableName)
        : base(SecureParameterMappingId.CreateUnique())
    {
        ResourceId = resourceId;
        SecureParameterName = secureParameterName;
        VariableGroupId = variableGroupId;
        PipelineVariableName = pipelineVariableName;
    }

    /// <summary>Creates a new <see cref="SecureParameterMapping"/> with a custom variable group assignment.</summary>
    /// <param name="resourceId">Identifier of the parent Azure resource.</param>
    /// <param name="secureParameterName">Name of the secure Bicep parameter.</param>
    /// <param name="variableGroupId">Pipeline variable group identifier.</param>
    /// <param name="pipelineVariableName">Variable name within the group.</param>
    /// <returns>A new <see cref="SecureParameterMapping"/> entity.</returns>
    internal static SecureParameterMapping Create(
        AzureResourceId resourceId,
        string secureParameterName,
        ProjectPipelineVariableGroupId variableGroupId,
        string pipelineVariableName)
        => new(resourceId, secureParameterName, variableGroupId, pipelineVariableName);

    /// <summary>Updates the variable group and pipeline variable name for this mapping.</summary>
    /// <param name="variableGroupId">New pipeline variable group identifier.</param>
    /// <param name="pipelineVariableName">New pipeline variable name.</param>
    internal void Update(ProjectPipelineVariableGroupId variableGroupId, string pipelineVariableName)
    {
        VariableGroupId = variableGroupId;
        PipelineVariableName = pipelineVariableName;
    }
}
