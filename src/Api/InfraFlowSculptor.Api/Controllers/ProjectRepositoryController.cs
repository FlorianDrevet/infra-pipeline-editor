using InfraFlowSculptor.Api.Controllers.Constants;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectRepository;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectRepository;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectLayoutPreset;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectRepository;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoint definitions for Project repository and layout operations.</summary>
public static class ProjectRepositoryController
{
    /// <summary>Registers the Project repository endpoints on the application builder.</summary>
    public static IApplicationBuilder UseProjectRepositoryController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup(Routes.Projects)
                .WithTags("Projects");

            MapRepositoryAndLayoutEndpoints(group);
        });
    }

    private static void MapRepositoryAndLayoutEndpoints(RouteGroupBuilder group)
    {
        // ── Repositories (V1 multi-repo topology) ───────────────

        group.MapPost("/{projectId:guid}/repositories",
                async ([FromRoute] Guid projectId,
                    [FromBody] AddProjectRepositoryRequest request,
                    IMediator mediator) =>
                {
                    var command = new AddProjectRepositoryCommand(
                        new ProjectId(projectId),
                        request.Alias,
                        request.ProviderType,
                        request.RepositoryUrl,
                        request.DefaultBranch,
                        request.ContentKinds);
                    var result = await mediator.Send(command);

                    return result.Match(
                        repoId => Results.Created(
                            $"/projects/{projectId}/repositories/{repoId.Value}",
                            new { id = repoId.Value.ToString() }),
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.AddProjectRepository)
            .WithSummary("Add a project-level Git repository declaration")
            .WithDescription("Declares a new Git repository at the project level. Each repository has a project-scoped alias and one or more content kinds (Infrastructure, ApplicationCode, Pipelines). Requires Owner access.")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{projectId:guid}/repositories/{repoId:guid}",
                async ([FromRoute] Guid projectId,
                    [FromRoute] Guid repoId,
                    [FromBody] UpdateProjectRepositoryRequest request,
                    IMediator mediator) =>
                {
                    var command = new UpdateProjectRepositoryCommand(
                        new ProjectId(projectId),
                        new ProjectRepositoryId(repoId),
                        request.ProviderType,
                        request.RepositoryUrl,
                        request.DefaultBranch,
                        request.ContentKinds);
                    var result = await mediator.Send(command);

                    return result.Match(
                        _ => Results.NoContent(),
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.UpdateProjectRepository)
            .WithSummary("Update a project-level Git repository declaration")
            .WithDescription("Updates an existing project repository (alias is immutable). Requires Owner access.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{projectId:guid}/repositories/{repoId:guid}",
                async ([FromRoute] Guid projectId,
                    [FromRoute] Guid repoId,
                    IMediator mediator) =>
                {
                    var command = new RemoveProjectRepositoryCommand(
                        new ProjectId(projectId),
                        new ProjectRepositoryId(repoId));
                    var result = await mediator.Send(command);

                    return result.Match(
                        _ => Results.NoContent(),
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.RemoveProjectRepository)
            .WithSummary("Remove a project-level Git repository declaration")
            .WithDescription("Removes a project repository. Returns 409 Conflict if the repository is still referenced by an infrastructure configuration binding. Requires Owner access.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{projectId:guid}/layout-preset",
                async ([FromRoute] Guid projectId,
                    [FromBody] SetProjectLayoutPresetRequest request,
                    IMediator mediator) =>
                {
                    var command = new SetProjectLayoutPresetCommand(
                        new ProjectId(projectId),
                        request.Preset);
                    var result = await mediator.Send(command);

                    return result.Match(
                        _ => Results.NoContent(),
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.SetProjectLayoutPreset)
            .WithSummary("Set the project layout preset")
            .WithDescription("Updates the project layout preset. Valid values: AllInOne, SplitInfraCode, MultiRepo. Switching to MultiRepo auto-clears project repositories. Requires Owner access.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
