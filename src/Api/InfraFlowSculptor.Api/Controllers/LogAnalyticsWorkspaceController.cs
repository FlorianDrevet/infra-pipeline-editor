using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.CreateLogAnalyticsWorkspace;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.DeleteLogAnalyticsWorkspace;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.UpdateLogAnalyticsWorkspace;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Queries.GetLogAnalyticsWorkspace;
using InfraFlowSculptor.Application.Common.Queries.GetDependentResources;
using InfraFlowSculptor.Contracts.Common;
using InfraFlowSculptor.Contracts.LogAnalyticsWorkspaces.Requests;
using InfraFlowSculptor.Contracts.LogAnalyticsWorkspaces.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoints for the Log Analytics Workspace resource.</summary>
public static class LogAnalyticsWorkspaceController
{
    /// <summary>Registers the Log Analytics Workspace endpoints under <c>/log-analytics-workspace</c>.</summary>
    public static IApplicationBuilder UseLogAnalyticsWorkspaceController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/log-analytics-workspace")
                .WithTags("Log Analytics Workspaces");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetLogAnalyticsWorkspaceQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            law =>
                            {
                                var response = mapper.Map<LogAnalyticsWorkspaceResponse>(law);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetLogAnalyticsWorkspace")
                .WithSummary("Get a Log Analytics Workspace")
                .WithDescription("Returns the full details of a single Azure Log Analytics Workspace resource.")
                .Produces<LogAnalyticsWorkspaceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreateLogAnalyticsWorkspaceRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateLogAnalyticsWorkspaceCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            law =>
                            {
                                var response = mapper.Map<LogAnalyticsWorkspaceResponse>(law);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "GetLogAnalyticsWorkspace",
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateLogAnalyticsWorkspace")
                .WithSummary("Create a Log Analytics Workspace")
                .WithDescription("Creates a new Azure Log Analytics Workspace resource inside the specified Resource Group.")
                .Produces<LogAnalyticsWorkspaceResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateLogAnalyticsWorkspaceRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateLogAnalyticsWorkspaceCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            law =>
                            {
                                var response = mapper.Map<LogAnalyticsWorkspaceResponse>(law);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateLogAnalyticsWorkspace")
                .WithSummary("Update a Log Analytics Workspace")
                .WithDescription("Replaces all mutable properties of an existing Log Analytics Workspace.")
                .Produces<LogAnalyticsWorkspaceResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteLogAnalyticsWorkspaceCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteLogAnalyticsWorkspace")
                .WithSummary("Delete a Log Analytics Workspace")
                .WithDescription("Permanently deletes an Azure Log Analytics Workspace resource.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
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
                .WithName("GetLogAnalyticsWorkspaceDependents")
                .WithSummary("Get dependent resources")
                .WithDescription("Returns all resources that depend on this Log Analytics Workspace and would be deleted alongside it.")
                .Produces<List<DependentResourceResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
