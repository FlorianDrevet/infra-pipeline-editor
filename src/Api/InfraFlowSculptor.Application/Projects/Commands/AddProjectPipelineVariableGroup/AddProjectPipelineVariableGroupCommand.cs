using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectPipelineVariableGroup;

/// <summary>
/// Command to add a new Azure DevOps Pipeline Variable Group to a project (shared across all configurations).
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="GroupName">The name of the Azure DevOps Variable Group.</param>
public record AddProjectPipelineVariableGroupCommand(ProjectId ProjectId, string GroupName)
    : ICommand<AddProjectPipelineVariableGroupResult>;

/// <summary>Result returned after adding a project pipeline variable group.</summary>
/// <param name="GroupId">The identifier of the created group.</param>
/// <param name="GroupName">The name of the variable group.</param>
public record AddProjectPipelineVariableGroupResult(Guid GroupId, string GroupName);
