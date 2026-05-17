using InfraFlowSculptor.Api.Common;
using InfraFlowSculptor.Api.Controllers.Constants;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Api.RateLimiting;
using InfraFlowSculptor.Application.Projects.Commands.DownloadProjectBicep;
using InfraFlowSculptor.Application.Projects.Commands.DownloadProjectBootstrapPipeline;
using InfraFlowSculptor.Application.Projects.Commands.DownloadProjectPipeline;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBicep;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectPipeline;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectArtifactsToMultiRepo;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectBicepToGit;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectBootstrapPipelineToGit;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectGeneratedArtifactsToGit;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectPipelineToGit;
using InfraFlowSculptor.Application.Projects.Queries.GetProjectBicepFileContent;
using InfraFlowSculptor.Application.Projects.Queries.GetProjectBootstrapPipelineFileContent;
using InfraFlowSculptor.Application.Projects.Queries.GetProjectLatestGeneration;
using InfraFlowSculptor.Application.Projects.Queries.GetProjectPipelineFileContent;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Projects.Responses;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoint definitions for Project generation, download, and push operations.</summary>
public static class ProjectGenerationController
{
    private const string ZipContentType = "application/zip";

    /// <summary>Registers the Project generation endpoints on the application builder.</summary>
    public static IApplicationBuilder UseProjectGenerationController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/projects")
                .WithTags("Projects");

            MapLatestGenerationEndpoint(group);
            MapBicepGenerationEndpoints(group);
            MapPipelineGenerationEndpoints(group);
            MapBootstrapAndPushArtifactEndpoints(group);
        });
    }

    private static void MapBicepGenerationEndpoints(RouteGroupBuilder group)
    {

            // ── Project-level Bicep Generation (mono-repo) ──────────

            group.MapPost("/{projectId:guid}/generate-bicep",
                    async ([FromRoute] Guid projectId, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var command = new GenerateProjectBicepCommand(new ProjectId(projectId));
                        var result = await mediator.Send(command, cancellationToken);

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
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(ProjectRouteNames.GenerateProjectBicep)
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
                                ZipContentType,
                                value.FileName),
                            errors => errors.Result()
                        );
                    })
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(ProjectRouteNames.DownloadProjectBicep)
                .WithSummary("Download generated Bicep files for a project")
                .WithDescription("Downloads the latest generated mono-repo Bicep files for the given project as a ZIP archive.")
                .Produces(StatusCodes.Status200OK, contentType: ZipContentType)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{projectId:guid}/generate-bicep/files/{*filePath}",
                    async ([FromRoute] Guid projectId, [FromRoute] string filePath, IMediator mediator) =>
                    {
                        // Audit SEC-004 (2026-04-23): reject path traversal before reaching the handler.
                        if (!SafeRelativePath.TryNormalize(filePath, out var safePath))
                            return Results.BadRequest(new { error = "Invalid file path" });

                        var query = new GetProjectBicepFileContentQuery(projectId, safePath);
                        var result = await mediator.Send(query);

                        return result.Match(
                            value => Results.Ok(new { content = value.Content }),
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.GetProjectBicepFileContent)
                .WithSummary("Get generated Bicep file content for a project")
                .WithDescription("Reads the latest generated mono-repo Bicep file content for the given project and relative file path.")
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Project-level Push to Git (mono-repo) ───────────────

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
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(ProjectRouteNames.PushProjectBicepToGit)
                .WithSummary("Push project-level Bicep files to Git (mono-repo)")
                .WithDescription("Pushes the latest project-level generated Bicep files to the configured Git repository. Used in MonoRepo mode.")
                .Produces<PushBicepToGitResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static void MapPipelineGenerationEndpoints(RouteGroupBuilder group)
    {
            // ── Project-level Pipeline Generation (mono-repo) ──────────

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
                                    value.ConfigFileUris,
                                    value.InfraCommonFileUris,
                                    value.AppCommonFileUris,
                                    value.InfraConfigFileUris,
                                    value.AppConfigFileUris);
                                return Results.Created($"/projects/{projectId}/generate-pipeline", response);
                            },
                            errors => errors.Result()
                        );
                    })
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(ProjectRouteNames.GenerateProjectPipeline)
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
                                ZipContentType,
                                value.FileName),
                            errors => errors.Result()
                        );
                    })
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(ProjectRouteNames.DownloadProjectPipeline)
                .WithSummary("Download generated pipeline files for a project")
                .WithDescription("Downloads the latest generated mono-repo pipeline files for the given project as a ZIP archive.")
                .Produces(StatusCodes.Status200OK, contentType: ZipContentType)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapGet("/{projectId:guid}/generate-pipeline/files/{*filePath}",
                    async ([FromRoute] Guid projectId, [FromRoute] string filePath, IMediator mediator) =>
                    {
                        // Audit SEC-004 (2026-04-23): reject path traversal before reaching the handler.
                        if (!SafeRelativePath.TryNormalize(filePath, out var safePath))
                            return Results.BadRequest(new { error = "Invalid file path" });

                        var query = new GetProjectPipelineFileContentQuery(projectId, safePath);
                        var result = await mediator.Send(query);

                        return result.Match(
                            value => Results.Ok(new { content = value.Content }),
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.GetProjectPipelineFileContent)
                .WithSummary("Get generated pipeline file content for a project")
                .WithDescription("Reads the latest generated mono-repo pipeline file content for the given project and relative file path.")
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            // ── Project-level Pipeline Push to Git (mono-repo) ───────────────

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
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(ProjectRouteNames.PushProjectPipelineToGit)
                .WithSummary("Push project-level pipeline files to Git (mono-repo)")
                .WithDescription("Pushes the latest project-level generated pipeline files to the configured Git repository. Used in MonoRepo mode.")
                .Produces<PushBicepToGitResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static void MapBootstrapAndPushArtifactEndpoints(RouteGroupBuilder group)
    {
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
                                new GenerateProjectBootstrapPipelineResponse(value.FileUris, value.InfraFileUris, value.AppFileUris)),
                            errors => errors.Result()
                        );
                    })
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(ProjectRouteNames.GenerateProjectBootstrapPipeline)
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
                                ZipContentType,
                                value.FileName),
                            errors => errors.Result()
                        );
                    })
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(ProjectRouteNames.DownloadProjectBootstrapPipeline)
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
                        // Audit SEC-004 (2026-04-23): reject path traversal before reaching the handler.
                        if (!SafeRelativePath.TryNormalize(filePath, out var safePath))
                            return Results.BadRequest(new { error = "Invalid file path" });

                        var query = new GetProjectBootstrapPipelineFileContentQuery(projectId, safePath);
                        var result = await mediator.Send(query);

                        return result.Match(
                            value => Results.Ok(new { content = value.Content }),
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.GetProjectBootstrapPipelineFileContent)
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
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(ProjectRouteNames.PushProjectBootstrapPipelineToGit)
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
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(ProjectRouteNames.PushProjectGeneratedArtifactsToGit)
                .WithSummary("Push generated project artifacts to Git in a single commit (mono-repo)")
                .WithDescription("Pushes the latest project-level generated Bicep, pipeline, and bootstrap pipeline files to the configured Git repository in one provider call and one commit.")
                .Produces<PushBicepToGitResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/{projectId:guid}/push-multi-repo-artifacts-to-git",
                    async ([FromRoute] Guid projectId,
                        [FromBody] PushMultiRepoArtifactsRequest request,
                        IMediator mediator) =>
                    {
                        var command = new PushProjectArtifactsToMultiRepoCommand(
                            new ProjectId(projectId),
                            request.Infra is null
                                ? null
                                : new RepoPushTarget(
                                    request.Infra.Alias,
                                    request.Infra.BranchName,
                                    request.Infra.CommitMessage),
                            request.Code is null
                                ? null
                                : new RepoPushTarget(
                                    request.Code.Alias,
                                    request.Code.BranchName,
                                    request.Code.CommitMessage));

                        var result = await mediator.Send(command);

                        return result.Match(
                            value =>
                            {
                                var response = new PushMultiRepoArtifactsResponse(
                                    value.Results
                                        .Select(r => new RepoPushResultResponse(
                                            r.Alias,
                                            r.Success,
                                            r.BranchUrl,
                                            r.CommitSha,
                                            r.FileCount,
                                            r.ErrorCode,
                                            r.ErrorDescription))
                                        .ToList());
                                return Results.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(ProjectRouteNames.PushProjectArtifactsToMultiRepo)
                .WithSummary("Push project artifacts to one or two repositories (SplitInfraCode multi push)")
                .WithDescription("Pushes the latest project-level generated artifacts to the requested infrastructure-flagged repository (Bicep + infra pipeline + bootstrap), the requested application-code repository (app pipeline files), or both in independent commits. Per-repo errors are reported in the response, not as HTTP errors.")
                .Produces<PushMultiRepoArtifactsResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static void MapLatestGenerationEndpoint(RouteGroupBuilder group)
    {
            group.MapGet("/{projectId:guid}/latest-generation",
                    async ([FromRoute] Guid projectId, IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var query = new GetProjectLatestGenerationQuery(projectId);
                        var result = await mediator.Send(query, cancellationToken);

                        return result.Match(
                            value =>
                            {
                                if (value.Bicep is null && value.Pipeline is null && value.Bootstrap is null)
                                    return Results.NotFound();

                                var response = new GetProjectLatestGenerationResponse(
                                    value.Bicep is not null
                                        ? new LatestBicepGenerationResponse(value.Bicep.CommonFilePaths, value.Bicep.ConfigFilePaths)
                                        : null,
                                    value.Pipeline is not null
                                        ? new LatestPipelineGenerationResponse(
                                            value.Pipeline.CommonFilePaths, value.Pipeline.ConfigFilePaths,
                                            value.Pipeline.InfraCommonFilePaths, value.Pipeline.AppCommonFilePaths,
                                            value.Pipeline.InfraConfigFilePaths, value.Pipeline.AppConfigFilePaths)
                                        : null,
                                    value.Bootstrap is not null
                                        ? new LatestBootstrapGenerationResponse(value.Bootstrap.FilePaths, value.Bootstrap.InfraFilePaths, value.Bootstrap.AppFilePaths)
                                        : null,
                                    value.GeneratedAt);
                                return Results.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(ProjectRouteNames.GetProjectLatestGeneration)
                .WithSummary("Get latest generation file listing")
                .WithDescription("Returns file paths of the latest generated artifacts (Bicep, Pipeline, Bootstrap) without re-generating. Returns 404 when no generation exists.")
                .Produces<GetProjectLatestGenerationResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
