using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.ValidateRecentItems;

/// <summary>Handler that validates recently viewed items against user access and item existence.</summary>
public sealed class ValidateRecentItemsQueryHandler(
    IProjectRepository projectRepository,
    IInfrastructureConfigRepository configRepository,
    ICurrentUser currentUser)
    : IQueryHandler<ValidateRecentItemsQuery, List<RecentItemResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<List<RecentItemResult>>> Handle(
        ValidateRecentItemsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Items.Count == 0)
        {
            return new List<RecentItemResult>();
        }

        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        // Use lightweight projections — only Id/Name/Description are needed, not full aggregates with Members/Users.
        var accessibleProjects = await projectRepository.GetProjectSummariesForUserAsync(userId, cancellationToken);
        var accessibleProjectIds = accessibleProjects
            .ToDictionary(p => p.Id, p => p);

        var accessibleConfigs = await configRepository.GetConfigSummariesForUserAsync(userId, cancellationToken);
        var accessibleConfigIds = accessibleConfigs
            .ToDictionary(c => c.Id, c => c);

        var results = new List<RecentItemResult>();

        foreach (var item in query.Items)
        {
            switch (item.Type.ToLowerInvariant())
            {
                case "project" when accessibleProjectIds.TryGetValue(item.Id, out var project):
                    results.Add(new RecentItemResult(
                        project.Id.ToString(),
                        project.Name,
                        "project",
                        project.Description));
                    break;

                case "config" when accessibleConfigIds.TryGetValue(item.Id, out var config):
                    results.Add(new RecentItemResult(
                        config.Id.ToString(),
                        config.Name,
                        "config",
                        null));
                    break;
            }
        }

        return results;
    }
}
