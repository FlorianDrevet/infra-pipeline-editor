using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.NetworkingProfiles.Queries.GetNetworkingProfile;

/// <summary>Handles retrieving the networking profile for an infra config.</summary>
public sealed class GetNetworkingProfileQueryHandler(
    INetworkingProfileRepository networkingProfileRepository,
    IInfraConfigAccessService accessService)
    : IQueryHandler<GetNetworkingProfileQuery, NetworkingProfileResult>
{
    public async Task<ErrorOr<NetworkingProfileResult>> Handle(
        GetNetworkingProfileQuery request,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyReadAccessAsync(request.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var profile = await networkingProfileRepository.GetByInfraConfigIdWithOverridesAsync(
            request.InfraConfigId, cancellationToken);

        if (profile is null)
            return Errors.NetworkingProfile.NotFoundForInfraConfig(request.InfraConfigId);

        return new NetworkingProfileResult(
            Id: profile.Id.Value.ToString(),
            InfraConfigId: profile.InfraConfigId.Value.ToString(),
            Mode: profile.Mode.Value.ToString(),
            VnetSourceType: profile.VnetReference.Source.Value.ToString(),
            ExistingVnetResourceId: profile.VnetReference.ExistingVnetResourceId,
            CreateNewAddressSpace: profile.VnetReference.CreateNewAddressSpace?.Value,
            CreateNewSubnetAddressPrefix: profile.VnetReference.PrivateEndpointsSubnetAddressPrefix?.Value,
            PrivateEndpointsSubnetName: profile.VnetReference.PrivateEndpointsSubnetName,
            DnsMode: profile.DnsConfig.Mode.Value.ToString(),
            DnsHubResourceGroupId: profile.DnsConfig.HubResourceGroupId,
            DnsHubSubscriptionId: profile.DnsConfig.HubSubscriptionId);
    }
}
