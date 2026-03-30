using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListPipelineVariableGroups;

/// <summary>
/// Handles listing pipeline variable groups with their mappings for an infrastructure configuration.
/// </summary>
public sealed class ListPipelineVariableGroupsQueryHandler(
    IInfraConfigAccessService accessService,
    IInfrastructureConfigRepository infraConfigRepository)
    : IQueryHandler<ListPipelineVariableGroupsQuery, List<PipelineVariableGroupResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<List<PipelineVariableGroupResult>>> Handle(
        ListPipelineVariableGroupsQuery query,
        CancellationToken cancellationToken)
    {
        var configId = new InfrastructureConfigId(query.InfraConfigId);
        var authResult = await accessService.VerifyReadAccessAsync(configId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var config = await infraConfigRepository.GetByIdWithPipelineVariableGroupsAsync(configId, cancellationToken);
        if (config is null)
            return Domain.Common.Errors.Errors.InfrastructureConfig.NotFoundError(configId);

        return config.PipelineVariableGroups
            .Select(g => new PipelineVariableGroupResult(
                g.Id.Value,
                g.GroupName,
                g.Mappings
                    .Select(m => new PipelineVariableMappingResult(
                        m.Id.Value,
                        m.PipelineVariableName,
                        m.BicepParameterName))
                    .ToList()))
            .ToList();
    }
}
