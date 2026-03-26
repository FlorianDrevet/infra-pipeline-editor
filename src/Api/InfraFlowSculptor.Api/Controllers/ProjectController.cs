using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListUsers;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectEnvironment;
using InfraFlowSculptor.Application.Projects.Commands.DeleteProject;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectMember;
using InfraFlowSculptor.Application.Projects.Commands.CreateProject;
using InfraFlowSculptor.Application.Projects.Commands.DownloadProjectBicep;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBicep;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectBicepToGit;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectEnvironment;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectGitConfig;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectMember;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceNamingTemplate;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectDefaultNamingTemplate;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectGitConfig;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceNamingTemplate;
using InfraFlowSculptor.Application.Projects.Commands.SetRepositoryMode;
using InfraFlowSculptor.Application.Projects.Commands.TestGitConnection;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectEnvironment;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectMemberRole;
using InfraFlowSculptor.Application.Projects.Queries.GetProject;
using InfraFlowSculptor.Application.Projects.Queries.GetProjectBicepFileContent;
using InfraFlowSculptor.Application.Projects.Queries.ListGitBranches;
using InfraFlowSculptor.Application.Projects.Queries.ListMyProjects;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectConfigs;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectResources;
using InfraFlowSculptor.Application.Projects.Queries.ValidateRecentItems;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Projects.Responses;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

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

            // ── Environments ──────────────────────────────────────────────

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
                            request.TenantId,
                            request.SubscriptionId,
                            request.Order,
                            request.RequiresApproval,
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
                .WithName("AddProjectEnvironment")
                .WithSummary("Add an environment to a project")
                .WithDescription("Adds a new project-level environment definition. Requires Owner or Contributor access.")
                .Produces<EnvironmentDefinitionResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
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
                            request.TenantId,
                            request.SubscriptionId,
                            request.Order,
                            request.RequiresApproval,
                            request.Tags.Select(t => (t.Name, t.Value)).ToList()
                        );
                        var result = await mediator.Send(command);

                        return result.Match(
                            env => Results.Ok(mapper.Map<EnvironmentDefinitionResponse>(env)),
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateProjectEnvironment")
                .WithSummary("Update a project environment")
                .WithDescription("Updates all fields of an existing project-level environment definition. Requires Owner or Contributor access.")
                .Produces<EnvironmentDefinitionResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
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
                .WithName("RemoveProjectEnvironment")
                .WithSummary("Remove a project environment")
                .WithDescription("Removes a project-level environment definition. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Naming Templates ──────────────────────────────────────────

            group.MapPut("/{id:guid}/naming/default",
                    async ([FromRoute] Guid id, SetProjectDefaultNamingTemplateRequest request, IMediator mediator) =>
                    {
                        var command = new SetProjectDefaultNamingTemplateCommand(
                            new ProjectId(id),
                            request.Template
                        );
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("SetProjectDefaultNamingTemplate")
                .WithSummary("Set the project default naming template")
                .WithDescription("Sets or clears the default naming template at the project level. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}/naming/resources/{resourceType}",
                    async ([FromRoute] Guid id, [FromRoute] string resourceType, SetProjectResourceNamingTemplateRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new SetProjectResourceNamingTemplateCommand(
                            new ProjectId(id),
                            resourceType,
                            request.Template
                        );
                        var result = await mediator.Send(command);

                        return result.Match(
                            tpl => Results.Ok(mapper.Map<ResourceNamingTemplateResponse>(tpl)),
                            errors => errors.Result()
                        );
                    })
                .WithName("SetProjectResourceNamingTemplate")
                .WithSummary("Set a per-resource-type naming template")
                .WithDescription("Creates or replaces a naming template for a specific Azure resource type at the project level. Requires Owner or Contributor access.")
                .Produces<ResourceNamingTemplateResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}/naming/resources/{resourceType}",
                    async ([FromRoute] Guid id, [FromRoute] string resourceType, IMediator mediator) =>
                    {
                        var command = new RemoveProjectResourceNamingTemplateCommand(
                            new ProjectId(id),
                            resourceType
                        );
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("RemoveProjectResourceNamingTemplate")
                .WithSummary("Remove a per-resource-type naming template")
                .WithDescription("Removes a per-resource-type naming template from the project. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Delete Project ────────────────────────────────────────────

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
                .WithName("DeleteProject")
                .WithSummary("Delete a project")
                .WithDescription("Permanently deletes a project and all its data. Requires Owner access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Recent items validation ──────────────────────────────────

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
                .WithName("ValidateRecentItems")
                .WithSummary("Validate recently viewed items")
                .WithDescription("Filters a list of recently viewed items, returning only those the current user still has access to with fresh data.")
                .Produces<IReadOnlyList<RecentItemResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            // ── Git Repository Configuration ─────────────────────────────

            group.MapPut("/{projectId:guid}/git-config",
                    async ([FromRoute] Guid projectId, [FromBody] SetGitConfigRequest request,
                        IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<SetProjectGitConfigCommand>(request) with
                        {
                            ProjectId = new ProjectId(projectId)
                        };
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("SetProjectGitConfig")
                .WithSummary("Set or update Git repository configuration")
                .WithDescription("Configures the Git repository where generated Bicep files can be pushed. Requires Owner access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{projectId:guid}/git-config",
                    async ([FromRoute] Guid projectId, IMediator mediator) =>
                    {
                        var command = new RemoveProjectGitConfigCommand(new ProjectId(projectId));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("RemoveProjectGitConfig")
                .WithSummary("Remove Git repository configuration")
                .WithDescription("Removes the Git repository configuration from the project. Requires Owner access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

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
                .WithName("TestGitConnection")
                .WithSummary("Test Git repository connection")
                .WithDescription("Tests the connection to the configured Git repository using the stored token. Requires Owner or Contributor access.")
                .Produces<TestGitConnectionResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
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
                .WithName("ListGitBranches")
                .WithSummary("List Git repository branches")
                .WithDescription("Lists all branches in the configured Git repository. Requires read access to the project.")
                .Produces<IReadOnlyList<GitBranchResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

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
                .WithName("ListProjectResources")
                .WithSummary("List all resources across configurations")
                .WithDescription("Returns all Azure resources across all infrastructure configurations in the project. Used for cross-config resource reference selection.")
                .Produces<IReadOnlyList<ProjectResourceResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Repository Mode ─────────────────────────────────────────

            group.MapPut("/{projectId:guid}/repository-mode",
                    async ([FromRoute] Guid projectId,
                        [FromBody] SetRepositoryModeRequest request,
                        IMediator mediator) =>
                    {
                        var command = new SetRepositoryModeCommand(
                            new ProjectId(projectId),
                            request.RepositoryMode);
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("SetRepositoryMode")
                .WithSummary("Set the repository mode (MonoRepo/MultiRepo)")
                .WithDescription("Configures how generated Bicep files are organized: MultiRepo (per-config push) or MonoRepo (project-level push with shared Common folder). Requires Owner access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Project-level Bicep Generation (mono-repo) ──────────────

            group.MapPost("/{projectId:guid}/generate-bicep",
                    async ([FromRoute] Guid projectId, IMediator mediator) =>
                    {
                        var command = new GenerateProjectBicepCommand(new ProjectId(projectId));
                        var result = await mediator.Send(command);

                        return result.Match(
                            value =>
                            {
                                var response = new GenerateProjectBicepResponse(
                                    value.CommonFileUris,
                                    value.ConfigFileUris);
                                return Results.Created($"/projects/{projectId}/generate-bicep", response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GenerateProjectBicep")
                .WithSummary("Generate Bicep files for the entire project (mono-repo)")
                .WithDescription("Generates Bicep files for all configurations in the project, organized as a mono-repo with a shared Common folder and per-config deployment folders.")
                .Produces<GenerateProjectBicepResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{projectId:guid}/generate-bicep/download",
                    async ([FromRoute] Guid projectId, IMediator mediator) =>
                    {
                        var command = new DownloadProjectBicepCommand(new ProjectId(projectId));
                        var result = await mediator.Send(command);

                        return result.Match(
                            value => Results.File(
                                value.ZipContent,
                                "application/zip",
                                value.FileName),
                            errors => errors.Result()
                        );
                    })
                .WithName("DownloadProjectBicep")
                .WithSummary("Download generated Bicep files for a project")
                .WithDescription("Downloads the latest generated mono-repo Bicep files for the given project as a ZIP archive.")
                .Produces(StatusCodes.Status200OK, contentType: "application/zip")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{projectId:guid}/generate-bicep/files/{*filePath}",
                    async ([FromRoute] Guid projectId, [FromRoute] string filePath, IMediator mediator) =>
                    {
                        var query = new GetProjectBicepFileContentQuery(projectId, filePath);
                        var result = await mediator.Send(query);

                        return result.Match(
                            value => Results.Ok(new { content = value.Content }),
                            errors => errors.Result()
                        );
                    })
                .WithName("GetProjectBicepFileContent")
                .WithSummary("Get generated Bicep file content for a project")
                .WithDescription("Reads the latest generated mono-repo Bicep file content for the given project and relative file path.")
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Project-level Push to Git (mono-repo) ───────────────────

            group.MapPost("/{projectId:guid}/push-to-git",
                    async ([FromRoute] Guid projectId,
                        [FromBody] PushBicepToGitRequest request,
                        IMediator mediator,
                        IMapper mapper) =>
                    {
                        var command = new PushProjectBicepToGitCommand(
                            new ProjectId(projectId),
                            request.BranchName,
                            request.CommitMessage);
                        var result = await mediator.Send(command);

                        return result.Match(
                            value => Results.Ok(mapper.Map<PushBicepToGitResponse>(value)),
                            errors => errors.Result()
                        );
                    })
                .WithName("PushProjectBicepToGit")
                .WithSummary("Push project-level Bicep files to Git (mono-repo)")
                .WithDescription("Pushes the latest project-level generated Bicep files to the configured Git repository. Used in MonoRepo mode.")
                .Produces<PushBicepToGitResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
