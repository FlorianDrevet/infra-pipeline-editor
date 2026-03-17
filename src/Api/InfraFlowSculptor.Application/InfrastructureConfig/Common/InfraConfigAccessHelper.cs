using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

internal sealed class InfraConfigAccessService(
    IInfrastructureConfigRepository repository,
    ICurrentUser currentUser)
    : IInfraConfigAccessService
{
    public async Task<ErrorOr<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> VerifyReadAccessAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var infraConfig = await repository.GetByIdWithMembersAsync(infraConfigId, cancellationToken);

        if (infraConfig is null || !infraConfig.Members.Any(m => m.UserId == userId))
            return Errors.InfrastructureConfig.NotFoundError(infraConfigId);

        return infraConfig;
    }

    public async Task<ErrorOr<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> VerifyWriteAccessAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var infraConfig = await repository.GetByIdWithMembersAsync(infraConfigId, cancellationToken);

        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(infraConfigId);

        var member = infraConfig.Members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
            return Errors.InfrastructureConfig.NotFoundError(infraConfigId);

        if (member.Role.Value == Role.RoleEnum.Reader)
            return Errors.InfrastructureConfig.ForbiddenError();

        return infraConfig;
    }
}
