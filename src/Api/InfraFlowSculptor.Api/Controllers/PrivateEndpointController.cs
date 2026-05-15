using InfraFlowSculptor.Api.Controllers.Constants;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.PrivateEndpoints.Commands.AddPrivateEndpoint;
using InfraFlowSculptor.Application.PrivateEndpoints.Commands.RemovePrivateEndpoint;
using InfraFlowSculptor.Application.PrivateEndpoints.Commands.UpdatePrivateEndpoint;
using InfraFlowSculptor.Application.PrivateEndpoints.Queries.GetPrivateEndpointConfigs;
using InfraFlowSculptor.Contracts.PrivateEndpoints.Requests;
using InfraFlowSculptor.Contracts.PrivateEndpoints.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>Endpoints for managing private endpoint configurations on Azure resources.</summary>
public static class PrivateEndpointController
{
    /// <summary>Registers private endpoint management endpoints.</summary>
    public static IApplicationBuilder UsePrivateEndpointController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/resources/{resourceId:guid}/private-endpoints")
                .WithTags("PrivateEndpoints");

            group.MapGet("",
                    async ([FromRoute] Guid resourceId, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetPrivateEndpointConfigsQuery(new AzureResourceId(resourceId));
                        var result = await mediator.Send(query);
                        return result.Match(
                            configs => TypedResults.Ok(
                                configs.Select(c => mapper.Map<PrivateEndpointConfigResponse>(c)).ToList()),
                            errors => errors.Result());
                    })
                .WithName(PrivateEndpointRouteNames.GetPrivateEndpoints)
                .WithSummary("List private endpoint configurations")
                .Produces<List<PrivateEndpointConfigResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPost("",
                    async ([FromRoute] Guid resourceId, AddPrivateEndpointRequest request,
                        IMediator mediator, IMapper mapper) =>
                    {
                        var command = new AddPrivateEndpointCommand(
                            new AzureResourceId(resourceId),
                            new AzureResourceId(request.SubnetId),
                            request.GroupId,
                            request.AutoApproval,
                            request.PrivateDnsZoneId.HasValue
                                ? new AzureResourceId(request.PrivateDnsZoneId.Value)
                                : null,
                            request.CustomNetworkInterfaceName);
                        var result = await mediator.Send(command);
                        return result.Match(
                            config => TypedResults.Created(
                                $"/resources/{resourceId}/private-endpoints/{config.Id.Value}",
                                mapper.Map<PrivateEndpointConfigResponse>(config)),
                            errors => errors.Result());
                    })
                .WithName(PrivateEndpointRouteNames.AddPrivateEndpoint)
                .WithSummary("Add a private endpoint")
                .Produces<PrivateEndpointConfigResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPut("/{peId:guid}",
                    async ([FromRoute] Guid resourceId, [FromRoute] Guid peId,
                        UpdatePrivateEndpointRequest request, IMediator mediator, IMapper mapper) =>
                    {
                        var command = new UpdatePrivateEndpointCommand(
                            new AzureResourceId(resourceId),
                            new PrivateEndpointConfigId(peId),
                            new AzureResourceId(request.SubnetId),
                            request.GroupId,
                            request.AutoApproval,
                            request.PrivateDnsZoneId.HasValue
                                ? new AzureResourceId(request.PrivateDnsZoneId.Value)
                                : null,
                            request.CustomNetworkInterfaceName);
                        var result = await mediator.Send(command);
                        return result.Match(
                            config => TypedResults.Ok(mapper.Map<PrivateEndpointConfigResponse>(config)),
                            errors => errors.Result());
                    })
                .WithName(PrivateEndpointRouteNames.UpdatePrivateEndpoint)
                .WithSummary("Update a private endpoint")
                .Produces<PrivateEndpointConfigResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapDelete("/{peId:guid}",
                    async ([FromRoute] Guid resourceId, [FromRoute] Guid peId, IMediator mediator) =>
                    {
                        var command = new RemovePrivateEndpointCommand(
                            new AzureResourceId(resourceId),
                            new PrivateEndpointConfigId(peId));
                        var result = await mediator.Send(command);
                        return result.Match(
                            _ => TypedResults.NoContent(),
                            errors => errors.Result());
                    })
                .WithName(PrivateEndpointRouteNames.RemovePrivateEndpoint)
                .WithSummary("Remove a private endpoint")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        });
    }
}
