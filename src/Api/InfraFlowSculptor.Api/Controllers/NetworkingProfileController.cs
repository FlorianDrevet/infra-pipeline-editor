using InfraFlowSculptor.Api.Controllers.Constants;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.NetworkingProfiles.Commands.RemovePrivateEndpointConfig;
using InfraFlowSculptor.Application.NetworkingProfiles.Commands.SetPrivateEndpointConfig;
using InfraFlowSculptor.Application.NetworkingProfiles.Commands.ToggleResourcePrivatization;
using InfraFlowSculptor.Contracts.NetworkingProfiles.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>
/// Private endpoint configuration endpoints (V3 per-resource PE).
/// </summary>
public static class NetworkingProfileController
{
    public static IApplicationBuilder UseNetworkingProfileController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            // Resource privatization toggle
            var privatization = endpoints.MapGroup(Routes.ResourcePrivatization)
                .WithTags("Private Endpoint");

            privatization.MapPut("",
                    async ([FromRoute] Guid infraConfigId, [FromRoute] Guid resourceId,
                        [FromBody] ToggleResourcePrivatizationRequest request,
                        IMediator mediator) =>
                    {
                        var command = new ToggleResourcePrivatizationCommand(
                            new InfrastructureConfigId(infraConfigId),
                            new AzureResourceId(resourceId),
                            request.IsPrivatized
                        );

                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => TypedResults.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName(NetworkingProfileRouteNames.ToggleResourcePrivatization)
                .WithSummary("Toggle resource privatization")
                .WithDescription("Marks a resource as privatized or deprivatized.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);

            // Resource private endpoint configuration (V3)
            var privateEndpoint = endpoints.MapGroup(Routes.ResourcePrivateEndpoint)
                .WithTags("Private Endpoint");

            privateEndpoint.MapPut("",
                    async ([FromRoute] Guid infraConfigId, [FromRoute] Guid resourceId,
                        [FromBody] SetPrivateEndpointConfigRequest request,
                        IMediator mediator) =>
                    {
                        var command = new SetPrivateEndpointConfigCommand(
                            new InfrastructureConfigId(infraConfigId),
                            new AzureResourceId(resourceId),
                            new AzureResourceId(Guid.Parse(request.VirtualNetworkId)),
                            request.SubnetName,
                            request.DnsMode,
                            request.DnsHubResourceGroupId,
                            request.DnsHubSubscriptionId
                        );

                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => TypedResults.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName(NetworkingProfileRouteNames.SetPrivateEndpointConfig)
                .WithSummary("Configure resource private endpoint")
                .WithDescription("Sets the private endpoint configuration (VNet, subnet, DNS mode) on an Azure resource.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);

            privateEndpoint.MapDelete("",
                    async ([FromRoute] Guid infraConfigId, [FromRoute] Guid resourceId,
                        IMediator mediator) =>
                    {
                        var command = new RemovePrivateEndpointConfigCommand(
                            new InfrastructureConfigId(infraConfigId),
                            new AzureResourceId(resourceId)
                        );

                        var result = await mediator.Send(command);

                        return result.Match(
                            _ => TypedResults.NoContent(),
                            errors => errors.Result()
                        );
                    })
                .WithName(NetworkingProfileRouteNames.RemovePrivateEndpointConfig)
                .WithSummary("Remove resource private endpoint")
                .WithDescription("Removes the private endpoint configuration from an Azure resource, reverting to public access.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);
        });
    }
}
