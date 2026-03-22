using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListUsers;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectMember;
using InfraFlowSculptor.Application.Projects.Commands.CreateProject;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectMember;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectMemberRole;
using InfraFlowSculptor.Application.Projects.Queries.GetProject;
using InfraFlowSculptor.Application.Projects.Queries.ListMyProjects;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectConfigs;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Projects.Responses;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoint definitions for the Project feature.</summary>
public static class ProjectController
{
    /// <summary>Registers the Project endpoints on the application builder.</summary>
    public static IApplicationBuilder UseProjectController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/projects")
                .WithTags("Projects");

            // ── Core CRUD ────────────────────────────────────────────────

            group.MapGet("",
                    async (IMediator mediator, IMapper mapper) =>
                    {
                        var query = new ListMyProjectsQuery();
                        var result = await mediator.Send(query);

                        return result.Match(
                            projects =>
                            {
                                var responses = projects.Select(p => mapper.Map<ProjectResponse>(p)).ToList();
                                return TypedResults.Ok(responses);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("ListMyProjects")
                .WithSummary("List my Projects")
                .WithDescription("Returns all Projects the current user is a member of.")
                .Produces<IReadOnlyList<ProjectResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetProjectQuery(new ProjectId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            project =>
                            {
                                var response = mapper.Map<ProjectResponse>(project);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetProject")
                .WithSummary("Get a Project")
                .WithDescription("Returns the full details of a single Project, including members.")
                .Produces<ProjectResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            group.MapPost("",
                    async (CreateProjectRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new CreateProjectCommand(request.Name, request.Description);
                        var result = await mediator.Send(command);

                        return result.Match(
                            project =>
                            {
                                var response = mapper.Map<ProjectResponse>(project);
                                return Results.Created($"/projects/{response.Id}", response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateProject")
                .WithSummary("Create a Project")
                .WithDescription("Creates a new Project. The current user is automatically added as Owner.")
                .Produces<ProjectResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            // ── Configurations ────────────────────────────────────────────

            group.MapGet("/{id:guid}/configs",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new ListProjectConfigsQuery(new ProjectId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            configs =>
                            {
                                var responses = configs.Select(c => mapper.Map<InfrastructureConfigResponse>(c)).ToList();
                                return TypedResults.Ok(responses);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("ListProjectConfigs")
                .WithSummary("List configurations for a project")
                .WithDescription("Returns all Infrastructure Configurations belonging to the specified Project.")
                .Produces<IReadOnlyList<InfrastructureConfigResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Users ──────────────────────────────────────────────────────

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
                .WithName("ListProjectUsers")
                .WithSummary("List registered users")
                .WithDescription("Returns all registered users available for project membership assignment.")
                .Produces<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            // ── Members ───────────────────────────────────────────────────

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
                .WithName("AddProjectMember")
                .WithSummary("Add a member to a project")
                .WithDescription("Adds a user to a Project with the specified role. Requires Owner access.")
                .Produces<ProjectResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
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
                .WithName("UpdateProjectMemberRole")
                .WithSummary("Update a project member's role")
                .WithDescription("Changes the role assigned to a member of a Project. Requires Owner access.")
                .Produces<ProjectResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
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
                .WithName("RemoveProjectMember")
                .WithSummary("Remove a member from a project")
                .WithDescription("Removes a user from a Project. Requires Owner access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
