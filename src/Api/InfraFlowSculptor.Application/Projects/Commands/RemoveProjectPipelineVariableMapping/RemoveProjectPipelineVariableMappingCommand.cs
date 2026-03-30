using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectPipelineVariableMapping;

/// <summary>
/// Command to remove a variable mapping from a project-level pipeline variable group.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="GroupId">The pipeline variable group identifier.</param>
/// <param name="MappingId">The mapping identifier to remove.</param>
public record RemoveProjectPipelineVariableMappingCommand(ProjectId ProjectId, Guid GroupId, Guid MappingId)
    : ICommand<Deleted>;
