using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectMember;

/// <summary>Handles the <see cref="RemoveProjectMemberCommand"/>.</summary>
public sealed class RemoveProjectMemberCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository)
    : ICommandHandler<RemoveProjectMemberCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveProjectMemberCommand command,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var project = accessResult.Value;

        var targetUserId = UserId.Create(command.UserId);
        var member = project.Members.FirstOrDefault(m => m.UserId == targetUserId);
        if (member is null)
            return Errors.Project.MemberNotFoundError();

        if (member.Role.Value == Role.RoleEnum.Owner)
            return Errors.Project.CannotRemoveOwnerError();

        project.RemoveMember(targetUserId);
        await projectRepository.UpdateAsync(project);

        return Result.Deleted;
    }
}
