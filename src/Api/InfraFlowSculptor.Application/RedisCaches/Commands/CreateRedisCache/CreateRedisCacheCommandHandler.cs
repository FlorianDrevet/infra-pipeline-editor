using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;

public class CreateRedisCacheCommandHandler(IRedisCacheRepository redisCacheRepository, IMapper mapper)
    : IRequestHandler<CreateRedisCacheCommand, ErrorOr<RedisCacheResult>>
{
    public async Task<ErrorOr<RedisCacheResult>> Handle(CreateRedisCacheCommand request, CancellationToken cancellationToken)
    {
        var settings = new RedisCacheSettings(
            request.Capacity,
            request.RedisVersion,
            request.EnableNonSslPort,
            request.MinimumTlsVersion,
            request.MaxMemoryPolicy);

        var redisCache = RedisCache.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.Sku,
            settings);

        var savedRedisCache = await redisCacheRepository.AddAsync(redisCache);

        return mapper.Map<RedisCacheResult>(savedRedisCache);
    }
}
