using ErrorOr;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.RedisCaches.Queries;

public record GetRedisCacheQuery(
    AzureResourceId Id
) : IRequest<ErrorOr<RedisCacheResult>>;
