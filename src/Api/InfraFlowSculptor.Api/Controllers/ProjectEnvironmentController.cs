using InfraFlowSculptor.Api.Controllers.Constants;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectEnvironment;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectEnvironment;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectEnvironment;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoint definitions for Project environment operations.</summary>
public static class ProjectEnvironmentController
{
    /// <summary>Registers the Project environment endpoints on the application builder.</summary>
    public static IApplicationBuilder UseProjectEnvironmentController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup(Routes.Projects)
                .WithTags("Projects");

            MapEnvironmentEndpoints(group);
        });
    }

    private static void MapEnvironmentEndpoints(RouteGroupBuilder group)
    {
        // ── Environments ──────────────────────────────────────

        group.MapPost("/{id:guid}/environments",
                async ([FromRoute] Guid id, AddProjectEnvironmentRequest request, IMediator mediator, IMapper mapper) =>
                {
                    var command = new AddProjectEnvironmentCommand(
                        new ProjectId(id),
                        request.Name,
                        request.ShortName,
                        request.Prefix,
                        request.Suffix,
                        request.Location,
                        request.SubscriptionId ?? Guid.Empty,
                        request.Order,
                        request.RequiresApproval,
                        request.AzureResourceManagerConnection,
                        request.Tags.Select(t => (t.Name, t.Value)).ToList()
                    );
                    var result = await mediator.Send(command);

                    return result.Match(
                        env =>
                        {
                            var response = mapper.Map<EnvironmentDefinitionResponse>(env);
                            return Results.Created($"/projects/{id}/environments/{response.Id}", response);
                        },
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.AddProjectEnvironment)
            .WithSummary("Add an environment to a project")
            .WithDescription("Adds a new project-level environment definition. Requires Owner or Contributor access.")
            .Produces<EnvironmentDefinitionResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}/environments/{envId:guid}",
                async ([FromRoute] Guid id, [FromRoute] Guid envId, UpdateProjectEnvironmentRequest request, IMediator mediator, IMapper mapper) =>
                {
                    var command = new UpdateProjectEnvironmentCommand(
                        new ProjectId(id),
                        new ProjectEnvironmentDefinitionId(envId),
                        request.Name,
                        request.ShortName,
                        request.Prefix,
                        request.Suffix,
                        request.Location,
                        request.SubscriptionId,
                        request.Order,
                        request.RequiresApproval,
                        request.AzureResourceManagerConnection,
                        request.Tags.Select(t => (t.Name, t.Value)).ToList()
                    );
                    var result = await mediator.Send(command);

                    return result.Match(
                        env => Results.Ok(mapper.Map<EnvironmentDefinitionResponse>(env)),
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.UpdateProjectEnvironment)
            .WithSummary("Update a project environment")
            .WithDescription("Updates all fields of an existing project-level environment definition. Requires Owner or Contributor access.")
            .Produces<EnvironmentDefinitionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}/environments/{envId:guid}",
                async ([FromRoute] Guid id, [FromRoute] Guid envId, IMediator mediator) =>
                {
                    var command = new RemoveProjectEnvironmentCommand(
                        new ProjectId(id),
                        new ProjectEnvironmentDefinitionId(envId)
                    );
                    var result = await mediator.Send(command);

                    return result.Match(
                        _ => Results.NoContent(),
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.RemoveProjectEnvironment)
            .WithSummary("Remove a project environment")
            .WithDescription("Removes a project-level environment definition. Requires Owner or Contributor access.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
