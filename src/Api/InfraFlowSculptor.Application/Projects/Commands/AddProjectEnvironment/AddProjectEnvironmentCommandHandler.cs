using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Location = InfraFlowSculptor.Domain.Common.ValueObjects.Location;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectEnvironment;

/// <summary>Handles the <see cref="AddProjectEnvironmentCommand"/>.</summary>
public sealed class AddProjectEnvironmentCommandHandler(
    IProjectRepository projectRepository,
    IProjectAccessService accessService,
    IMapper mapper)
    : IRequestHandler<AddProjectEnvironmentCommand, ErrorOr<ProjectEnvironmentDefinitionResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ProjectEnvironmentDefinitionResult>> Handle(
        AddProjectEnvironmentCommand command, CancellationToken cancellationToken)
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

        var env = project.AddEnvironment(data);

        await projectRepository.UpdateAsync(project);

        return mapper.Map<ProjectEnvironmentDefinitionResult>(env);
    }
}
