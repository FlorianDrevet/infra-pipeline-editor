using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceAbbreviation;

/// <summary>Command to set or update a per-resource-type abbreviation at the project level.</summary>
public record SetProjectResourceAbbreviationCommand(
    ProjectId ProjectId,
    string ResourceType,
    string Abbreviation
) : ICommand<ProjectResourceAbbreviationResult>;
