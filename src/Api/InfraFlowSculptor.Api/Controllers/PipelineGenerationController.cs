using InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadPipeline;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.GeneratePipeline;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushPipelineToGit;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetPipelineFileContent;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Api.Common;
using InfraFlowSculptor.Api.RateLimiting;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

using InfraFlowSculptor.Api.Controllers.Constants;

namespace InfraFlowSculptor.Api.Controllers;

public static class PipelineGenerationController
{
    public static IApplicationBuilder UsePipelineGenerationController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup(Routes.GeneratePipeline)
                .WithTags("Generate Pipeline");

            group.MapPost("",
                    async (GeneratePipelineRequest request, IMediator mediator) =>
                    {
                        var command = new GeneratePipelineCommand(request.InfrastructureConfigId);
                        var result = await mediator.Send(command);

                        return result.Match(
                            value =>
                            {
                                var response = new GeneratePipelineResponse(value.FileUris);
                                return Results.Created((string?)null, response);
                            },
                            errors => errors.Result()
                        );
                    })
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(PipelineGenerationRouteNames.GeneratePipeline)
                .Produces<GeneratePipelineResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapGet("/{configId:guid}/download",
                    async (Guid configId, IMediator mediator) =>
                    {
                        var command = new DownloadPipelineCommand(configId);
                        var result = await mediator.Send(command);

                        return result.Match(
                            value => Results.File(
                                value.ZipContent,
                                "application/zip",
                                value.FileName),
                            errors => errors.Result()
                        );
                    })
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(PipelineGenerationRouteNames.DownloadPipeline)
                .Produces(StatusCodes.Status200OK, contentType: "application/zip")
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapGet("/{configId:guid}/files/{*filePath}",
                    async (Guid configId, string filePath, IMediator mediator) =>
                    {
                        if (!SafeRelativePath.TryNormalize(filePath, out var safePath))
                        {
                            return Results.BadRequest(new { message = "Invalid file path." });
                        }

                        var query = new GetPipelineFileContentQuery(configId, safePath);
                        var result = await mediator.Send(query);

                        return result.Match(
                            value => Results.Ok(new { content = value.Content }),
                            errors => errors.Result()
                        );
                    })
                .WithName(PipelineGenerationRouteNames.GetPipelineFileContent)
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPost("/{configId:guid}/push-to-git",
                    async ([FromRoute] Guid configId,
                        [FromBody] PushPipelineToGitRequest request,
                        IMediator mediator,
                        IMapper mapper) =>
                    {
                        var command = new PushPipelineToGitCommand(configId, request.BranchName, request.CommitMessage);
                        var result = await mediator.Send(command);

                        return result.Match(
                            value => Results.Ok(mapper.Map<PushPipelineToGitResponse>(value)),
                            errors => errors.Result()
                        );
                    })
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(PipelineGenerationRouteNames.PushPipelineToGit)
                .WithSummary("Push generated pipeline files to Git")
                .WithDescription("Pushes the latest generated Azure DevOps pipeline files to the configured Git repository.")
                .Produces<PushPipelineToGitResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}

