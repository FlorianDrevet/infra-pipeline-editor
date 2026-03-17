using ErrorOr;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.RedisCaches.Queries;

public record ListRedisCachesQuery(
    ResourceGroupId ResourceGroupId
) : IRequest<ErrorOr<List<RedisCacheResult>>>;
