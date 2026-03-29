using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

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
    Guid TenantId,
    Guid SubscriptionId,
    int Order,
    bool RequiresApproval,
    string? AzureResourceManagerConnection,
    IReadOnlyList<(string Name, string Value)> Tags
) : IRequest<ErrorOr<ProjectEnvironmentDefinitionResult>>;
