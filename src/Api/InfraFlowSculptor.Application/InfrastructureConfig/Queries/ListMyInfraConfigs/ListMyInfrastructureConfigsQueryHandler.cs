using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListMyInfraConfigs;

/// <summary>
/// Returns all configurations across all projects the current user is a member of.
/// </summary>
public sealed class ListMyInfrastructureConfigsQueryHandler(
    IProjectRepository projectRepository,
    IInfrastructureConfigRepository configRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<ListMyInfrastructureConfigsQuery, ErrorOr<List<GetInfrastructureConfigResult>>>
{
    public async Task<ErrorOr<List<GetInfrastructureConfigResult>>> Handle(
        ListMyInfrastructureConfigsQuery query, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var projects = await projectRepository.GetAllForUserAsync(userId, cancellationToken);

        var allConfigs = new List<GetInfrastructureConfigResult>();
        foreach (var project in projects)
        {
            var configs = await configRepository.GetByProjectIdAsync(project.Id, cancellationToken);
            allConfigs.AddRange(configs.Select(c => mapper.Map<GetInfrastructureConfigResult>(c)));
        }

        return allConfigs;
    }
}
