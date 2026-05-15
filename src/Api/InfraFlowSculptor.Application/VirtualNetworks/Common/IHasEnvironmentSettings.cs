namespace InfraFlowSculptor.Application.VirtualNetworks.Common;

/// <summary>Marker interface for commands carrying VNet environment settings.</summary>
internal interface IHasEnvironmentSettings
{
    /// <summary>Gets the environment settings collection.</summary>
    IReadOnlyList<VirtualNetworkEnvironmentConfigData>? EnvironmentSettings { get; }
}
