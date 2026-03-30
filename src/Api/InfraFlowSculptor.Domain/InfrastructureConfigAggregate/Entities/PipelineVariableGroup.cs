using ErrorOr;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.PipelineVariableGroup;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;

/// <summary>
/// Represents a reference to an Azure DevOps Pipeline Library (Variable Group)
/// and its variable-to-Bicep-parameter mappings.
/// </summary>
public sealed class PipelineVariableGroup : Entity<PipelineVariableGroupId>
{
    /// <summary>Gets the owning infrastructure configuration identifier.</summary>
    public InfrastructureConfigId InfraConfigId { get; private set; } = null!;

    /// <summary>Gets the name of the Azure DevOps Variable Group (e.g. <c>MyApp-Secrets</c>).</summary>
    public string GroupName { get; private set; } = null!;

    private readonly List<PipelineVariableMapping> _mappings = [];

    /// <summary>Gets the variable-to-Bicep-parameter mappings in this group.</summary>
    public IReadOnlyCollection<PipelineVariableMapping> Mappings => _mappings.AsReadOnly();

    private PipelineVariableGroup(
        PipelineVariableGroupId id,
        InfrastructureConfigId infraConfigId,
        string groupName)
        : base(id)
    {
        InfraConfigId = infraConfigId;
        GroupName = groupName;
    }

    /// <summary>
    /// Creates a new <see cref="PipelineVariableGroup"/> with a generated identifier.
    /// </summary>
    public static PipelineVariableGroup Create(
        InfrastructureConfigId infraConfigId,
        string groupName)
    {
        return new PipelineVariableGroup(
            PipelineVariableGroupId.CreateUnique(),
            infraConfigId,
            groupName);
    }

    /// <summary>
    /// Adds a variable mapping to this group.
    /// </summary>
    /// <returns>The created mapping or an error if a duplicate Bicep parameter name exists.</returns>
    public ErrorOr<PipelineVariableMapping> AddMapping(string pipelineVariableName, string bicepParameterName)
    {
        if (_mappings.Any(m => m.BicepParameterName == bicepParameterName))
            return Domain.Common.Errors.Errors.InfrastructureConfig.DuplicateVariableMapping(bicepParameterName);

        var mapping = PipelineVariableMapping.Create(Id, pipelineVariableName, bicepParameterName);
        _mappings.Add(mapping);
        return mapping;
    }

    /// <summary>
    /// Removes a variable mapping by its identifier.
    /// </summary>
    /// <returns><see cref="Result.Deleted"/> on success, or a not-found error.</returns>
    public ErrorOr<Deleted> RemoveMapping(PipelineVariableMappingId mappingId)
    {
        var mapping = _mappings.FirstOrDefault(m => m.Id == mappingId);
        if (mapping is null)
            return Domain.Common.Errors.Errors.InfrastructureConfig.VariableMappingNotFound(mappingId);

        _mappings.Remove(mapping);
        return Result.Deleted;
    }

    /// <summary>EF Core constructor.</summary>
    private PipelineVariableGroup() { }
}
