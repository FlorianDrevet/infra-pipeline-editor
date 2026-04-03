using InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadPipeline;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateAppPipeline;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.GeneratePipeline;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushPipelineToGit;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetPipelineFileContent;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

public static class PipelineGenerationController
{
    public static IApplicationBuilder UsePipelineGenerationController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/generate-pipeline")
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
                .WithName("GeneratePipeline")
                .Produces<GeneratePipelineResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status404NotFound)
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
                .WithName("DownloadPipeline")
                .Produces(StatusCodes.Status200OK, contentType: "application/zip")
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapGet("/{configId:guid}/files/{*filePath}",
                    async (Guid configId, string filePath, IMediator mediator) =>
                    {
                        var query = new GetPipelineFileContentQuery(configId, filePath);
                        var result = await mediator.Send(query);

                        return result.Match(
                            value => Results.Ok(new { content = value.Content }),
                            errors => errors.Result()
                        );
                    })
                .WithName("GetPipelineFileContent")
                .Produces(StatusCodes.Status200OK)
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
                .WithName("PushPipelineToGit")
                .WithSummary("Push generated pipeline files to Git")
                .WithDescription("Pushes the latest generated Azure DevOps pipeline files to the configured Git repository.")
                .Produces<PushPipelineToGitResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("/app",
                    async (GenerateAppPipelineRequest request, IMediator mediator) =>
                    {
                        var command = new GenerateAppPipelineCommand(
                            request.InfrastructureConfigId,
                            request.ResourceId);
                        var result = await mediator.Send(command);

                        return result.Match(
                            value =>
                            {
                                var response = new GenerateAppPipelineResponse(value.FileUris);
                                return Results.Created((string?)null, response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GenerateAppPipeline")
                .WithSummary("Generate application CI/CD pipeline for a compute resource")
                .WithDescription("Generates Azure DevOps pipeline YAML files for building and deploying application code to a Container App, Web App, or Function App.")
                .Produces<GenerateAppPipelineResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);
        });
    }
}
