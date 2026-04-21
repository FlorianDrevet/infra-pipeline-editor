using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceAbbreviation;

/// <summary>Command to remove a per-resource-type abbreviation from a project.</summary>
public record RemoveProjectResourceAbbreviationCommand(
    ProjectId ProjectId,
    string ResourceType
) : ICommand<Deleted>;
