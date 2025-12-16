using BicepGenerator.Application.Commands.GenerateBicep;
using BicepGenerator.Contracts.GenerateBicep.Requests;
using BicepGenerator.Contracts.GenerateBicep.Responses;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Errors;

namespace BicepGenerator.Api.Controllers;

public static class KeyVaultControllerController
{
    public static IApplicationBuilder UseBicepGenerationControllerController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var config = endpoints.MapGroup("/generate-bicep")
                .WithTags("Generate Bicep");

            config.MapPost("",
                    async (GenerateBicepRequest request, IMediator mediator) =>
                    {
                        var command = new GenerateBicepCommand();
                        var result = await mediator.Send(command);

                        return result.Match(
                            bicepUri =>
                            {
                                var response = new GenerateBicepResponse(bicepUri);
                                return Results.Created($"", response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GenerateBicep");
        });
    }
}