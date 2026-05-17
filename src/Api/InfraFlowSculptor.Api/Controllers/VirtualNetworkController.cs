using InfraFlowSculptor.Api.Controllers.Constants;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.VirtualNetworks.Commands.CreateVirtualNetwork;
using InfraFlowSculptor.Application.VirtualNetworks.Commands.DeleteVirtualNetwork;
using InfraFlowSculptor.Application.VirtualNetworks.Commands.UpdateVirtualNetwork;
using InfraFlowSculptor.Application.VirtualNetworks.Queries;
using InfraFlowSculptor.Contracts.VirtualNetworks.Requests;
using InfraFlowSculptor.Contracts.VirtualNetworks.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoints for Virtual Network resources.</summary>
public static class VirtualNetworkController
{
    /// <summary>Registers Virtual Network endpoints.</summary>
    public static IApplicationBuilder UseVirtualNetworkController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/virtual-network")
                .WithTags("VirtualNetworks");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetVirtualNetworkQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            vnet =>
                            {
                                var response = mapper.Map<VirtualNetworkResponse>(vnet);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(VirtualNetworkRouteNames.GetVirtualNetwork)
                .WithSummary("Get a Virtual Network")
                .WithDescription("Returns the full details of a single Azure Virtual Network resource.")
                .Produces<VirtualNetworkResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreateVirtualNetworkRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateVirtualNetworkCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            vnet =>
                            {
                                var response = mapper.Map<VirtualNetworkResponse>(vnet);
                                return TypedResults.CreatedAtRoute(
                                    routeName: VirtualNetworkRouteNames.GetVirtualNetwork,
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(VirtualNetworkRouteNames.CreateVirtualNetwork)
                .WithSummary("Create a Virtual Network")
                .WithDescription("Creates a new Azure Virtual Network resource inside the specified Resource Group.")
                .Produces<VirtualNetworkResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateVirtualNetworkRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateVirtualNetworkCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            vnet =>
                            {
                                var response = mapper.Map<VirtualNetworkResponse>(vnet);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(VirtualNetworkRouteNames.UpdateVirtualNetwork)
                .WithSummary("Update a Virtual Network")
                .WithDescription("Replaces all mutable properties of an existing Virtual Network.")
                .Produces<VirtualNetworkResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteVirtualNetworkCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName(VirtualNetworkRouteNames.DeleteVirtualNetwork)
                .WithSummary("Delete a Virtual Network")
                .WithDescription("Permanently deletes an Azure Virtual Network resource.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
