using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

internal static class MemberCommandHelper
{
    /// <summary>
    /// Loads the config, verifies the calling user is an Owner, and finds the target member.
    /// Returns the loaded aggregate and the resolved target UserId, or an error.
    /// </summary>
    internal static async Task<ErrorOr<(Domain.InfrastructureConfigAggregate.InfrastructureConfig Config, UserId TargetUserId)>> AuthorizeOwnerAndFindTargetAsync(
        IInfrastructureConfigRepository repository,
        ICurrentUser currentUser,
        InfrastructureConfigId infraConfigId,
        Guid targetUserIdRaw,
        CancellationToken cancellationToken)
    {
        var currentUserId = await currentUser.GetUserIdAsync(cancellationToken);
        var infraConfig = await repository.GetByIdWithMembersAsync(infraConfigId, cancellationToken);

        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(infraConfigId);

        var currentMember = infraConfig.Members.FirstOrDefault(m => m.UserId == currentUserId);
        if (currentMember is null || currentMember.Role.Value != Role.RoleEnum.Owner)
            return Errors.Member.ForbiddenError();

        var targetUserId = new UserId(targetUserIdRaw);
        var targetMember = infraConfig.Members.FirstOrDefault(m => m.UserId == targetUserId);
        if (targetMember is null)
            return Errors.Member.NotFoundError(targetUserId);

        return (infraConfig, targetUserId);
    }
}
