using InfraFlowSculptor.Application.ContainerRegistries.Commands.CreateContainerRegistry;
using InfraFlowSculptor.Application.ContainerRegistries.Commands.DeleteContainerRegistry;
using InfraFlowSculptor.Application.ContainerRegistries.Commands.UpdateContainerRegistry;
using InfraFlowSculptor.Application.ContainerRegistries.Queries.CheckAcrPullAccess;
using InfraFlowSculptor.Application.ContainerRegistries.Queries.GetContainerRegistry;
using InfraFlowSculptor.Contracts.ContainerRegistries.Requests;
using InfraFlowSculptor.Contracts.ContainerRegistries.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoints for the Container Registry resource.</summary>
public static class ContainerRegistryController
{
    /// <summary>Registers the Container Registry endpoints under <c>/container-registry</c>.</summary>
    public static IApplicationBuilder UseContainerRegistryController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/container-registry")
                .WithTags("Container Registries");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetContainerRegistryQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            cr =>
                            {
                                var response = mapper.Map<ContainerRegistryResponse>(cr);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetContainerRegistry")
                .WithSummary("Get a Container Registry")
                .WithDescription("Returns the full details of a single Azure Container Registry resource.")
                .Produces<ContainerRegistryResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreateContainerRegistryRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateContainerRegistryCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            cr =>
                            {
                                var response = mapper.Map<ContainerRegistryResponse>(cr);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "GetContainerRegistry",
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateContainerRegistry")
                .WithSummary("Create a Container Registry")
                .WithDescription("Creates a new Azure Container Registry resource inside the specified Resource Group.")
                .Produces<ContainerRegistryResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateContainerRegistryRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateContainerRegistryCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            cr =>
                            {
                                var response = mapper.Map<ContainerRegistryResponse>(cr);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateContainerRegistry")
                .WithSummary("Update a Container Registry")
                .WithDescription("Replaces all mutable properties of an existing Container Registry.")
                .Produces<ContainerRegistryResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteContainerRegistryCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteContainerRegistry")
                .WithSummary("Delete a Container Registry")
                .WithDescription("Permanently deletes an Azure Container Registry resource.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // Endpoint to check whether a compute resource has AcrPull access on a Container Registry
            var acrAccessGroup = endpoints.MapGroup("/azure-resources/{resourceId:guid}/check-acr-pull-access")
                .WithTags("Container Registries");

            acrAccessGroup.MapGet("/{containerRegistryId:guid}",
                    async ([FromRoute] Guid resourceId, [FromRoute] Guid containerRegistryId, [FromQuery] string? acrAuthMode,
                        IMediator mediator, IMapper mapper) =>
                    {
                        var query = new CheckAcrPullAccessQuery(
                            new AzureResourceId(resourceId),
                            new AzureResourceId(containerRegistryId),
                            acrAuthMode);
                        var result = await mediator.Send(query);

                        return result.Match(
                            access => Results.Ok(mapper.Map<CheckAcrPullAccessResponse>(access)),
                            errors => errors.Result());
                    })
                .WithName("CheckAcrPullAccess")
                .WithSummary("Check ACR Pull access")
                .WithDescription("Checks whether a compute resource has the AcrPull role assignment on the specified Container Registry.")
                .Produces<CheckAcrPullAccessResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
