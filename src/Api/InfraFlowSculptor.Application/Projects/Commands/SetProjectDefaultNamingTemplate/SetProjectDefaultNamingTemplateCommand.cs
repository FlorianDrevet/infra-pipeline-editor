using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectDefaultNamingTemplate;

/// <summary>Command to set or clear the project-level default naming template.</summary>
public record SetProjectDefaultNamingTemplateCommand(
    ProjectId ProjectId,
    string? Template
) : IRequest<ErrorOr<Success>>;
