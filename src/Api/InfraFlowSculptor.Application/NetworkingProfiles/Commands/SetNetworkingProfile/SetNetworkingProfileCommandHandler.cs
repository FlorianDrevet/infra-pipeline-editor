using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.NetworkingProfileAggregate;
using InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.NetworkingProfiles.Commands.SetNetworkingProfile;

/// <summary>Handles creating or updating a networking profile.</summary>
public sealed class SetNetworkingProfileCommandHandler(
    INetworkingProfileRepository networkingProfileRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<SetNetworkingProfileCommand, NetworkingProfileResult>
{
    public async Task<ErrorOr<NetworkingProfileResult>> Handle(
        SetNetworkingProfileCommand request,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(request.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var vnetReference = BuildVnetReference(request);
        var dnsConfig = BuildDnsConfig(request);
        var mode = new NetworkingMode(request.Mode);

        var existing = await networkingProfileRepository.GetByInfraConfigIdAsync(
            request.InfraConfigId, cancellationToken);

        if (existing is not null)
        {
            existing.ChangeMode(mode);
            existing.UpdateVnetReference(vnetReference);
            existing.UpdateDnsConfig(dnsConfig);
            networkingProfileRepository.Update(existing);
            return MapToResult(existing);
        }

        var profile = NetworkingProfile.Create(
            request.InfraConfigId,
            mode,
            vnetReference,
            dnsConfig);

        networkingProfileRepository.Add(profile);
        return MapToResult(profile);
    }

    private static VnetReference BuildVnetReference(SetNetworkingProfileCommand request)
    {
        return request.VnetSourceType switch
        {
            VnetSource.SourceType.CreateNew => VnetReference.CreateNew(
                new CidrBlock(request.CreateNewAddressSpace!),
                new CidrBlock(request.CreateNewSubnetAddressPrefix!),
                request.PrivateEndpointsSubnetName),

            VnetSource.SourceType.UseExisting => VnetReference.UseExisting(
                request.ExistingVnetResourceId!,
                request.PrivateEndpointsSubnetName ?? "snet-pe"),

            VnetSource.SourceType.UseHubSpoke => VnetReference.UseHubSpoke(
                request.ExistingVnetResourceId!,
                request.PrivateEndpointsSubnetName ?? "snet-pe"),

            _ => throw new ArgumentOutOfRangeException(nameof(request), request.VnetSourceType, "Unsupported VNet source type.")
        };
    }

    private static DnsConfig BuildDnsConfig(SetNetworkingProfileCommand request)
    {
        return request.DnsMode switch
        {
            DnsMode.Mode.AutoManaged => DnsConfig.AutoManaged(),
            DnsMode.Mode.CentralizedHub => DnsConfig.CentralizedHub(
                request.DnsHubResourceGroupId!,
                request.DnsHubSubscriptionId!),
            DnsMode.Mode.Custom => DnsConfig.Custom(),
            _ => throw new ArgumentOutOfRangeException(nameof(request), request.DnsMode, "Unsupported DNS mode.")
        };
    }

    private static NetworkingProfileResult MapToResult(NetworkingProfile profile)
    {
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
