using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.ListProjectResources;

/// <summary>
/// Query to list all Azure resources across all configurations in a project.
/// Used by the frontend to populate cross-configuration resource pickers.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
public record ListProjectResourcesQuery(Guid ProjectId)
    : IRequest<ErrorOr<List<ProjectResourceResult>>>;
