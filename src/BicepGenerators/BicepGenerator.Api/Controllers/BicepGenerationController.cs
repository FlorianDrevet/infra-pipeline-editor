using BicepGenerator.Application.Commands.DownloadBicep;
using BicepGenerator.Application.Commands.GenerateBicep;
using BicepGenerator.Application.Queries.GetBicepFileContent;
using BicepGenerator.Contracts.GenerateBicep.Requests;
using BicepGenerator.Contracts.GenerateBicep.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Errors;

namespace BicepGenerator.Api.Controllers;

public static class BicepGenerationController
{
    public static IApplicationBuilder UseBicepGenerationController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/generate-bicep")
                .WithTags("Generate Bicep");

            group.MapPost("",
                    async (GenerateBicepRequest request, IMediator mediator) =>
                    {
                        var command = new GenerateBicepCommand(request.InfrastructureConfigId);
                        var result = await mediator.Send(command);

                        return result.Match(
                            value =>
                            {
                                var response = new GenerateBicepResponse(
                                    value.MainBicepUri,
                                    value.ParameterFileUris,
                                    value.ModuleUris);
                                return Results.Created(value.MainBicepUri.ToString(), response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GenerateBicep")
                .Produces<GenerateBicepResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status404NotFound)
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
                .ProducesProblem(StatusCodes.Status404NotFound);
        });
    }
}
