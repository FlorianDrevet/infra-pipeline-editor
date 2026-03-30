using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Queries.GetProject;

/// <summary>Query to retrieve a project by its identifier.</summary>
public record GetProjectQuery(ProjectId Id) : IQuery<ProjectResult>;
