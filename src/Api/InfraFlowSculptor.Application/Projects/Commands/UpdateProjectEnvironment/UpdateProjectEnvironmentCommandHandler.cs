using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Location = InfraFlowSculptor.Domain.Common.ValueObjects.Location;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Projects.Commands.UpdateProjectEnvironment;

/// <summary>Handles the <see cref="UpdateProjectEnvironmentCommand"/>.</summary>
public sealed class UpdateProjectEnvironmentCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService,
    IMapper mapper)
    : IRequestHandler<UpdateProjectEnvironmentCommand, ErrorOr<ProjectEnvironmentDefinitionResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ProjectEnvironmentDefinitionResult>> Handle(
        UpdateProjectEnvironmentCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.ProjectId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var project = await projectRepository.GetByIdWithAllAsync(command.ProjectId, cancellationToken);
        if (project is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        var tags = command.Tags.Select(t => new Tag(t.Name, t.Value));

        var data = new EnvironmentDefinitionData(
            new Name(command.Name),
            new ShortName(command.ShortName),
            new Prefix(command.Prefix),
            new Suffix(command.Suffix),
            new Location(Enum.Parse<Location.LocationEnum>(command.Location, ignoreCase: true)),
            new TenantId(command.TenantId),
            new SubscriptionId(command.SubscriptionId),
            new Order(command.Order),
            new RequiresApproval(command.RequiresApproval),
            tags);

        var env = project.UpdateEnvironment(command.EnvironmentId, data);
        if (env is null)
            return Errors.Project.NotFoundError(command.ProjectId);

        await projectRepository.UpdateAsync(project);

        return mapper.Map<ProjectEnvironmentDefinitionResult>(env);
    }
}
