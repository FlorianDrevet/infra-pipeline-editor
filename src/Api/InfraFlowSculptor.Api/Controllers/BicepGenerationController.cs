using InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadBicep;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBicep;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushBicepToGit;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetBicepFileContent;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

public static class BicepGenerationController
{
    public static IApplicationBuilder UseBicepGenerationController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/generate-bicep")
                .WithTags("Generate Bicep");

            group.MapPost("",
                    async (GenerateBicepRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new GenerateBicepCommand(request.InfrastructureConfigId);
                        var result = await mediator.Send(command);

                        return result.Match(
                            value =>
                            {
                                var response = mapper.Map<GenerateBicepResponse>(value);
                                return Results.Created(value.MainBicepUri.ToString(), response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GenerateBicep")
                .Produces<GenerateBicepResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapGet("/{configId:guid}/download",
                    async (Guid configId, IMediator mediator) =>
                    {
                        var command = new DownloadBicepCommand(configId);
                        var result = await mediator.Send(command);

                        return result.Match(
                            value => Results.File(
                                value.ZipContent,
                                "application/zip",
                                value.FileName),
                            errors => errors.Result()
                        );
                    })
                .WithName("DownloadBicep")
                .Produces(StatusCodes.Status200OK, contentType: "application/zip")
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapGet("/{configId:guid}/files/{*filePath}",
                    async (Guid configId, string filePath, IMediator mediator) =>
                    {
                        var query = new GetBicepFileContentQuery(configId, filePath);
                        var result = await mediator.Send(query);

                        return result.Match(
                            value => Results.Ok(new { content = value.Content }),
                            errors => errors.Result()
                        );
                    })
                .WithName("GetBicepFileContent")
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPost("/{configId:guid}/push-to-git",
                    async ([FromRoute] Guid configId,
                        [FromBody] PushBicepToGitRequest request,
                        IMediator mediator,
                        IMapper mapper) =>
                    {
                        var command = new PushBicepToGitCommand(configId, request.BranchName, request.CommitMessage);
                        var result = await mediator.Send(command);

                        return result.Match(
                            value => Results.Ok(mapper.Map<PushBicepToGitResponse>(value)),
                            errors => errors.Result()
                        );
                    })
                .WithName("PushBicepToGit")
                .WithSummary("Push generated Bicep files to Git")
                .WithDescription("Pushes the latest generated Bicep files to the configured Git repository, creating or updating the specified branch.")
                .Produces<PushBicepToGitResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
