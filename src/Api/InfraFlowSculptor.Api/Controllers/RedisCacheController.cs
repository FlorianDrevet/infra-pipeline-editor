using InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;
using InfraFlowSculptor.Application.RedisCaches.Commands.DeleteRedisCache;
using InfraFlowSculptor.Application.RedisCaches.Commands.UpdateRedisCache;
using InfraFlowSculptor.Application.RedisCaches.Queries;
using InfraFlowSculptor.Contracts.RedisCaches.Requests;
using InfraFlowSculptor.Contracts.RedisCaches.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

public static class RedisCacheController
{
    public static IApplicationBuilder UseRedisCacheController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var config = endpoints.MapGroup("/redis-cache")
                .WithTags("RedisCaches");

            config.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetRedisCacheQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            redisCache =>
                            {
                                var response = mapper.Map<RedisCacheResponse>(redisCache);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetRedisCache");

            config.MapPost("",
                    async (CreateRedisCacheRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateRedisCacheCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            redisCache =>
                            {
                                var response = mapper.Map<RedisCacheResponse>(redisCache);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "GetRedisCache",
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateRedisCache");

            config.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateRedisCacheRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateRedisCacheCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            redisCache =>
                            {
                                var response = mapper.Map<RedisCacheResponse>(redisCache);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateRedisCache");

            config.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteRedisCacheCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteRedisCache");
        });
    }
}
