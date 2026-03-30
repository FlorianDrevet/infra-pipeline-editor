using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceNamingTemplate;

/// <summary>Command to remove a per-resource-type naming template from a project.</summary>
public record RemoveProjectResourceNamingTemplateCommand(
    ProjectId ProjectId,
    string ResourceType
) : ICommand<Deleted>;
