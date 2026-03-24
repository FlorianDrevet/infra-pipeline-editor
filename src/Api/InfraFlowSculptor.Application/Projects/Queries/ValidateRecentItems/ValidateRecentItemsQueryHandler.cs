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
    : IRequestHandler<ValidateRecentItemsQuery, ErrorOr<List<RecentItemResult>>>
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

        var accessibleProjects = await projectRepository.GetAllForUserAsync(userId, cancellationToken);
        var accessibleProjectIds = accessibleProjects
            .ToDictionary(p => p.Id.Value, p => p);

        var accessibleConfigs = await configRepository.GetAllForUserAsync(userId, cancellationToken);
        var accessibleConfigIds = accessibleConfigs
            .ToDictionary(c => c.Id.Value, c => c);

        var results = new List<RecentItemResult>();

        foreach (var item in query.Items)
        {
            switch (item.Type.ToLowerInvariant())
            {
                case "project" when accessibleProjectIds.TryGetValue(item.Id, out var project):
                    results.Add(new RecentItemResult(
                        project.Id.Value.ToString(),
                        project.Name.Value,
                        "project",
                        project.Description));
                    break;

                case "config" when accessibleConfigIds.TryGetValue(item.Id, out var config):
                    results.Add(new RecentItemResult(
                        config.Id.Value.ToString(),
                        config.Name.Value,
                        "config",
                        null));
                    break;
            }
        }

        return results;
    }
}
