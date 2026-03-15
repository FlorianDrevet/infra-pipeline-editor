using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveMember;

public class RemoveMemberCommandHandler(
    IInfrastructureConfigRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<RemoveMemberCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveMemberCommand command, CancellationToken cancellationToken)
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

        if (targetMember.Role.Value == Role.RoleEnum.Owner)
            return Errors.Member.CannotRemoveOwnerError();

        infraConfig.RemoveMember(targetUserId);
        await repository.UpdateAsync(infraConfig);

        return Result.Deleted;
    }
}
