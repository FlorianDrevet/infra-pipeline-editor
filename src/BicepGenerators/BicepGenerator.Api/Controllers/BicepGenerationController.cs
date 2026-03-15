using BicepGenerator.Application.Commands.GenerateBicep;
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
                                    value.ParametersUri,
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
        });
    }
}
