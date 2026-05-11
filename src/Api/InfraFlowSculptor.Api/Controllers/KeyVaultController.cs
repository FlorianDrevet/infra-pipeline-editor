using InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Commands.DeleteKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Commands.UpdateKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Queries;
using MediatR;
using InfraFlowSculptor.Contracts.KeyVaults.Requests;
using InfraFlowSculptor.Contracts.KeyVaults.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

public static class KeyVaultController
{
    public static IApplicationBuilder UseKeyVaultController(this IApplicationBuilder builder)
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
                .WithName("GetKeyVault")
                .WithSummary("Get a Key Vault")
                .WithDescription("Returns the full details of a single Azure Key Vault resource.")
                .Produces<KeyVaultResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

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
                .WithName("CreateKeyVault")
                .WithSummary("Create a Key Vault")
                .WithDescription("Creates a new Azure Key Vault resource inside the specified Resource Group. Requires Owner or Contributor access.")
                .Produces<KeyVaultResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            config.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateKeyVaultRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateKeyVaultCommand>((id, request));
                        var result = await mediator.Send(command);

                        return result.Match(
                            keyVault =>
                            {
                                var response = mapper.Map<KeyVaultResponse>(keyVault);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("UpdateKeyVault")
                .WithSummary("Update a Key Vault")
                .WithDescription("Replaces all mutable properties of an existing Key Vault. Requires Owner or Contributor access.")
                .Produces<KeyVaultResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            config.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteKeyVaultCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteKeyVault")
                .WithSummary("Delete a Key Vault")
                .WithDescription("Permanently deletes an Azure Key Vault resource. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
