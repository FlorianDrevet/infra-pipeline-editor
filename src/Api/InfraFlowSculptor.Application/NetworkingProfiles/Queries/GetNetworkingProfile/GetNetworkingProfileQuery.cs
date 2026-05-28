using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.NetworkingProfiles.Queries.GetNetworkingProfile;

/// <summary>Retrieves the networking profile for an infrastructure configuration.</summary>
public record GetNetworkingProfileQuery(
    InfrastructureConfigId InfraConfigId
) : IQuery<NetworkingProfileResult>;
