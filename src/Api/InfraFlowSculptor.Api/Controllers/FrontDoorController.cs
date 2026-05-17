using InfraFlowSculptor.Api.Controllers.Constants;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.FrontDoors.Commands.CreateFrontDoor;
using InfraFlowSculptor.Application.FrontDoors.Commands.DeleteFrontDoor;
using InfraFlowSculptor.Application.FrontDoors.Commands.UpdateFrontDoor;
using InfraFlowSculptor.Application.FrontDoors.Queries;
using InfraFlowSculptor.Contracts.FrontDoors.Requests;
using InfraFlowSculptor.Contracts.FrontDoors.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoints for Front Door resources.</summary>
public static class FrontDoorController
{
    /// <summary>Registers Front Door endpoints.</summary>
    public static IApplicationBuilder UseFrontDoorController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/front-door")
                .WithTags("FrontDoors");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetFrontDoorQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);
                        return result.Match(
                            fd => TypedResults.Ok(mapper.Map<FrontDoorResponse>(fd)),
                            errors => errors.Result());
                    })
                .WithName(FrontDoorRouteNames.GetFrontDoor)
                .WithSummary("Get a Front Door")
                .Produces<FrontDoorResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreateFrontDoorRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateFrontDoorCommand>(request);
                        var result = await mediator.Send(command);
                        return result.Match(
                            fd =>
                            {
                                var response = mapper.Map<FrontDoorResponse>(fd);
                                return TypedResults.CreatedAtRoute(
                                    routeName: FrontDoorRouteNames.GetFrontDoor,
                                    routeValues: new { id = response.Id },
                                    value: response);
                            },
                            errors => errors.Result());
                    })
                .WithName(FrontDoorRouteNames.CreateFrontDoor)
                .WithSummary("Create a Front Door")
                .Produces<FrontDoorResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateFrontDoorRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateFrontDoorCommand>((id, request));
                        var result = await mediator.Send(command);
                        return result.Match(
                            fd => TypedResults.Ok(mapper.Map<FrontDoorResponse>(fd)),
                            errors => errors.Result());
                    })
                .WithName(FrontDoorRouteNames.UpdateFrontDoor)
                .WithSummary("Update a Front Door")
                .Produces<FrontDoorResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteFrontDoorCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);
                        return result.Match(_ => Results.NoContent(), errors => errors.Result());
                    })
                .WithName(FrontDoorRouteNames.DeleteFrontDoor)
                .WithSummary("Delete a Front Door")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
