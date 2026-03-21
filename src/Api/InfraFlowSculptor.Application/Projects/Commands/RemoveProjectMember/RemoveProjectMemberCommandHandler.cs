using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectMember;

/// <summary>Handles the <see cref="RemoveProjectMemberCommand"/> request.</summary>
public sealed class RemoveProjectMemberCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository repository)
    : IRequestHandler<RemoveProjectMemberCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveProjectMemberCommand command, CancellationToken cancellationToken)
    {
        var projectId = new ProjectId(command.ProjectId);
        var accessResult = await accessService.VerifyWriteAccessAsync(projectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var project = accessResult.Value;
        var targetUserId = new UserId(command.UserId);

        var member = project.Members.FirstOrDefault(m => m.UserId == targetUserId);
        if (member is null)
            return Errors.Project.MemberNotFoundError(targetUserId);

        // Prevent removing the last owner
        if (member.Role.Value == ProjectRole.ProjectRoleEnum.Owner
            && project.Members.Count(m => m.Role.Value == ProjectRole.ProjectRoleEnum.Owner) == 1)
            return Errors.Project.CannotRemoveOwnerError();

        project.RemoveMember(targetUserId);
        await repository.UpdateAsync(project);

        return Result.Deleted;
    }
}
