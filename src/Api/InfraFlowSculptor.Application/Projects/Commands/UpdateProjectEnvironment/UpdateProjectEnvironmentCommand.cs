using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.UpdateProjectEnvironment;

/// <summary>Command to update a project-level environment definition.</summary>
public record UpdateProjectEnvironmentCommand(
    ProjectId ProjectId,
    ProjectEnvironmentDefinitionId EnvironmentId,
    string Name,
    string ShortName,
    string Prefix,
    string Suffix,
    string Location,
    Guid SubscriptionId,
    int Order,
    bool RequiresApproval,
    string? AzureResourceManagerConnection,
    IReadOnlyCollection<(string Name, string Value)> Tags
) : ICommand<ProjectEnvironmentDefinitionResult>;
