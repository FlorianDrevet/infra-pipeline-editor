using InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;
using InfraFlowSculptor.Application.RedisCaches.Commands.DeleteRedisCache;
using InfraFlowSculptor.Application.RedisCaches.Commands.UpdateRedisCache;
using InfraFlowSculptor.Application.RedisCaches.Queries;
using InfraFlowSculptor.Contracts.RedisCaches.Requests;
using InfraFlowSculptor.Contracts.RedisCaches.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

public static class RedisCacheController
{
    public static IEndpointRouteBuilder MapRedisCacheEndpoints(this IEndpointRouteBuilder app)
    {
        var config = app.MapGroup("/redis-cache")
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
            .WithName("GetRedisCache")
            .WithSummary("Get a Redis Cache")
            .WithDescription("Returns the full details of a single Azure Redis Cache resource.")
            .Produces<RedisCacheResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

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
            .WithName("CreateRedisCache")
            .WithSummary("Create a Redis Cache")
            .WithDescription("Creates a new Azure Redis Cache resource inside the specified Resource Group. Requires Owner or Contributor access.")
            .Produces<RedisCacheResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

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
            .WithName("UpdateRedisCache")
            .WithSummary("Update a Redis Cache")
            .WithDescription("Replaces all mutable properties of an existing Redis Cache. Requires Owner or Contributor access.")
            .Produces<RedisCacheResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

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
            .WithName("DeleteRedisCache")
            .WithSummary("Delete a Redis Cache")
            .WithDescription("Permanently deletes an Azure Redis Cache resource. Requires Owner or Contributor access.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
        return app;
    }
}
