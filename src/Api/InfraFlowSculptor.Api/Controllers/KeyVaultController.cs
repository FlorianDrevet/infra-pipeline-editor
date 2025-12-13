using InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Queries;
using MediatR;
using InfraFlowSculptor.Contracts.KeyVaults.Requests;
using InfraFlowSculptor.Contracts.KeyVaults.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

public static class KeyVaultControllerController
{
    public static IApplicationBuilder UseKeyVaultControllerController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var config = endpoints.MapGroup("/keyvault")
                .WithTags("KeyVaults");

            config.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetKeyVaultQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            keyVault =>
                            {
                                var response = mapper.Map<KeyVaultResponse>(keyVault);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetKeyVault");

            config.MapPost("",
                    async (CreateKeyVaultRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateKeyVaultCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            keyVault =>
                            {
                                var response = mapper.Map<KeyVaultResponse>(keyVault);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "GetKeyVault",
                                    routeValues: new { id = response.Id },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateKeyVault");
        });
    }
}