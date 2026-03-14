using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.DeleteRedisCache;

public class DeleteRedisCacheCommandHandler(IRedisCacheRepository redisCacheRepository)
    : IRequestHandler<DeleteRedisCacheCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteRedisCacheCommand request, CancellationToken cancellationToken)
    {
        var deleted = await redisCacheRepository.DeleteAsync(request.Id);

        if (!deleted)
            return Errors.RedisCache.NotFoundError(request.Id);

        return Result.Deleted;
    }
}
