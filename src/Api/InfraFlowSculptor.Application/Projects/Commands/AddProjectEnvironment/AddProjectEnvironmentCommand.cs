using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectEnvironment;

/// <summary>Command to add an environment definition to a project.</summary>
public record AddProjectEnvironmentCommand(
    ProjectId ProjectId,
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
