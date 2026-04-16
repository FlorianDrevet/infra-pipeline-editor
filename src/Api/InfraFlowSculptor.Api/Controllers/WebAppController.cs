using InfraFlowSculptor.Application.WebApps.Commands.CreateWebApp;
using InfraFlowSculptor.Application.WebApps.Commands.DeleteWebApp;
using InfraFlowSculptor.Application.WebApps.Commands.UpdateWebApp;
using InfraFlowSculptor.Application.WebApps.Queries;
using MediatR;
using InfraFlowSculptor.Contracts.WebApps.Requests;
using InfraFlowSculptor.Contracts.WebApps.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoint definitions for the Web App feature.</summary>
public static class WebAppController
{
    /// <summary>Registers the Web App endpoints on the application builder.</summary>
    public static IApplicationBuilder UseWebAppController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/web-app")
                .WithTags("Web Apps");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetWebAppQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            webApp =>
                            {
                                var response = mapper.Map<WebAppResponse>(webApp);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetWebApp")
                .WithSummary("Get a Web App")
                .WithDescription("Returns the full details of a single Azure Web App resource.")
                .Produces<WebAppResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreateWebAppRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateWebAppCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            webApp =>
                            {
                                var response = mapper.Map<WebAppResponse>(webApp);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "GetWebApp",
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateWebApp")
                .WithSummary("Create a Web App")
                .WithDescription("Creates a new Azure Web App resource inside the specified Resource Group. The App Service Plan must already exist. Requires Owner or Contributor access.")
                .Produces<WebAppResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateWebAppRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateWebAppCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            webApp =>
                            {
                                var response = mapper.Map<WebAppResponse>(webApp);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateWebApp")
                .WithSummary("Update a Web App")
                .WithDescription("Replaces all mutable properties of an existing Web App. Requires Owner or Contributor access.")
                .Produces<WebAppResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteWebAppCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteWebApp")
                .WithSummary("Delete a Web App")
                .WithDescription("Permanently deletes an Azure Web App resource. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
