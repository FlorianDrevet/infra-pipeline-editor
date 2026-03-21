using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.ListProjectConfigs;

/// <summary>Handles the <see cref="ListProjectConfigsQuery"/> request.</summary>
public sealed class ListProjectConfigsQueryHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IMapper mapper)
    : IRequestHandler<ListProjectConfigsQuery, ErrorOr<List<GetInfrastructureConfigResult>>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<List<GetInfrastructureConfigResult>>> Handle(
        ListProjectConfigsQuery query, CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyReadAccessAsync(query.ProjectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var project = await projectRepository.GetByIdWithConfigurationsAsync(query.ProjectId, cancellationToken);

        return project!.Configurations
            .Select(c => mapper.Map<GetInfrastructureConfigResult>(c))
            .ToList();
    }
}
