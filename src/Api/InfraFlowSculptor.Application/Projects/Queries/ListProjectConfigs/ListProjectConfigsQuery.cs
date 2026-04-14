using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Queries.ListProjectConfigs;

/// <summary>Query to list all infrastructure configurations belonging to a project.</summary>
public record ListProjectConfigsQuery(ProjectId ProjectId)
    : IQuery<List<GetInfrastructureConfigResult>>;
