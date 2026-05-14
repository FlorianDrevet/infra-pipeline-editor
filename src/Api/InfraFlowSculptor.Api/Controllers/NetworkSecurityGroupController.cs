using InfraFlowSculptor.Api.Controllers.Constants;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.CreateNetworkSecurityGroup;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.DeleteNetworkSecurityGroup;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.UpdateNetworkSecurityGroup;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Queries;
using InfraFlowSculptor.Contracts.NetworkSecurityGroups.Requests;
using InfraFlowSculptor.Contracts.NetworkSecurityGroups.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Minimal API endpoints for Network Security Group resources.</summary>
public static class NetworkSecurityGroupController
{
    /// <summary>Registers Network Security Group endpoints.</summary>
    public static IApplicationBuilder UseNetworkSecurityGroupController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/network-security-group")
                .WithTags("NetworkSecurityGroups");

            group.MapGet("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetNetworkSecurityGroupQuery(new AzureResourceId(id));
                        var result = await mediator.Send(query);
                        return result.Match(
                            nsg => TypedResults.Ok(mapper.Map<NetworkSecurityGroupResponse>(nsg)),
                            errors => errors.Result());
                    })
                .WithName(NetworkSecurityGroupRouteNames.GetNetworkSecurityGroup)
                .WithSummary("Get a Network Security Group")
                .Produces<NetworkSecurityGroupResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async (CreateNetworkSecurityGroupRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<CreateNetworkSecurityGroupCommand>(request);
                        var result = await mediator.Send(command);
                        return result.Match(
                            nsg =>
                            {
                                var response = mapper.Map<NetworkSecurityGroupResponse>(nsg);
                                return TypedResults.CreatedAtRoute(
                                    routeName: NetworkSecurityGroupRouteNames.GetNetworkSecurityGroup,
                                    routeValues: new { id = response.Id },
                                    value: response);
                            },
                            errors => errors.Result());
                    })
                .WithName(NetworkSecurityGroupRouteNames.CreateNetworkSecurityGroup)
                .WithSummary("Create a Network Security Group")
                .Produces<NetworkSecurityGroupResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}",
                    async ([FromRoute] Guid id, UpdateNetworkSecurityGroupRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = mapper.Map<UpdateNetworkSecurityGroupCommand>((id, request));
                        var result = await mediator.Send(command);
                        return result.Match(
                            nsg => TypedResults.Ok(mapper.Map<NetworkSecurityGroupResponse>(nsg)),
                            errors => errors.Result());
                    })
                .WithName(NetworkSecurityGroupRouteNames.UpdateNetworkSecurityGroup)
                .WithSummary("Update a Network Security Group")
                .Produces<NetworkSecurityGroupResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}",
                    async ([FromRoute] Guid id, IMediator mediator) =>
                    {
                        var command = new DeleteNetworkSecurityGroupCommand(new AzureResourceId(id));
                        var result = await mediator.Send(command);
                        return result.Match(_ => Results.NoContent(), errors => errors.Result());
                    })
                .WithName(NetworkSecurityGroupRouteNames.DeleteNetworkSecurityGroup)
                .WithSummary("Delete a Network Security Group")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
