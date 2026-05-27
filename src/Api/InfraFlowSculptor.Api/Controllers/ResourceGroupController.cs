using InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Commands.DeleteResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Commands.UpdateResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Queries.GetResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupResources;
using MediatR;
using InfraFlowSculptor.Contracts.ResourceGroups.Requests;
using InfraFlowSculptor.Contracts.ResourceGroups.Responses;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using InfraFlowSculptor.Api.Errors;

using InfraFlowSculptor.Api.Controllers.Constants;

namespace InfraFlowSculptor.Api.Controllers;

public static class ResourceGroupController
{
    public static IApplicationBuilder UseResourceGroupController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var config = endpoints.MapGroup(Routes.ResourceGroup)
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
                .WithName(ResourceGroupRouteNames.GetResourceGroup)
                .WithSummary("Get a Resource Group")
                .WithDescription("Returns the full details of a single Resource Group, including its Azure region.")
                .Produces<ResourceGroupResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
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
                .WithName(ResourceGroupRouteNames.ListResourceGroupResources)
                .WithSummary("List resources in a Resource Group")
                .WithDescription("Returns a lightweight list of all Azure resources (Key Vaults, Storage Accounts, Redis Caches, etc.) that belong to the specified Resource Group.")
                .Produces<IReadOnlyList<AzureResourceResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
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
                                    routeName: ResourceGroupRouteNames.GetResourceGroup,
                                    routeValues: new { id = resourceGroup.Id.Value },
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(ResourceGroupRouteNames.CreateResourceGroup)
                .WithSummary("Create a Resource Group")
                .WithDescription("Creates a new Azure Resource Group inside an existing Infrastructure Configuration. Requires Owner or Contributor access.")
                .Produces<ResourceGroupResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            config.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateResourceGroupRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new UpdateResourceGroupCommand(
                            new ResourceGroupId(id),
                            mapper.Map<Domain.Common.ValueObjects.Name>(request.Name),
                            mapper.Map<Domain.Common.ValueObjects.Location>(request.Location));
                        var result = await mediator.Send(command);

                        return result.Match(
                            resourceGroup =>
                            {
                                var response = mapper.Map<ResourceGroupResponse>(resourceGroup);
                                return TypedResults.Ok(response);
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName(ResourceGroupRouteNames.UpdateResourceGroup)
                .WithSummary("Update a Resource Group")
                .WithDescription("Updates the name and location of an existing Resource Group. Requires Owner or Contributor access.")
                .Produces<ResourceGroupResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
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
                .WithName(ResourceGroupRouteNames.DeleteResourceGroup)
                .WithSummary("Delete a Resource Group")
                .WithDescription("Permanently deletes a Resource Group and all its contained Azure resources. Requires Owner or Contributor access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}

