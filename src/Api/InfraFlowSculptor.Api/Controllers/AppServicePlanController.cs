using InfraFlowSculptor.Application.AppServicePlans.Commands.CreateAppServicePlan;
using InfraFlowSculptor.Application.AppServicePlans.Commands.DeleteAppServicePlan;
using InfraFlowSculptor.Application.AppServicePlans.Commands.UpdateAppServicePlan;
using InfraFlowSculptor.Application.AppServicePlans.Queries;
using InfraFlowSculptor.Application.Common.Queries.GetDependentResources;
using MediatR;
using InfraFlowSculptor.Contracts.AppServicePlans.Requests;
using InfraFlowSculptor.Contracts.AppServicePlans.Responses;
using InfraFlowSculptor.Contracts.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoint definitions for the App Service Plan feature.</summary>
public static class AppServicePlanController
{
    /// <summary>Registers the App Service Plan endpoints on the application builder.</summary>
    public static IEndpointRouteBuilder MapAppServicePlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/app-service-plan")
            .WithTags("App Service Plans");

        group.MapGet("/{id:guid}",
                async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                {
                    var query = new GetAppServicePlanQuery(new AzureResourceId(id));
                    var result = await mediator.Send(query);

                    return result.Match(
                        plan =>
                        {
                            var response = mapper.Map<AppServicePlanResponse>(plan);
                            return TypedResults.Ok(response);
                        },
                        errors => errors.Result()
                    );
                })
            .WithName("GetAppServicePlan")
            .WithSummary("Get an App Service Plan")
            .WithDescription("Returns the full details of a single Azure App Service Plan resource.")
            .Produces<AppServicePlanResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("",
                async (CreateAppServicePlanRequest request, IMediator mediator, IMapper mapper) =>
                {
                    var command = mapper.Map<CreateAppServicePlanCommand>(request);
                    var result = await mediator.Send(command);

                    return result.Match(
                        plan =>
                        {
                            var response = mapper.Map<AppServicePlanResponse>(plan);
                            return TypedResults.CreatedAtRoute(
                                routeName: "GetAppServicePlan",
                                routeValues: new { id = response.Id },
                                value: response
                            );
                        },
                        errors => errors.Result()
                    );
                })
            .WithName("CreateAppServicePlan")
            .WithSummary("Create an App Service Plan")
            .WithDescription("Creates a new Azure App Service Plan resource. Requires Owner or Contributor access.")
            .Produces<AppServicePlanResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}",
                async ([FromRoute] Guid id, UpdateAppServicePlanRequest request, IMediator mediator, IMapper mapper) =>
                {
                    var command = mapper.Map<UpdateAppServicePlanCommand>((id, request));
                    var result = await mediator.Send(command);

                    return result.Match(
                        plan =>
                        {
                            var response = mapper.Map<AppServicePlanResponse>(plan);
                            return TypedResults.Ok(response);
                        },
                        errors => errors.Result()
                    );
                })
            .WithName("UpdateAppServicePlan")
            .WithSummary("Update an App Service Plan")
            .WithDescription("Replaces all mutable properties of an existing App Service Plan. Requires Owner or Contributor access.")
            .Produces<AppServicePlanResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}",
                async ([FromRoute] Guid id, IMediator mediator) =>
                {
                    var command = new DeleteAppServicePlanCommand(new AzureResourceId(id));
                    var result = await mediator.Send(command);

                    return result.Match(
                        _ => Results.NoContent(),
                        errors => errors.Result()
                    );
                })
            .WithName("DeleteAppServicePlan")
            .WithSummary("Delete an App Service Plan")
            .WithDescription("Permanently deletes an Azure App Service Plan resource. Requires Owner or Contributor access.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}/dependents",
                async ([FromRoute] Guid id, IMediator mediator) =>
                {
                    var query = new GetDependentResourcesQuery(new AzureResourceId(id));
                    var result = await mediator.Send(query);

                    return result.Match(
                        dependents => Results.Ok(dependents.Select(d =>
                            new DependentResourceResponse(d.Id.ToString(), d.Name, d.ResourceType)).ToList()),
                        errors => errors.Result()
                    );
                })
            .WithName("GetAppServicePlanDependents")
            .WithSummary("Get dependent resources")
            .WithDescription("Returns all resources that depend on this App Service Plan and would be deleted alongside it.")
            .Produces<List<DependentResourceResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
        return app;
    }
}
