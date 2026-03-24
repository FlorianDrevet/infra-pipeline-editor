using InfraFlowSculptor.Application.ApplicationInsights.Commands.CreateApplicationInsights;
using InfraFlowSculptor.Application.ApplicationInsights.Commands.DeleteApplicationInsights;
using InfraFlowSculptor.Application.ApplicationInsights.Commands.UpdateApplicationInsights;
using InfraFlowSculptor.Application.ApplicationInsights.Queries.GetApplicationInsights;
using InfraFlowSculptor.Contracts.ApplicationInsights.Requests;
using InfraFlowSculptor.Contracts.ApplicationInsights.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoints for the Application Insights resource.</summary>
public static class ApplicationInsightsController
{
    /// <summary>Registers the Application Insights endpoints under <c>/application-insights</c>.</summary>
    public static IApplicationBuilder UseApplicationInsightsController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/application-insights")
                .WithTags("Application Insights");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetApplicationInsightsQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            ai =>
                            {
                                var response = mapper.Map<ApplicationInsightsResponse>(ai);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetApplicationInsights")
                .WithSummary("Get an Application Insights resource")
                .WithDescription("Returns the full details of a single Azure Application Insights resource.")
                .Produces<ApplicationInsightsResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreateApplicationInsightsRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateApplicationInsightsCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            ai =>
                            {
                                var response = mapper.Map<ApplicationInsightsResponse>(ai);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "GetApplicationInsights",
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateApplicationInsights")
                .WithSummary("Create an Application Insights resource")
                .WithDescription("Creates a new Azure Application Insights resource inside the specified Resource Group.")
                .Produces<ApplicationInsightsResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateApplicationInsightsRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateApplicationInsightsCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            ai =>
                            {
                                var response = mapper.Map<ApplicationInsightsResponse>(ai);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateApplicationInsights")
                .WithSummary("Update an Application Insights resource")
                .WithDescription("Replaces all mutable properties of an existing Application Insights resource.")
                .Produces<ApplicationInsightsResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteApplicationInsightsCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteApplicationInsights")
                .WithSummary("Delete an Application Insights resource")
                .WithDescription("Permanently deletes an Azure Application Insights resource.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
