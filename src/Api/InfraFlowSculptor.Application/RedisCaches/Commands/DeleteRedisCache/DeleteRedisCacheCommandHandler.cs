using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.DeleteRedisCache;

public class DeleteRedisCacheCommandHandler(
    IRedisCacheRepository redisCacheRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteRedisCacheCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteRedisCacheCommand request, CancellationToken cancellationToken)
    {
        var redisCache = await redisCacheRepository.GetByIdAsync(request.Id, cancellationToken);
        if (redisCache is null)
            return Errors.RedisCache.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(redisCache.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.RedisCache.NotFoundError(request.Id);

        var authResult = await InfraConfigAccessHelper.VerifyWriteAccessAsync(
            infraConfigRepository, currentUser, resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        await redisCacheRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}
