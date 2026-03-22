using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.ListMyProjects;

/// <summary>Query to retrieve all projects the current user is a member of.</summary>
public record ListMyProjectsQuery : IRequest<ErrorOr<List<ProjectResult>>>;
