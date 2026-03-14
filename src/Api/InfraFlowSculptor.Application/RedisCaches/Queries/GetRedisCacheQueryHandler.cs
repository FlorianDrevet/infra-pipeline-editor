using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.RedisCaches.Queries;

public class GetRedisCacheQueryHandler(IRedisCacheRepository redisCacheRepository, IMapper mapper)
    : IRequestHandler<GetRedisCacheQuery, ErrorOr<RedisCacheResult>>
{
    public async Task<ErrorOr<RedisCacheResult>> Handle(GetRedisCacheQuery query, CancellationToken cancellationToken)
    {
        var redisCache = await redisCacheRepository.GetByIdAsync(query.Id, cancellationToken);

        if (redisCache is null)
            return Errors.RedisCache.NotFoundError(query.Id);

        return mapper.Map<RedisCacheResult>(redisCache);
    }
}
