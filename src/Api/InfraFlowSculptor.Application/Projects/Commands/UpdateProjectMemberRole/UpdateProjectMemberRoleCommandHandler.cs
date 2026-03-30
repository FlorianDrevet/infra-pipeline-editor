using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.UpdateProjectMemberRole;

/// <summary>Handles the <see cref="UpdateProjectMemberRoleCommand"/>.</summary>
public sealed class UpdateProjectMemberRoleCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IMapper mapper)
    : ICommandHandler<UpdateProjectMemberRoleCommand, ProjectResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ProjectResult>> Handle(
        UpdateProjectMemberRoleCommand command,
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

        if (!Enum.TryParse<Role.RoleEnum>(command.NewRole, out var roleEnum))
            return Error.Validation("Project.InvalidRole", $"Invalid role: {command.NewRole}");

        project.ChangeRole(targetUserId, new Role(roleEnum));
        var saved = await projectRepository.UpdateAsync(project);

        return mapper.Map<ProjectResult>(saved);
    }
}
