using InfraFlowSculptor.Contracts.AppServicePlans.Requests;

namespace InfraFlowSculptor.Contracts.AppServicePlans.Responses;

/// <summary>Represents an Azure App Service Plan resource.</summary>
/// <param name="Id">Unique identifier of the App Service Plan.</param>
/// <param name="ResourceGroupId">Identifier of the parent Resource Group.</param>
/// <param name="Name">Display name of the App Service Plan.</param>
/// <param name="Location">Azure region where the App Service Plan is deployed.</param>
/// <param name="OsType">Operating system type (Windows or Linux).</param>
/// <param name="EnvironmentSettings">Per-environment typed configuration overrides.</param>
public record AppServicePlanResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    string OsType,
    IReadOnlyList<AppServicePlanEnvironmentConfigResponse> EnvironmentSettings,

    bool IsExisting = false

);
