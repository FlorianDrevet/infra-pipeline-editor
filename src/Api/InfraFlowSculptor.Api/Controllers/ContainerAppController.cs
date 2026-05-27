using InfraFlowSculptor.Application.ContainerApps.Commands.CreateContainerApp;
using InfraFlowSculptor.Application.ContainerApps.Commands.DeleteContainerApp;
using InfraFlowSculptor.Application.ContainerApps.Commands.UpdateContainerApp;
using InfraFlowSculptor.Application.ContainerApps.Queries.GetContainerApp;
using InfraFlowSculptor.Contracts.ContainerApps.Requests;
using InfraFlowSculptor.Contracts.ContainerApps.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

using InfraFlowSculptor.Api.Controllers.Constants;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoints for the Container App resource.</summary>
public static class ContainerAppController
{
    /// <summary>Registers the Container App endpoints under <c>/container-app</c>.</summary>
    public static IApplicationBuilder UseContainerAppController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup(Routes.ContainerApp)
                .WithTags("Container Apps");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetContainerAppQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            ca =>
                            {
                                var response = mapper.Map<ContainerAppResponse>(ca);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(ContainerAppRouteNames.GetContainerApp)
                .WithSummary("Get a Container App")
                .WithDescription("Returns the full details of a single Azure Container App resource.")
                .Produces<ContainerAppResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreateContainerAppRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateContainerAppCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            ca =>
                            {
                                var response = mapper.Map<ContainerAppResponse>(ca);
                                return TypedResults.CreatedAtRoute(
                                    routeName: ContainerAppRouteNames.GetContainerApp,
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(ContainerAppRouteNames.CreateContainerApp)
                .WithSummary("Create a Container App")
                .WithDescription("Creates a new Azure Container App resource inside the specified Resource Group.")
                .Produces<ContainerAppResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateContainerAppRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateContainerAppCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            ca =>
                            {
                                var response = mapper.Map<ContainerAppResponse>(ca);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(ContainerAppRouteNames.UpdateContainerApp)
                .WithSummary("Update a Container App")
                .WithDescription("Replaces all mutable properties of an existing Container App.")
                .Produces<ContainerAppResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteContainerAppCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName(ContainerAppRouteNames.DeleteContainerApp)
                .WithSummary("Delete a Container App")
                .WithDescription("Permanently deletes an Azure Container App resource.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}

