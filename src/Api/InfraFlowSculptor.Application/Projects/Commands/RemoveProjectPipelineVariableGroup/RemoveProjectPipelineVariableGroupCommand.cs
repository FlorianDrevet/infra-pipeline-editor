using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectPipelineVariableGroup;

/// <summary>
/// Command to remove a pipeline variable group from a project.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="GroupId">The pipeline variable group identifier to remove.</param>
public record RemoveProjectPipelineVariableGroupCommand(ProjectId ProjectId, Guid GroupId)
    : ICommand<Deleted>;
