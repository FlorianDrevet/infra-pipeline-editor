using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.CreateProject;

/// <summary>Handles the <see cref="CreateProjectCommand"/>.</summary>
public sealed class CreateProjectCommandHandler(
    IProjectRepository repository,
    ICurrentUser currentUser)
    : ICommandHandler<CreateProjectCommand, ProjectResult>
{
    /// <summary>Default naming template applied to every new project.</summary>
    private const string DefaultTemplate = "{name}-{resourceAbbr}{suffix}";

    /// <summary>Per-resource-type naming template overrides applied on project creation.</summary>
    private static readonly Dictionary<string, string> DefaultResourceTemplates = new()
    {
        ["ResourceGroup"] = "{resourceAbbr}-{name}{suffix}",
        ["StorageAccount"] = "{name}{resourceAbbr}{envShort}",
    };

    /// <inheritdoc />
    public async Task<ErrorOr<ProjectResult>> Handle(
        CreateProjectCommand command,
        CancellationToken cancellationToken)
    {
        var nameVo = new Name(command.Name);
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        var project = Project.Create(nameVo, command.Description, userId);

        project.SetDefaultNamingTemplate(new NamingTemplate(DefaultTemplate));

        foreach (var (resourceType, template) in DefaultResourceTemplates)
        {
            project.SetResourceNamingTemplate(resourceType, new NamingTemplate(template));
        }

        var saved = await repository.AddAsync(project);

        return ProjectResultMapper.ToProjectResult(saved);
    }
}
