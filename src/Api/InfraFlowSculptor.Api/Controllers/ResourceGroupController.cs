using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.KeyVaults.Queries;
using InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Commands.CreateResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Queries.GetResourceGroup;
using InfraFlowSculptor.Contracts.KeyVaults.Responses;
using MediatR;
using InfraFlowSculptor.Contracts.ResourceGroups.Requests;
using InfraFlowSculptor.Contracts.ResourceGroups.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

public static class ResourceGroupController
{
    public static IApplicationBuilder UseResourceGroupController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var config = endpoints.MapGroup("/resource-group")
                .WithTags("ResourceGroups")
                .WithOpenApi();
            
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
                .WithOpenApi();
            
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
                                    routeName: "resource-group",
                                    routeValues: resourceGroup.Id,
                                    value: response
                                );
                            },
                            errors => errors.Result()
                        );
                    })
                .WithName("CreateResourceGroup")
                .WithOpenApi();
        });
    }
}
