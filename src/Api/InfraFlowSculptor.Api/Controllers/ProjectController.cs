using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInfraConfigLayoutMode;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddInfraConfigRepository;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateInfraConfigRepository;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveInfraConfigRepository;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListUsers;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectEnvironment;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectRepository;
using InfraFlowSculptor.Application.Projects.Commands.DeleteProject;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectMember;
using InfraFlowSculptor.Application.Projects.Commands.CreateProject;
using InfraFlowSculptor.Application.Projects.Commands.DownloadProjectBicep;
using InfraFlowSculptor.Application.Projects.Commands.DownloadProjectPipeline;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBicep;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectPipeline;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectBicepToGit;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectGeneratedArtifactsToGit;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectPipelineToGit;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectEnvironment;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectMember;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectRepository;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceAbbreviation;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceNamingTemplate;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectDefaultNamingTemplate;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectLayoutPreset;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceAbbreviation;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceNamingTemplate;
using InfraFlowSculptor.Application.Projects.Commands.SetAgentPool;
using InfraFlowSculptor.Application.Projects.Commands.TestGitConnection;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectEnvironment;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectMemberRole;
using InfraFlowSculptor.Application.Projects.Commands.UpdateProjectRepository;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectPipelineVariableGroup;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectPipelineVariableGroup;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectTags;
using InfraFlowSculptor.Application.Projects.Queries.GetProject;
using InfraFlowSculptor.Application.Projects.Commands.DownloadProjectBootstrapPipeline;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectBootstrapPipelineToGit;
using InfraFlowSculptor.Application.Projects.Queries.GetProjectBicepFileContent;
using InfraFlowSculptor.Application.Projects.Queries.GetProjectBootstrapPipelineFileContent;
using InfraFlowSculptor.Application.Projects.Queries.GetProjectPipelineFileContent;
using InfraFlowSculptor.Application.Projects.Queries.ListGitBranches;
using InfraFlowSculptor.Application.Projects.Queries.ListMyProjects;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectConfigs;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectResources;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectPipelineVariableGroups;
using InfraFlowSculptor.Application.Projects.Queries.ValidateRecentItems;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Projects.Responses;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
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
                .ProducesProblem(StatusCodes.Status401Unauthorized)
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
                .WithName("UpdateProjectMemberRole")
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
                .WithName("RemoveProjectMember")
                .WithSummary("Remove a member from a project")
                .WithDescription("Removes a user from a Project. Requires Owner access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
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
                            request.SubscriptionId,
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
                .WithName("AddProjectEnvironment")
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
                .WithName("UpdateProjectEnvironment")
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
                .WithName("RemoveProjectEnvironment")
                .WithSummary("Remove a project environment")
                .WithDescription("Removes a project-level environment definition. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
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
                .ProducesProblem(StatusCodes.Status401Unauthorized)
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
                .ProducesProblem(StatusCodes.Status401Unauthorized)
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
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Abbreviation Overrides ────────────────────────────────

            group.MapPut("/{id:guid}/naming/abbreviations/{resourceType}",
                    async ([FromRoute] Guid id, [FromRoute] string resourceType, SetResourceAbbreviationOverrideRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new SetProjectResourceAbbreviationCommand(
                            new ProjectId(id),
                            resourceType,
                            request.Abbreviation
                        );
                        var result = await mediator.Send(command);

                        return result.Match(
                            abbr => Results.Ok(mapper.Map<ResourceAbbreviationOverrideResponse>(abbr)),
                            errors => errors.Result()
                        );
                    })
                .WithName("SetProjectResourceAbbreviation")
                .WithSummary("Set a per-resource-type abbreviation override")
                .WithDescription("Creates or replaces the abbreviation for a specific Azure resource type at the project level. Must be lowercase alphanumeric, max 10 characters. Requires Owner or Contributor access.")
                .Produces<ResourceAbbreviationOverrideResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}/naming/abbreviations/{resourceType}",
                    async ([FromRoute] Guid id, [FromRoute] string resourceType, IMediator mediator) =>
                    {
                        var command = new RemoveProjectResourceAbbreviationCommand(
                            new ProjectId(id),
                            resourceType
                        );
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("RemoveProjectResourceAbbreviation")
                .WithSummary("Remove a per-resource-type abbreviation override")
                .WithDescription("Removes the abbreviation override for a specific Azure resource type from the project. The catalog default will be used instead. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Tags ──────────────────────────────────────────────────

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
                .WithName("SetProjectTags")
                .WithSummary("Set project-level tags")
                .WithDescription("Replaces all project-level default tags with the provided set. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
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
                .ProducesProblem(StatusCodes.Status401Unauthorized)
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
                .WithName("TestGitConnection")
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
                .WithName("ListGitBranches")
                .WithSummary("List Git repository branches")
                .WithDescription("Lists all branches in the configured Git repository. Requires read access to the project.")
                .Produces<IReadOnlyList<GitBranchResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
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
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Agent Pool ──────────────────────────────────────────────

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
                .WithName("SetProjectAgentPool")
                .WithSummary("Set or clear the agent pool for pipeline generation")
                .WithDescription("Sets the self-hosted agent pool name used in generated pipelines. Send null or empty to revert to the Microsoft-hosted pool (vmImage: ubuntu-latest). Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Repositories (V1 multi-repo topology) ───────────────────

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
                .WithName("AddProjectRepository")
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
                .WithName("UpdateProjectRepository")
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
                .WithName("RemoveProjectRepository")
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
                .WithName("SetProjectLayoutPreset")
                .WithSummary("Set the project layout preset")
                .WithDescription("Updates the project layout preset. Valid values: AllInOne, SplitInfraCode, MultiRepo. Switching to MultiRepo auto-clears project repositories. Requires Owner access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);


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
                .WithName("SetInfraConfigLayoutMode")
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
                .WithName("AddInfraConfigRepository")
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
                .WithName("UpdateInfraConfigRepository")
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
                .WithName("RemoveInfraConfigRepository")
                .WithSummary("Delete an InfraConfig repository")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
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
                .ProducesProblem(StatusCodes.Status401Unauthorized)
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
                .ProducesProblem(StatusCodes.Status401Unauthorized)
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
                .ProducesProblem(StatusCodes.Status401Unauthorized)
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
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Project-level Pipeline Generation (mono-repo) ──────────────

            group.MapPost("/{projectId:guid}/generate-pipeline",
                    async ([FromRoute] Guid projectId, IMediator mediator) =>
                    {
                        var command = new GenerateProjectPipelineCommand(new ProjectId(projectId));
                        var result = await mediator.Send(command);

                        return result.Match(
                            value =>
                            {
                                var response = new GenerateProjectPipelineResponse(
                                    value.CommonFileUris,
                                    value.ConfigFileUris);
                                return Results.Created($"/projects/{projectId}/generate-pipeline", response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GenerateProjectPipeline")
                .WithSummary("Generate pipeline files for the entire project (mono-repo)")
                .WithDescription("Generates Azure DevOps pipeline YAML files for all configurations in the project.")
                .Produces<GenerateProjectPipelineResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{projectId:guid}/generate-pipeline/download",
                    async ([FromRoute] Guid projectId, IMediator mediator) =>
                    {
                        var command = new DownloadProjectPipelineCommand(new ProjectId(projectId));
                        var result = await mediator.Send(command);

                        return result.Match(
                            value => Results.File(
                                value.ZipContent,
                                "application/zip",
                                value.FileName),
                            errors => errors.Result()
                        );
                    })
                .WithName("DownloadProjectPipeline")
                .WithSummary("Download generated pipeline files for a project")
                .WithDescription("Downloads the latest generated mono-repo pipeline files for the given project as a ZIP archive.")
                .Produces(StatusCodes.Status200OK, contentType: "application/zip")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{projectId:guid}/generate-pipeline/files/{*filePath}",
                    async ([FromRoute] Guid projectId, [FromRoute] string filePath, IMediator mediator) =>
                    {
                        var query = new GetProjectPipelineFileContentQuery(projectId, filePath);
                        var result = await mediator.Send(query);

                        return result.Match(
                            value => Results.Ok(new { content = value.Content }),
                            errors => errors.Result()
                        );
                    })
                .WithName("GetProjectPipelineFileContent")
                .WithSummary("Get generated pipeline file content for a project")
                .WithDescription("Reads the latest generated mono-repo pipeline file content for the given project and relative file path.")
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Project-level Pipeline Push to Git (mono-repo) ───────────────────

            group.MapPost("/{projectId:guid}/push-pipeline-to-git",
                    async ([FromRoute] Guid projectId,
                        [FromBody] PushBicepToGitRequest request,
                        IMediator mediator,
                        IMapper mapper) =>
                    {
                        var command = new PushProjectPipelineToGitCommand(
                            new ProjectId(projectId),
                            request.BranchName,
                            request.CommitMessage);
                        var result = await mediator.Send(command);

                        return result.Match(
                            value => Results.Ok(mapper.Map<PushBicepToGitResponse>(value)),
                            errors => errors.Result()
                        );
                    })
                .WithName("PushProjectPipelineToGit")
                .WithSummary("Push project-level pipeline files to Git (mono-repo)")
                .WithDescription("Pushes the latest project-level generated pipeline files to the configured Git repository. Used in MonoRepo mode.")
                .Produces<PushBicepToGitResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Project-level Bootstrap Pipeline (Azure DevOps) ──────────────────────────────

            group.MapPost("/{projectId:guid}/generate-bootstrap-pipeline",
                    async ([FromRoute] Guid projectId,
                        IMediator mediator) =>
                    {
                        var command = new GenerateProjectBootstrapPipelineCommand(new ProjectId(projectId));
                        var result = await mediator.Send(command);

                        return result.Match(
                            value => Results.Created(
                                $"/projects/{projectId}/generate-bootstrap-pipeline/files/bootstrap.pipeline.yml",
                                new GenerateProjectBootstrapPipelineResponse(value.FileUris)),
                            errors => errors.Result()
                        );
                    })
                .WithName("GenerateProjectBootstrapPipeline")
                .WithSummary("Generate the Azure DevOps bootstrap pipeline for a project")
                .WithDescription("Generates bootstrap.pipeline.yml — an idempotent Azure DevOps pipeline that provisions pipeline definitions, variable groups and authorizations via az devops CLI.")
                .Produces<GenerateProjectBootstrapPipelineResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{projectId:guid}/generate-bootstrap-pipeline/download",
                    async ([FromRoute] Guid projectId,
                        IMediator mediator) =>
                    {
                        var command = new DownloadProjectBootstrapPipelineCommand(new ProjectId(projectId));
                        var result = await mediator.Send(command);

                        return result.Match(
                            value => Results.File(
                                value.ZipContent,
                                "application/zip",
                                value.FileName),
                            errors => errors.Result()
                        );
                    })
                .WithName("DownloadProjectBootstrapPipeline")
                .WithSummary("Download the latest bootstrap pipeline as a ZIP archive")
                .WithDescription("Returns a ZIP archive containing the latest generated bootstrap.pipeline.yml for the given project.")
                .Produces<FileContentResult>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{projectId:guid}/generate-bootstrap-pipeline/files/{*filePath}",
                    async ([FromRoute] Guid projectId,
                        [FromRoute] string filePath,
                        IMediator mediator) =>
                    {
                        var query = new GetProjectBootstrapPipelineFileContentQuery(projectId, filePath);
                        var result = await mediator.Send(query);

                        return result.Match(
                            value => Results.Ok(new { content = value.Content }),
                            errors => errors.Result()
                        );
                    })
                .WithName("GetProjectBootstrapPipelineFileContent")
                .WithSummary("Get generated bootstrap pipeline file content for a project")
                .WithDescription("Reads the latest generated bootstrap pipeline file content for the given project and relative file path.")
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/{projectId:guid}/push-bootstrap-pipeline-to-git",
                    async ([FromRoute] Guid projectId,
                        [FromBody] PushBicepToGitRequest request,
                        IMediator mediator,
                        IMapper mapper) =>
                    {
                        var command = new PushProjectBootstrapPipelineToGitCommand(
                            new ProjectId(projectId),
                            request.BranchName,
                            request.CommitMessage);
                        var result = await mediator.Send(command);

                        return result.Match(
                            value => Results.Ok(mapper.Map<PushBicepToGitResponse>(value)),
                            errors => errors.Result()
                        );
                    })
                .WithName("PushProjectBootstrapPipelineToGit")
                .WithSummary("Push the bootstrap pipeline file to Git (Azure DevOps)")
                .WithDescription("Pushes the latest generated bootstrap.pipeline.yml to the configured Git repository.")
                .Produces<PushBicepToGitResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/{projectId:guid}/push-generated-artifacts-to-git",
                    async ([FromRoute] Guid projectId,
                        [FromBody] PushBicepToGitRequest request,
                        IMediator mediator,
                        IMapper mapper) =>
                    {
                        var command = new PushProjectGeneratedArtifactsToGitCommand(
                            new ProjectId(projectId),
                            request.BranchName,
                            request.CommitMessage);
                        var result = await mediator.Send(command);

                        return result.Match(
                            value => Results.Ok(mapper.Map<PushBicepToGitResponse>(value)),
                            errors => errors.Result()
                        );
                    })
                .WithName("PushProjectGeneratedArtifactsToGit")
                .WithSummary("Push generated project artifacts to Git in a single commit (mono-repo)")
                .WithDescription("Pushes the latest project-level generated Bicep, pipeline, and bootstrap pipeline files to the configured Git repository in one provider call and one commit.")
                .Produces<PushBicepToGitResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Pipeline Variable Groups (project-level) ────────────────

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
                .WithName("ListProjectPipelineVariableGroups")
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
                .WithName("AddProjectPipelineVariableGroup")
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
                .WithName("RemoveProjectPipelineVariableGroup")
                .WithSummary("Remove a project-level pipeline variable group")
                .WithDescription("Removes a variable group from the project.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
