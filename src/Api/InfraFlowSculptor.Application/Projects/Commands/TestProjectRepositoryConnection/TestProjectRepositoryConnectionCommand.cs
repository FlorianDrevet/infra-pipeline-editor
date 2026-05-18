using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.TestProjectRepositoryConnection;

/// <summary>
/// Command to test the Git connection for a specific project repository.
/// </summary>
public record TestProjectRepositoryConnectionCommand(
    ProjectId ProjectId,
    ProjectRepositoryId RepositoryId) : ICommand<TestGitConnectionResult>;