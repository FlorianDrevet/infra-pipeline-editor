using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.OwnedEntities;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.NetworkingProfiles.Commands.SetPrivateEndpointConfig;

/// <summary>
/// Configures a resource-level private endpoint.
/// Validates that the selected VNet belongs to the same project (cross-config allowed)
/// and that the requested subnet exists on the VNet.
/// </summary>
public sealed class SetPrivateEndpointConfigCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IVirtualNetworkRepository virtualNetworkRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<SetPrivateEndpointConfigCommand, Success>
{
    /// <inheritdoc/>
    public async Task<ErrorOr<Success>> Handle(
        SetPrivateEndpointConfigCommand request,
        CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(request.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var infraConfig = authResult.Value;

        // Load the target resource.
        var resource = await azureResourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);
        if (resource is null)
            return Error.NotFound("AzureResource.NotFound",
                $"Resource '{request.ResourceId}' not found.");

        // Load the VNet once — GetByIdReadOnlyAsync includes Subnets (via WithSubResources).
        var vnet = await virtualNetworkRepository.GetByIdReadOnlyAsync(request.VirtualNetworkId, cancellationToken);
        if (vnet is null)
            return Error.NotFound("VirtualNetwork.NotFound",
                $"Virtual network '{request.VirtualNetworkId}' not found.");

        // Validate the VNet belongs to the same project (cross-config is allowed).
        // Use vnet.ResourceGroupId (FK always available) to load the resource group by PK —
        // more efficient than GetByContainedResourceIdAsync which does a full-table join.
        var vnetRg = await resourceGroupRepository.GetByIdReadOnlyAsync(vnet.ResourceGroupId, cancellationToken);
        if (vnetRg is null)
            return Error.NotFound("VirtualNetwork.NotFound",
                $"Virtual network '{request.VirtualNetworkId}' not found.");

        var vnetConfig = await infraConfigRepository.GetByIdAsync(vnetRg.InfraConfigId, cancellationToken);
        if (vnetConfig is null || vnetConfig.ProjectId != infraConfig.ProjectId)
            return Error.Validation("VirtualNetwork.NotInSameProject",
                "The selected virtual network does not belong to the same project.");

        // Validate the subnet exists on the VNet (Subnets already loaded via WithSubResources).
        var subnetExists = vnet.Subnets.Any(s => s.Name.Value == request.SubnetName);
        if (!subnetExists)
            return Error.Validation("Subnet.NotFound",
                $"Subnet '{request.SubnetName}' does not exist on the selected virtual network.");

        // Build the domain configuration.
        if (!Enum.TryParse<PrivateEndpointDnsMode.Mode>(request.DnsMode, ignoreCase: true, out var dnsMode))
            return Error.Validation("PrivateEndpointDnsMode.Invalid",
                $"'{request.DnsMode}' is not a valid DNS mode.");

        var configuration = dnsMode switch
        {
            PrivateEndpointDnsMode.Mode.AutoManaged =>
                PrivateEndpointConfiguration.AutoManaged(request.VirtualNetworkId, new Name(request.SubnetName)),
            PrivateEndpointDnsMode.Mode.ExistingHub =>
                PrivateEndpointConfiguration.ExistingHub(
                    request.VirtualNetworkId,
                    new Name(request.SubnetName),
                    request.DnsHubResourceGroupId!,
                    request.DnsHubSubscriptionId!),
            PrivateEndpointDnsMode.Mode.Disabled =>
                PrivateEndpointConfiguration.Disabled(request.VirtualNetworkId, new Name(request.SubnetName)),
            _ => throw new InvalidOperationException($"Unsupported DNS mode: {dnsMode}")
        };

        resource.ConfigurePrivateEndpoint(configuration);
        azureResourceRepository.Update(resource);

        return Result.Success;
    }
}
