using InfraFlowSculptor.Contracts.AppConfigurations.Requests;

namespace InfraFlowSculptor.Contracts.AppConfigurations.Responses;

/// <summary>Represents an Azure App Configuration resource.</summary>
/// <param name="Id">Unique identifier of the App Configuration.</param>
/// <param name="ResourceGroupId">Identifier of the parent Resource Group.</param>
/// <param name="Name">Display name of the App Configuration.</param>
/// <param name="Location">Azure region where the App Configuration is deployed.</param>
/// <param name="EnvironmentSettings">Per-environment typed configuration overrides.</param>
public record AppConfigurationResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    IReadOnlyList<AppConfigurationEnvironmentConfigResponse> EnvironmentSettings,

    bool IsExisting = false

);
