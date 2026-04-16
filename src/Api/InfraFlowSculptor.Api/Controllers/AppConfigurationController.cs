using InfraFlowSculptor.Application.AppConfigurations.Commands.CreateAppConfiguration;
using InfraFlowSculptor.Application.AppConfigurations.Commands.DeleteAppConfiguration;
using InfraFlowSculptor.Application.AppConfigurations.Commands.UpdateAppConfiguration;
using InfraFlowSculptor.Application.AppConfigurations.Queries;
using InfraFlowSculptor.Contracts.AppConfigurations.Requests;
using InfraFlowSculptor.Contracts.AppConfigurations.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoints for the App Configuration resource.</summary>
public static class AppConfigurationController
{
    /// <summary>Registers the App Configuration endpoints under <c>/app-configuration</c>.</summary>
    public static IApplicationBuilder UseAppConfigurationController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var config = endpoints.MapGroup("/app-configuration")
                .WithTags("App Configurations");

            config.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetAppConfigurationQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            appConfiguration =>
                            {
                                var response = mapper.Map<AppConfigurationResponse>(appConfiguration);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetAppConfiguration")
                .WithSummary("Get an App Configuration")
                .WithDescription("Returns the full details of a single Azure App Configuration resource.")
                .Produces<AppConfigurationResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            config.MapPost("",
                    async (CreateAppConfigurationRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateAppConfigurationCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            appConfiguration =>
                            {
                                var response = mapper.Map<AppConfigurationResponse>(appConfiguration);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "GetAppConfiguration",
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateAppConfiguration")
                .WithSummary("Create an App Configuration")
                .WithDescription("Creates a new Azure App Configuration resource inside the specified Resource Group. Requires Owner or Contributor access.")
                .Produces<AppConfigurationResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            config.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateAppConfigurationRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateAppConfigurationCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            appConfiguration =>
                            {
                                var response = mapper.Map<AppConfigurationResponse>(appConfiguration);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateAppConfiguration")
                .WithSummary("Update an App Configuration")
                .WithDescription("Replaces all mutable properties of an existing App Configuration. Requires Owner or Contributor access.")
                .Produces<AppConfigurationResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            config.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteAppConfigurationCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteAppConfiguration")
                .WithSummary("Delete an App Configuration")
                .WithDescription("Permanently deletes an Azure App Configuration resource. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
