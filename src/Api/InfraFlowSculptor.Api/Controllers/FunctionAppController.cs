using InfraFlowSculptor.Application.FunctionApps.Commands.CreateFunctionApp;
using InfraFlowSculptor.Application.FunctionApps.Commands.DeleteFunctionApp;
using InfraFlowSculptor.Application.FunctionApps.Commands.UpdateFunctionApp;
using InfraFlowSculptor.Application.FunctionApps.Queries;
using MediatR;
using InfraFlowSculptor.Contracts.FunctionApps.Requests;
using InfraFlowSculptor.Contracts.FunctionApps.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoint definitions for the Function App feature.</summary>
public static class FunctionAppController
{
    /// <summary>Registers the Function App endpoints on the application builder.</summary>
    public static IApplicationBuilder UseFunctionAppController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/function-app")
                .WithTags("Function Apps");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetFunctionAppQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            functionApp =>
                            {
                                var response = mapper.Map<FunctionAppResponse>(functionApp);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetFunctionApp")
                .WithSummary("Get a Function App")
                .WithDescription("Returns the full details of a single Azure Function App resource.")
                .Produces<FunctionAppResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreateFunctionAppRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateFunctionAppCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            functionApp =>
                            {
                                var response = mapper.Map<FunctionAppResponse>(functionApp);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "GetFunctionApp",
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateFunctionApp")
                .WithSummary("Create a Function App")
                .WithDescription("Creates a new Azure Function App resource inside the specified Resource Group. The App Service Plan must already exist. Requires Owner or Contributor access.")
                .Produces<FunctionAppResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateFunctionAppRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateFunctionAppCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            functionApp =>
                            {
                                var response = mapper.Map<FunctionAppResponse>(functionApp);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateFunctionApp")
                .WithSummary("Update a Function App")
                .WithDescription("Replaces all mutable properties of an existing Function App. Requires Owner or Contributor access.")
                .Produces<FunctionAppResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteFunctionAppCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteFunctionApp")
                .WithSummary("Delete a Function App")
                .WithDescription("Permanently deletes an Azure Function App resource. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
