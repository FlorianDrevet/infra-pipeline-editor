namespace InfraFlowSculptor.Application.FrontDoors.Common;

/// <summary>Carries per-environment Front Door configuration data within CQRS commands and results.</summary>
/// <param name="EnvironmentName">Name of the environment.</param>
/// <param name="Sku">Front Door pricing tier.</param>
public record FrontDoorEnvironmentConfigData(
    string EnvironmentName,
    string Sku);
