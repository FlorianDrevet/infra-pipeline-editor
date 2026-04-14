using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.TestGitConnection;

/// <summary>Command to test the Git repository connection for a project.</summary>
public record TestGitConnectionCommand(
    ProjectId ProjectId
) : ICommand<TestGitConnectionResult>;
