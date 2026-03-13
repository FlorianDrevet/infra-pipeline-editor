using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;

public class CreateRedisCacheCommandHandler(IRedisCacheRepository redisCacheRepository, IMapper mapper)
    : IRequestHandler<CreateRedisCacheCommand, ErrorOr<RedisCacheResult>>
{
    public async Task<ErrorOr<RedisCacheResult>> Handle(CreateRedisCacheCommand request, CancellationToken cancellationToken)
    {
        var redisCache = RedisCache.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.Sku,
            request.Capacity,
            request.RedisVersion,
            request.EnableNonSslPort,
            request.MinimumTlsVersion,
            request.MaxMemoryPolicy);

        var savedRedisCache = await redisCacheRepository.AddAsync(redisCache);

        return mapper.Map<RedisCacheResult>(savedRedisCache);
    }
}
