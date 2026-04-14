using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectDefaultNamingTemplate;

/// <summary>Command to set or clear the project-level default naming template.</summary>
public record SetProjectDefaultNamingTemplateCommand(
    ProjectId ProjectId,
    string? Template
) : ICommand<Success>;
