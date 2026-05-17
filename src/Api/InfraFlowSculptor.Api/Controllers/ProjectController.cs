using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInfraConfigLayoutMode;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddInfraConfigRepository;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateInfraConfigRepository;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveInfraConfigRepository;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Application.Projects.Commands.DeleteProject;
using InfraFlowSculptor.Application.Projects.Commands.CreateProject;
using InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;
using InfraFlowSculptor.Application.Projects.Commands.SetAgentPool;
using InfraFlowSculptor.Application.Projects.Commands.TestGitConnection;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectPipelineVariableGroup;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectPipelineVariableGroup;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectTags;
using InfraFlowSculptor.Application.Projects.Queries.GetProject;
using InfraFlowSculptor.Application.Projects.Queries.ListGitBranches;
using InfraFlowSculptor.Application.Projects.Queries.ListCodeRepoBranches;
using InfraFlowSculptor.Application.Projects.Queries.SearchCodeRepoFiles;
using InfraFlowSculptor.Application.Projects.Queries.ListMyProjects;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectConfigs;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectResources;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;
using InfraFlowSculptor.Application.Projects.Queries.ValidateRecentItems;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Projects.Responses;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

using InfraFlowSculptor.Api.Controllers.Constants;

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

            MapCoreCrudEndpoints(group);
            MapConfigurationListEndpoint(group);
            MapTagsDeleteAndRecentEndpoints(group);
            MapGitOperationEndpoints(group);
            MapResourceAndAgentPoolEndpoints(group);
            MapInfraConfigRepositoryEndpoints(group);
            MapPipelineVariableGroupEndpoints(group);
        });
    }

    private static void MapCoreCrudEndpoints(RouteGroupBuilder group)
    {
            // ── Core CRUD ────────────────────────────────────────

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
                .WithName(ProjectRouteNames.ListMyProjects)
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
                .WithName(ProjectRouteNames.GetProject)
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
                .WithName(ProjectRouteNames.CreateProject)
                .WithSummary("Create a Project")
                .WithDescription("Creates a new Project. The current user is automatically added as Owner.")
                .Produces<ProjectResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            group.MapPost("/with-setup",
                    async (CreateProjectWithSetupRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateProjectWithSetupCommand>(request);
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
                .WithName(ProjectRouteNames.CreateProjectWithSetup)
                .WithSummary("Create a Project with setup")
                .WithDescription("Creates a new Project and applies the wizard layout, environments, and repository slots in a single operation.")
                .Produces<ProjectResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static void MapConfigurationListEndpoint(RouteGroupBuilder group)
    {
            // ── Configurations ────────────────────────────────────

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
                .WithName(ProjectRouteNames.ListProjectConfigs)
                .WithSummary("List configurations for a project")
                .WithDescription("Returns all Infrastructure Configurations belonging to the specified Project.")
                .Produces<IReadOnlyList<InfrastructureConfigResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static void MapTagsDeleteAndRecentEndpoints(RouteGroupBuilder group)
    {
            // ── Tags ──────────────────────────────────────────────

            group.MapPut("/{id:guid}/tags",
                    async ([FromRoute] Guid id, [FromBody] SetProjectTagsRequest request, ISender sender) =>
                    {
                        var command = new SetProjectTagsCommand(
                            id,
                            request.Tags.Select(t => (t.Name, t.Value)).ToList());
                        var result = await sender.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.SetProjectTags)
                .WithSummary("Set project-level tags")
                .WithDescription("Replaces all project-level default tags with the provided set. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Delete Project ────────────────────────────────────

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteProjectCommand(new ProjectId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.DeleteProject)
                .WithSummary("Delete a project")
                .WithDescription("Permanently deletes a project and all its data. Requires Owner access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Recent items validation ──────────────────────────

            group.MapPost("/validate-recent",
                    async ([FromBody] ValidateRecentItemsRequest request, IMediator mediator) =>
                    {
                        var items = request.Items
                            .Where(i => Guid.TryParse(i.Id, out _))
                            .Select(i => new RecentItemReference(Guid.Parse(i.Id), i.Type))
                            .ToList();

                        var query = new ValidateRecentItemsQuery(items);
                        var result = await mediator.Send(query);

                        return result.Match(
                            validated => TypedResults.Ok(
                                validated.Select(r => new RecentItemResponse(r.Id, r.Name, r.Type, r.Description)).ToList()),
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.ValidateRecentItems)
                .WithSummary("Validate recently viewed items")
                .WithDescription("Filters a list of recently viewed items, returning only those the current user still has access to with fresh data.")
                .Produces<IReadOnlyList<RecentItemResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static void MapGitOperationEndpoints(RouteGroupBuilder group)
    {
            // ── Git Repository Operations (test + list branches) ─────────────────────────────

            group.MapPost("/{projectId:guid}/git-config/test",
                    async ([FromRoute] Guid projectId, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new TestGitConnectionCommand(new ProjectId(projectId));
                        var result = await mediator.Send(command);

                        return result.Match(
                            value => Results.Ok(mapper.Map<TestGitConnectionResponse>(value)),
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.TestGitConnection)
                .WithSummary("Test Git repository connection")
                .WithDescription("Tests the connection to the configured Git repository using the stored token. Requires Owner or Contributor access.")
                .Produces<TestGitConnectionResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{projectId:guid}/git-config/branches",
                    async ([FromRoute] Guid projectId, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new ListGitBranchesQuery(new ProjectId(projectId));
                        var result = await mediator.Send(query);

                        return result.Match(
                            branches =>
                            {
                                var responses = branches.Select(b => mapper.Map<GitBranchResponse>(b)).ToList();
                                return TypedResults.Ok(responses);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.ListGitBranches)
                .WithSummary("List Git repository branches")
                .WithDescription("Lists all branches in the configured Git repository. Requires read access to the project.")
                .Produces<IReadOnlyList<GitBranchResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Git Code Repository Operations (branches + file search) ───────────────

            group.MapGet("/{projectId:guid}/git-config/code-branches",
                    async ([FromRoute] Guid projectId, [FromQuery] Guid? configId, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new ListCodeRepoBranchesQuery(
                            new ProjectId(projectId),
                            configId.HasValue ? new InfrastructureConfigId(configId.Value) : null);
                        var result = await mediator.Send(query);

                        return result.Match(
                            branches =>
                            {
                                var responses = branches.Select(b => mapper.Map<GitBranchResponse>(b)).ToList();
                                return TypedResults.Ok(responses);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.ListCodeRepoBranches)
                .WithSummary("List code repository branches")
                .WithDescription("Lists all branches in the application-code Git repository. Requires read access to the project.")
                .Produces<IReadOnlyList<GitBranchResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{projectId:guid}/git-config/code-files",
                    async ([FromRoute] Guid projectId, [FromQuery] string branch, [FromQuery] string? pattern, [FromQuery] Guid? configId, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new SearchCodeRepoFilesQuery(
                            new ProjectId(projectId),
                            branch,
                            pattern,
                            configId.HasValue ? new InfrastructureConfigId(configId.Value) : null);
                        var result = await mediator.Send(query);

                        return result.Match(
                            files =>
                            {
                                var responses = files.Select(f => mapper.Map<GitFileResponse>(f)).ToList();
                                return TypedResults.Ok(responses);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.SearchCodeRepoFiles)
                .WithSummary("Search files in code repository")
                .WithDescription("Searches for files matching a filename pattern in the application-code Git repository on a specific branch. Requires read access to the project.")
                .Produces<IReadOnlyList<GitFileResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static void MapResourceAndAgentPoolEndpoints(RouteGroupBuilder group)
    {
            // GET /{id:guid}/resources
            group.MapGet("/{id:guid}/resources",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new ListProjectResourcesQuery(id);
                        var result = await mediator.Send(query);

                        return result.Match(
                            resources =>
                            {
                                var responses = resources.Select(r => mapper.Map<ProjectResourceResponse>(r)).ToList();
                                return TypedResults.Ok(responses);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.ListProjectResources)
                .WithSummary("List all resources across configurations")
                .WithDescription("Returns all Azure resources across all infrastructure configurations in the project. Used for cross-config resource reference selection.")
                .Produces<IReadOnlyList<ProjectResourceResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Agent Pool ──────────────────────────────────────

            group.MapPut("/{projectId:guid}/agent-pool",
                    async ([FromRoute] Guid projectId,
                        [FromBody] SetAgentPoolRequest request,
                        IMediator mediator) =>
                    {
                        var command = new SetAgentPoolCommand(
                            new ProjectId(projectId),
                            request.AgentPoolName);
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.SetProjectAgentPool)
                .WithSummary("Set or clear the agent pool for pipeline generation")
                .WithDescription("Sets the self-hosted agent pool name used in generated pipelines. Send null or empty to revert to the Microsoft-hosted pool (vmImage: ubuntu-latest). Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static void MapInfraConfigRepositoryEndpoints(RouteGroupBuilder group)
    {

            // ── InfraConfig Repositories (MultiRepo project layout only) ──────────────

            group.MapPut("/{projectId:guid}/configs/{configId:guid}/layout-mode",
                    async ([FromRoute] Guid projectId, [FromRoute] Guid configId,
                        [FromBody] SetInfraConfigLayoutModeRequest request, IMediator mediator) =>
                    {
                        var command = new SetInfraConfigLayoutModeCommand(
                            new ProjectId(projectId),
                            new InfrastructureConfigId(configId),
                            request.Mode);
                        var result = await mediator.Send(command);
                        return result.Match(_ => Results.NoContent(), errors => errors.Result());
                    })
                .WithName(ProjectRouteNames.SetInfraConfigLayoutMode)
                .WithSummary("Set or clear the per-configuration layout mode")
                .WithDescription("Sets the layout mode (AllInOne or SplitInfraCode) for the configuration. Only meaningful when the parent project layout is MultiRepo. Switching mode clears existing config-level repositories. Requires Owner access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/{projectId:guid}/configs/{configId:guid}/repositories",
                    async ([FromRoute] Guid projectId, [FromRoute] Guid configId,
                        [FromBody] AddInfraConfigRepositoryRequest request, IMediator mediator) =>
                    {
                        var command = new AddInfraConfigRepositoryCommand(
                            new ProjectId(projectId),
                            new InfrastructureConfigId(configId),
                            request.Alias,
                            request.ProviderType,
                            request.RepositoryUrl,
                            request.DefaultBranch,
                            request.ContentKinds);
                        var result = await mediator.Send(command);
                        return result.Match(
                            id => Results.Created($"/projects/{projectId}/configs/{configId}/repositories/{id.Value}", new { id = id.Value.ToString() }),
                            errors => errors.Result());
                    })
                .WithName(ProjectRouteNames.AddInfraConfigRepository)
                .WithSummary("Declare a Git repository on an InfrastructureConfig (MultiRepo only)")
                .WithDescription("Adds a Git repository to the configuration. Allowed only when the parent project layout is MultiRepo and the configuration has a layout mode set. Requires Owner access.")
                .Produces(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{projectId:guid}/configs/{configId:guid}/repositories/{repositoryId:guid}",
                    async ([FromRoute] Guid projectId, [FromRoute] Guid configId, [FromRoute] Guid repositoryId,
                        [FromBody] UpdateInfraConfigRepositoryRequest request, IMediator mediator) =>
                    {
                        var command = new UpdateInfraConfigRepositoryCommand(
                            new ProjectId(projectId),
                            new InfrastructureConfigId(configId),
                            new InfraConfigRepositoryId(repositoryId),
                            request.ProviderType,
                            request.RepositoryUrl,
                            request.DefaultBranch,
                            request.ContentKinds);
                        var result = await mediator.Send(command);
                        return result.Match(_ => Results.NoContent(), errors => errors.Result());
                    })
                .WithName(ProjectRouteNames.UpdateInfraConfigRepository)
                .WithSummary("Update an InfraConfig repository")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{projectId:guid}/configs/{configId:guid}/repositories/{repositoryId:guid}",
                    async ([FromRoute] Guid projectId, [FromRoute] Guid configId, [FromRoute] Guid repositoryId, IMediator mediator) =>
                    {
                        var command = new RemoveInfraConfigRepositoryCommand(
                            new ProjectId(projectId),
                            new InfrastructureConfigId(configId),
                            new InfraConfigRepositoryId(repositoryId));
                        var result = await mediator.Send(command);
                        return result.Match(_ => Results.NoContent(), errors => errors.Result());
                    })
                .WithName(ProjectRouteNames.RemoveInfraConfigRepository)
                .WithSummary("Delete an InfraConfig repository")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static void MapPipelineVariableGroupEndpoints(RouteGroupBuilder group)
    {

            // ── Pipeline Variable Groups (project-level) ────────

            // GET /{projectId:guid}/pipeline-variable-groups
            group.MapGet("/{projectId:guid}/pipeline-variable-groups",
                    async ([FromRoute] Guid projectId, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new ListProjectPipelineVariableGroupsQuery(new ProjectId(projectId));
                        var result = await mediator.Send(query);

                        return result.Match(
                            groups =>
                            {
                                var responses = groups.Select(g => mapper.Map<ProjectPipelineVariableGroupResponse>(g)).ToList();
                                return TypedResults.Ok(responses);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.ListProjectPipelineVariableGroups)
                .WithSummary("List project-level pipeline variable groups")
                .WithDescription("Returns all Azure DevOps Variable Groups (Libraries) configured at project level, shared across all configurations.")
                .Produces<IReadOnlyList<ProjectPipelineVariableGroupResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // POST /{projectId:guid}/pipeline-variable-groups
            group.MapPost("/{projectId:guid}/pipeline-variable-groups",
                    async ([FromRoute] Guid projectId, AddProjectPipelineVariableGroupRequest request, IMediator mediator) =>
                    {
                        var command = new AddProjectPipelineVariableGroupCommand(new ProjectId(projectId), request.GroupName);
                        var result = await mediator.Send(command);

                        return result.Match(
                            g => Results.Created(
                                $"/projects/{projectId}/pipeline-variable-groups/{g.GroupId}",
                                new ProjectPipelineVariableGroupResponse(
                                    g.GroupId.ToString(),
                                    g.GroupName,
                                    [])),
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.AddProjectPipelineVariableGroup)
                .WithSummary("Add a project-level pipeline variable group")
                .WithDescription("Adds an Azure DevOps Variable Group (Library) reference to the project, shared across all configurations for pipeline generation.")
                .Produces<ProjectPipelineVariableGroupResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // DELETE /{projectId:guid}/pipeline-variable-groups/{groupId:guid}
            group.MapDelete("/{projectId:guid}/pipeline-variable-groups/{groupId:guid}",
                    async ([FromRoute] Guid projectId, [FromRoute] Guid groupId, IMediator mediator) =>
                    {
                        var command = new RemoveProjectPipelineVariableGroupCommand(new ProjectId(projectId), groupId);
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.RemoveProjectPipelineVariableGroup)
                .WithSummary("Remove a project-level pipeline variable group")
                .WithDescription("Removes a variable group from the project.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
