using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
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
        var authResult = await MemberCommandHelper.AuthorizeOwnerAndFindTargetAsync(
            repository, currentUser, command.InfraConfigId, command.TargetUserId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var (infraConfig, targetUserId) = authResult.Value;

        var targetMember = infraConfig.Members.First(m => m.UserId == targetUserId);
        if (targetMember.Role.Value == Role.RoleEnum.Owner)
            return Errors.Member.CannotRemoveOwnerError();

        infraConfig.RemoveMember(targetUserId);
        await repository.UpdateAsync(infraConfig);

        return Result.Deleted;
    }
}
