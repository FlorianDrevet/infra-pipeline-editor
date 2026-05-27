using InfraFlowSculptor.Api.Controllers.Constants;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListUsers;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectMember;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectMember;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectMemberRole;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Projects.Responses;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoint definitions for Project membership operations.</summary>
public static class ProjectMemberController
{
    /// <summary>Registers the Project membership endpoints on the application builder.</summary>
    public static IApplicationBuilder UseProjectMemberController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup(Routes.Projects)
                .WithTags("Projects");

            MapUserAndMembershipEndpoints(group);
        });
    }

    private static void MapUserAndMembershipEndpoints(RouteGroupBuilder group)
    {
        // ── Users ──────────────────────────────────────────────

        group.MapGet("/users",
                async (IMediator mediator, IMapper mapper) =>
                {
                    var query = new ListUsersQuery();
                    var result = await mediator.Send(query);

                    return result.Match(
                        users =>
                        {
                            var responses = users.Select(u => mapper.Map<UserResponse>(u)).ToList();
                            return TypedResults.Ok(responses);
                        },
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.ListProjectUsers)
            .WithSummary("List registered users")
            .WithDescription("Returns all registered users available for project membership assignment.")
            .Produces<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        // ── Members ───────────────────────────────────────────

        group.MapPost("/{id:guid}/members",
                async ([FromRoute] Guid id, AddProjectMemberRequest request, IMediator mediator, IMapper mapper) =>
                {
                    var command = new AddProjectMemberCommand(
                        new ProjectId(id),
                        request.UserId,
                        request.Role);
                    var result = await mediator.Send(command);

                    return result.Match(
                        project =>
                        {
                            var response = mapper.Map<ProjectResponse>(project);
                            return Results.Ok(response);
                        },
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.AddProjectMember)
            .WithSummary("Add a member to a project")
            .WithDescription("Adds a user to a Project with the specified role. Requires Owner access.")
            .Produces<ProjectResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}/members/{userId:guid}",
                async ([FromRoute] Guid id, [FromRoute] Guid userId, UpdateProjectMemberRoleRequest request, IMediator mediator, IMapper mapper) =>
                {
                    var command = new UpdateProjectMemberRoleCommand(
                        new ProjectId(id),
                        userId,
                        request.NewRole);
                    var result = await mediator.Send(command);

                    return result.Match(
                        project =>
                        {
                            var response = mapper.Map<ProjectResponse>(project);
                            return Results.Ok(response);
                        },
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.UpdateProjectMemberRole)
            .WithSummary("Update a project member's role")
            .WithDescription("Changes the role assigned to a member of a Project. Requires Owner access.")
            .Produces<ProjectResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}/members/{userId:guid}",
                async ([FromRoute] Guid id, [FromRoute] Guid userId, IMediator mediator) =>
                {
                    var command = new RemoveProjectMemberCommand(
                        new ProjectId(id),
                        userId);
                    var result = await mediator.Send(command);

                    return result.Match(
                        _ => Results.NoContent(),
                        errors => errors.Result()
                    );
                })
            .WithName(ProjectRouteNames.RemoveProjectMember)
            .WithSummary("Remove a member from a project")
            .WithDescription("Removes a user from a Project. Requires Owner access.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
