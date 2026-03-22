using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceNamingTemplate;

/// <summary>Command to set or update a per-resource-type naming template at the project level.</summary>
public record SetProjectResourceNamingTemplateCommand(
    ProjectId ProjectId,
    string ResourceType,
    string Template
) : IRequest<ErrorOr<ProjectResourceNamingTemplateResult>>;
