using InfraFlowSculptor.Application.CosmosDbs.Commands.CreateCosmosDb;
using InfraFlowSculptor.Application.CosmosDbs.Commands.DeleteCosmosDb;
using InfraFlowSculptor.Application.CosmosDbs.Commands.UpdateCosmosDb;
using InfraFlowSculptor.Application.CosmosDbs.Queries;
using InfraFlowSculptor.Contracts.CosmosDbs.Requests;
using InfraFlowSculptor.Contracts.CosmosDbs.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoints for the Cosmos DB resource.</summary>
public static class CosmosDbController
{
    /// <summary>Registers the Cosmos DB endpoints under <c>/cosmos-db</c>.</summary>
    public static IEndpointRouteBuilder MapCosmosDbEndpoints(this IEndpointRouteBuilder app)
    {
        var config = app.MapGroup("/cosmos-db")
            .WithTags("Cosmos DB");

        config.MapGet("/{id:guid}",
                async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                {
                    var query = new GetCosmosDbQuery(new AzureResourceId(id));
                    var result = await mediator.Send(query);

                    return result.Match(
                        cosmosDb =>
                        {
                            var response = mapper.Map<CosmosDbResponse>(cosmosDb);
                            return TypedResults.Ok(response);
                        },
                        errors => errors.Result()
                    );
                })
            .WithName("GetCosmosDb")
            .WithSummary("Get a Cosmos DB account")
            .WithDescription("Returns the full details of a single Azure Cosmos DB database account resource.")
            .Produces<CosmosDbResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        config.MapPost("",
                async (CreateCosmosDbRequest request, IMediator mediator, IMapper mapper) =>
                {
                    var command = mapper.Map<CreateCosmosDbCommand>(request);
                    var result = await mediator.Send(command);

                    return result.Match(
                        cosmosDb =>
                        {
                            var response = mapper.Map<CosmosDbResponse>(cosmosDb);
                            return TypedResults.CreatedAtRoute(
                                routeName: "GetCosmosDb",
                                routeValues: new { id = response.Id },
                                value: response
                            );
                        },
                        errors => errors.Result()
                    );
                })
            .WithName("CreateCosmosDb")
            .WithSummary("Create a Cosmos DB account")
            .WithDescription("Creates a new Azure Cosmos DB database account resource inside the specified Resource Group. Requires Owner or Contributor access.")
            .Produces<CosmosDbResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        config.MapPut("/{id:guid}",
                async ([FromRoute] Guid id, UpdateCosmosDbRequest request, IMediator mediator, IMapper mapper) =>
                {
                    var command = mapper.Map<UpdateCosmosDbCommand>((id, request));
                    var result = await mediator.Send(command);

                    return result.Match(
                        cosmosDb =>
                        {
                            var response = mapper.Map<CosmosDbResponse>(cosmosDb);
                            return TypedResults.Ok(response);
                        },
                        errors => errors.Result()
                    );
                })
            .WithName("UpdateCosmosDb")
            .WithSummary("Update a Cosmos DB account")
            .WithDescription("Replaces all mutable properties of an existing Cosmos DB account. Requires Owner or Contributor access.")
            .Produces<CosmosDbResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        config.MapDelete("/{id:guid}",
                async ([FromRoute] Guid id, IMediator mediator) =>
                {
                    var command = new DeleteCosmosDbCommand(new AzureResourceId(id));
                    var result = await mediator.Send(command);

                    return result.Match(
                        _ => Results.NoContent(),
                        errors => errors.Result()
                    );
                })
            .WithName("DeleteCosmosDb")
            .WithSummary("Delete a Cosmos DB account")
            .WithDescription("Permanently deletes an Azure Cosmos DB database account resource. Requires Owner or Contributor access.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
        return app;
    }
}
