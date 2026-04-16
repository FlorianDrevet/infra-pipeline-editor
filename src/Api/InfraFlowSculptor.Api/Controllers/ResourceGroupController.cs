using InfraFlowSculptor.Application.KeyVaults.Queries;
using InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Commands.CreateResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Commands.DeleteResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Queries.GetResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupResources;
using InfraFlowSculptor.Contracts.KeyVaults.Responses;
using MediatR;
using InfraFlowSculptor.Contracts.ResourceGroups.Requests;
using InfraFlowSculptor.Contracts.ResourceGroups.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

namespace InfraFlowSculptor.Api.Controllers;

public static class ResourceGroupController
{
    public static IApplicationBuilder UseResourceGroupController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var config = endpoints.MapGroup("/resource-group")
                .WithTags("ResourceGroups");

            config.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetResourceGroupQuery(new ResourceGroupId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            resourceGroup =>
                            {
                                var response = mapper.Map<ResourceGroupResponse>(resourceGroup);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("GetResourceGroup")
                .WithSummary("Get a Resource Group")
                .WithDescription("Returns the full details of a single Resource Group, including its Azure region.")
                .Produces<ResourceGroupResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            config.MapGet("/{id:guid}/resources",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new ListResourceGroupResourcesQuery(new ResourceGroupId(id));
                        var result = await mediator.Send(query);

                        return result.Match(
                            resources =>
                            {
                                var response = resources.Select(r => mapper.Map<AzureResourceResponse>(r)).ToList();
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("ListResourceGroupResources")
                .WithSummary("List resources in a Resource Group")
                .WithDescription("Returns a lightweight list of all Azure resources (Key Vaults, Storage Accounts, Redis Caches, etc.) that belong to the specified Resource Group.")
                .Produces<IReadOnlyCollection<AzureResourceResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            config.MapPost("",
                    async (CreateResourceGroupRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateResourceGroupCommand>(request);
                        var result = await mediator.Send(command);

                        return result.Match(
                            resourceGroup =>
                            {
                                var response = mapper.Map<ResourceGroupResponse>(resourceGroup);
                                return TypedResults.CreatedAtRoute(
                                    routeName: "GetResourceGroup",
                                    routeValues: new { id = resourceGroup.Id.Value },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateResourceGroup")
                .WithSummary("Create a Resource Group")
                .WithDescription("Creates a new Azure Resource Group inside an existing Infrastructure Configuration. Requires Owner or Contributor access.")
                .Produces<ResourceGroupResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            config.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteResourceGroupCommand(new ResourceGroupId(id));
                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => Results.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName("DeleteResourceGroup")
                .WithSummary("Delete a Resource Group")
                .WithDescription("Permanently deletes a Resource Group and all its contained Azure resources. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
