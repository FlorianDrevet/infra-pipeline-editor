using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateMemberRole;

public class UpdateMemberRoleCommandHandler(
    IInfrastructureConfigRepository repository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<UpdateMemberRoleCommand, ErrorOr<GetInfrastructureConfigResult>>
{
    public async Task<ErrorOr<GetInfrastructureConfigResult>> Handle(
        UpdateMemberRoleCommand command, CancellationToken cancellationToken)
    {
        var currentUserId = await currentUser.GetUserIdAsync(cancellationToken);
        var infraConfig = await repository.GetByIdWithMembersAsync(command.InfraConfigId, cancellationToken);

        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(command.InfraConfigId);

        var currentMember = infraConfig.Members.FirstOrDefault(m => m.UserId == currentUserId);
        if (currentMember is null || currentMember.Role.Value != Role.RoleEnum.Owner)
            return Errors.Member.ForbiddenError();

        var targetUserId = new UserId(command.TargetUserId);
        var targetMember = infraConfig.Members.FirstOrDefault(m => m.UserId == targetUserId);
        if (targetMember is null)
            return Errors.Member.NotFoundError(targetUserId);

        var newRole = new Role(Enum.Parse<Role.RoleEnum>(command.NewRole, ignoreCase: true));
        infraConfig.ChangeRole(targetUserId, newRole);

        var saved = await repository.UpdateAsync(infraConfig);
        return mapper.Map<GetInfrastructureConfigResult>(saved);
    }
}
