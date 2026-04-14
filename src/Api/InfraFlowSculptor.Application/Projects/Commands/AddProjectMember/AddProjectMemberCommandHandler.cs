using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectMember;

/// <summary>Handles the <see cref="AddProjectMemberCommand"/>.</summary>
public sealed class AddProjectMemberCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository projectRepository,
    IMapper mapper)
    : ICommandHandler<AddProjectMemberCommand, ProjectResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ProjectResult>> Handle(
        AddProjectMemberCommand command,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyOwnerAccessAsync(command.ProjectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var project = accessResult.Value;

        var targetUserId = UserId.Create(command.UserId);
        if (project.Members.Any(m => m.UserId == targetUserId))
            return Errors.Project.MemberAlreadyExistsError();

        if (!Enum.TryParse<Role.RoleEnum>(command.Role, out var roleEnum))
            return Errors.Project.InvalidRoleError(command.Role);

        project.AddMember(targetUserId, new Role(roleEnum));
        var saved = await projectRepository.UpdateAsync(project);

        return mapper.Map<ProjectResult>(saved);
    }
}
