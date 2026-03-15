using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RedisCaches.Common;
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
        var authResult = await RedisCacheAccessHelper.GetWithWriteAccessAsync(
            request.Id, redisCacheRepository, resourceGroupRepository, infraConfigRepository, currentUser, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        await redisCacheRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}
