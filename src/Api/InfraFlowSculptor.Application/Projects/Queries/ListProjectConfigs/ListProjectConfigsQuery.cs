using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.ListProjectConfigs;

/// <summary>Query to list all infrastructure configurations belonging to a project.</summary>
public record ListProjectConfigsQuery(ProjectId ProjectId)
    : IRequest<ErrorOr<List<GetInfrastructureConfigResult>>>;
