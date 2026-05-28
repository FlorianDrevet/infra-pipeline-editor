using InfraFlowSculptor.Api.Controllers.Constants;
using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.NetworkingProfiles.Commands.SetNetworkingProfile;
using InfraFlowSculptor.Application.NetworkingProfiles.Commands.ToggleResourcePrivatization;
using InfraFlowSculptor.Application.NetworkingProfiles.Queries.GetNetworkingProfile;
using InfraFlowSculptor.Contracts.NetworkingProfiles.Requests;
using InfraFlowSculptor.Contracts.NetworkingProfiles.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

public static class NetworkingProfileController
{
    public static IApplicationBuilder UseNetworkingProfileController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup(Routes.NetworkingProfile)
                .WithTags("Networking Profile");

            group.MapGet("",
                    async ([FromRoute] Guid infraConfigId, IMediator mediator, IMapper mapper) =>
                    {
                        var query = new GetNetworkingProfileQuery(new InfrastructureConfigId(infraConfigId));
                        var result = await mediator.Send(query);

                        return result.Match(
                            profile => TypedResults.Ok(mapper.Map<NetworkingProfileResponse>(profile)),
                            errors => errors.Result()
                        );
                    })
                .WithName(NetworkingProfileRouteNames.GetNetworkingProfile)
                .WithSummary("Get networking profile")
                .WithDescription("Returns the networking profile for the specified infrastructure configuration.")
                .Produces<NetworkingProfileResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPut("",
                    async ([FromRoute] Guid infraConfigId, [FromBody] SetNetworkingProfileRequest request,
                        IMediator mediator, IMapper mapper) =>
                    {
                        var command = new SetNetworkingProfileCommand(
                            new InfrastructureConfigId(infraConfigId),
                            Enum.Parse<NetworkingMode.Mode>(request.Mode, ignoreCase: true),
                            Enum.Parse<VnetSource.SourceType>(request.VnetSourceType, ignoreCase: true),
                            request.ExistingVnetResourceId,
                            request.CreateNewAddressSpace,
                            request.CreateNewSubnetAddressPrefix,
                            request.PrivateEndpointsSubnetName,
                            Enum.Parse<DnsMode.Mode>(request.DnsMode, ignoreCase: true),
                            request.DnsHubResourceGroupId,
                            request.DnsHubSubscriptionId
                        );

                        var result = await mediator.Send(command);

                        return result.Match(
                            profile => TypedResults.Ok(mapper.Map<NetworkingProfileResponse>(profile)),
                            errors => errors.Result()
                        );
                    })
                .WithName(NetworkingProfileRouteNames.SetNetworkingProfile)
                .WithSummary("Create or update networking profile")
                .WithDescription("Creates or updates the networking profile (VNet, DNS, mode) for the specified infrastructure configuration.")
                .Produces<NetworkingProfileResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status400BadRequest);

            // Resource privatization endpoint
            var privatization = endpoints.MapGroup(Routes.ResourcePrivatization)
                .WithTags("Networking Profile");

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
                .WithDescription("Marks a resource as privatized or deprivatized within the networking profile scope.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);
        });
    }
}
