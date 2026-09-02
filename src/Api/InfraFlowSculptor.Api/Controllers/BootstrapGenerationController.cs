using InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadBootstrap;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBootstrap;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushBootstrapToGit;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetBootstrapFileContent;
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

/// <summary>
/// Infrastructure-configuration-level bootstrap generation endpoints. Config-level counterpart of
/// <see cref="PipelineGenerationController"/>, used when the owning project declares a
/// <c>MultiRepo</c> layout (each configuration owns its own repository and its own bootstrap
/// pipeline definition).
/// </summary>
public static class BootstrapGenerationController
{
    public static IApplicationBuilder UseBootstrapGenerationController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup(Routes.GenerateBootstrap)
                .WithTags("Generate Bootstrap");

            group.MapPost("",
                    async (GenerateBootstrapRequest request, IMediator mediator) =>
                    {
                        var command = new GenerateBootstrapCommand(request.InfrastructureConfigId);
                        var result = await mediator.Send(command);

                        return result.Match(
                            value =>
                            {
                                var response = new GenerateBootstrapResponse(
                                    value.FileUris,
                                    value.InfraFileUris,
                                    value.AppFileUris);
                                return Results.Created((string?)null, response);
                            },
                            errors => errors.Result()
                        );
                    })
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(BootstrapGenerationRouteNames.GenerateBootstrap)
                .Produces<GenerateBootstrapResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapGet("/{configId:guid}/download",
                    async (Guid configId, IMediator mediator) =>
                    {
                        var command = new DownloadBootstrapCommand(configId);
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
                .WithName(BootstrapGenerationRouteNames.DownloadBootstrap)
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

                        var query = new GetBootstrapFileContentQuery(configId, safePath);
                        var result = await mediator.Send(query);

                        return result.Match(
                            value => Results.Ok(new { content = value.Content }),
                            errors => errors.Result()
                        );
                    })
                .WithName(BootstrapGenerationRouteNames.GetBootstrapFileContent)
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPost("/{configId:guid}/push-to-git",
                    async ([FromRoute] Guid configId,
                        [FromBody] PushBootstrapToGitRequest request,
                        IMediator mediator,
                        IMapper mapper) =>
                    {
                        var command = new PushBootstrapToGitCommand(configId, request.BranchName, request.CommitMessage);
                        var result = await mediator.Send(command);

                        return result.Match(
                            value => Results.Ok(mapper.Map<PushBootstrapToGitResponse>(value)),
                            errors => errors.Result()
                        );
                    })
                .RequireRateLimiting(RateLimitingPolicyNames.Expensive)
                .WithName(BootstrapGenerationRouteNames.PushBootstrapToGit)
                .WithSummary("Push generated bootstrap files to Git")
                .WithDescription("Pushes the latest generated bootstrap pipeline file to the configuration's own Git repository (MultiRepo layout).")
                .Produces<PushBootstrapToGitResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
