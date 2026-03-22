using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.ListProjectConfigs;

/// <summary>Handles the <see cref="ListProjectConfigsQuery"/>.</summary>
public sealed class ListProjectConfigsQueryHandler(
    IProjectAccessService accessService,
    IInfrastructureConfigRepository configRepository,
    IMapper mapper)
    : IRequestHandler<ListProjectConfigsQuery, ErrorOr<List<GetInfrastructureConfigResult>>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<List<GetInfrastructureConfigResult>>> Handle(
        ListProjectConfigsQuery query,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyReadAccessAsync(query.ProjectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var configs = await configRepository.GetByProjectIdAsync(query.ProjectId, cancellationToken);
        return configs.Select(c => mapper.Map<GetInfrastructureConfigResult>(c)).ToList();
    }
}
