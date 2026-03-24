using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectEnvironment;

/// <summary>Command to add an environment definition to a project.</summary>
public record AddProjectEnvironmentCommand(
    ProjectId ProjectId,
    string Name,
    string ShortName,
    string Prefix,
    string Suffix,
    string Location,
    Guid TenantId,
    Guid SubscriptionId,
    int Order,
    bool RequiresApproval,
    IReadOnlyList<(string Name, string Value)> Tags
) : IRequest<ErrorOr<ProjectEnvironmentDefinitionResult>>;
