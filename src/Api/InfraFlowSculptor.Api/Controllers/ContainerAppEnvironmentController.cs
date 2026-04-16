using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.CreateContainerAppEnvironment;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.DeleteContainerAppEnvironment;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.UpdateContainerAppEnvironment;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Queries.GetContainerAppEnvironment;
using InfraFlowSculptor.Contracts.ContainerAppEnvironments.Requests;
using InfraFlowSculptor.Contracts.ContainerAppEnvironments.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoints for the Container App Environment resource.</summary>
public static class ContainerAppEnvironmentController
{
    /// <summary>Registers the Container App Environment endpoints under <c>/container-app-environment</c>.</summary>
    public static IEndpointRouteBuilder MapContainerAppEnvironmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/container-app-environment")
            .WithTags("Container App Environments");

        group.MapGet("/{id:guid}",
                async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                {
                    var query = new GetContainerAppEnvironmentQuery(new AzureResourceId(id));
                    var result = await mediator.Send(query);

                    return result.Match(
                        cae =>
                        {
                            var response = mapper.Map<ContainerAppEnvironmentResponse>(cae);
                            return TypedResults.Ok(response);
                        },
                        errors => errors.Result()
                    );
                })
            .WithName("GetContainerAppEnvironment")
            .WithSummary("Get a Container App Environment")
            .WithDescription("Returns the full details of a single Azure Container App Environment resource.")
            .Produces<ContainerAppEnvironmentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("",
                async (CreateContainerAppEnvironmentRequest request, IMediator mediator, IMapper mapper) =>
                {
                    var command = mapper.Map<CreateContainerAppEnvironmentCommand>(request);
                    var result = await mediator.Send(command);

                    return result.Match(
                        cae =>
                        {
                            var response = mapper.Map<ContainerAppEnvironmentResponse>(cae);
                            return TypedResults.CreatedAtRoute(
                                routeName: "GetContainerAppEnvironment",
                                routeValues: new { id = response.Id },
                                value: response
                            );
                        },
                        errors => errors.Result()
                    );
                })
            .WithName("CreateContainerAppEnvironment")
            .WithSummary("Create a Container App Environment")
            .WithDescription("Creates a new Azure Container App Environment resource inside the specified Resource Group.")
            .Produces<ContainerAppEnvironmentResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}",
                async ([FromRoute] Guid id, UpdateContainerAppEnvironmentRequest request, IMediator mediator, IMapper mapper) =>
                {
                    var command = mapper.Map<UpdateContainerAppEnvironmentCommand>((id, request));
                    var result = await mediator.Send(command);

                    return result.Match(
                        cae =>
                        {
                            var response = mapper.Map<ContainerAppEnvironmentResponse>(cae);
                            return TypedResults.Ok(response);
                        },
                        errors => errors.Result()
                    );
                })
            .WithName("UpdateContainerAppEnvironment")
            .WithSummary("Update a Container App Environment")
            .WithDescription("Replaces all mutable properties of an existing Container App Environment.")
            .Produces<ContainerAppEnvironmentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}",
                async ([FromRoute] Guid id, IMediator mediator) =>
                {
                    var command = new DeleteContainerAppEnvironmentCommand(new AzureResourceId(id));
                    var result = await mediator.Send(command);

                    return result.Match(
                        _ => Results.NoContent(),
                        errors => errors.Result()
                    );
                })
            .WithName("DeleteContainerAppEnvironment")
            .WithSummary("Delete a Container App Environment")
            .WithDescription("Permanently deletes an Azure Container App Environment resource.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
        return app;
    }
}
