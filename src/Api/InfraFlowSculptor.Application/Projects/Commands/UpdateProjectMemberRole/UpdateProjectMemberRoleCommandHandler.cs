using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.UpdateProjectMemberRole;

/// <summary>Handles the <see cref="UpdateProjectMemberRoleCommand"/> request.</summary>
public sealed class UpdateProjectMemberRoleCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository repository,
    IMapper mapper)
    : IRequestHandler<UpdateProjectMemberRoleCommand, ErrorOr<ProjectResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ProjectResult>> Handle(
        UpdateProjectMemberRoleCommand command, CancellationToken cancellationToken)
    {
        var projectId = new ProjectId(command.ProjectId);
        var accessResult = await accessService.VerifyWriteAccessAsync(projectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var project = accessResult.Value;
        var targetUserId = new UserId(command.UserId);

        if (project.Members.All(m => m.UserId != targetUserId))
            return Errors.Project.MemberNotFoundError(targetUserId);

        var newRole = new ProjectRole(Enum.Parse<ProjectRole.ProjectRoleEnum>(command.NewRole));
        project.ChangeRole(targetUserId, newRole);

        var updated = await repository.UpdateAsync(project);
        return mapper.Map<ProjectResult>(updated);
    }
}
