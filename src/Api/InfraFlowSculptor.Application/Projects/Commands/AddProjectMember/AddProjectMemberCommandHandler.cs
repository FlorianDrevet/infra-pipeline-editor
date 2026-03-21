using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectMember;

/// <summary>Handles the <see cref="AddProjectMemberCommand"/> request.</summary>
public sealed class AddProjectMemberCommandHandler(
    IProjectAccessService accessService,
    IProjectRepository repository,
    IMapper mapper)
    : IRequestHandler<AddProjectMemberCommand, ErrorOr<ProjectResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ProjectResult>> Handle(
        AddProjectMemberCommand command, CancellationToken cancellationToken)
    {
        var projectId = new ProjectId(command.ProjectId);
        var accessResult = await accessService.VerifyWriteAccessAsync(projectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var project = accessResult.Value;
        var targetUserId = new UserId(command.UserId);

        if (project.Members.Any(m => m.UserId == targetUserId))
            return Errors.Project.AlreadyMemberError(targetUserId);

        var role = new ProjectRole(Enum.Parse<ProjectRole.ProjectRoleEnum>(command.Role));
        project.AddMember(targetUserId, role);

        var updated = await repository.UpdateAsync(project);
        return mapper.Map<ProjectResult>(updated);
    }
}
