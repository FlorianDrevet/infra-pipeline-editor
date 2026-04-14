using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;

namespace InfraFlowSculptor.Application.Projects.Queries.ListMyProjects;

/// <summary>Query to retrieve all projects the current user is a member of.</summary>
public record ListMyProjectsQuery : IQuery<List<ProjectResult>>;
