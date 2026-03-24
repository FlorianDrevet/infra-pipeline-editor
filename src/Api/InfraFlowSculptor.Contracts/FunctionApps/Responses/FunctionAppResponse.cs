using InfraFlowSculptor.Contracts.FunctionApps.Requests;

namespace InfraFlowSculptor.Contracts.FunctionApps.Responses;

/// <summary>Represents an Azure Function App resource.</summary>
/// <param name="Id">Unique identifier of the Function App.</param>
/// <param name="ResourceGroupId">Identifier of the parent Resource Group.</param>
/// <param name="Name">Display name of the Function App.</param>
/// <param name="Location">Azure region where the Function App is deployed.</param>
/// <param name="AppServicePlanId">Identifier of the hosting App Service Plan.</param>
/// <param name="RuntimeStack">Runtime stack (e.g., "DotNet", "Node").</param>
/// <param name="RuntimeVersion">Runtime version (e.g., "8.0", "20").</param>
/// <param name="HttpsOnly">Whether the app requires HTTPS only.</param>
/// <param name="EnvironmentSettings">Per-environment typed configuration overrides.</param>
public record FunctionAppResponse(
    Guid Id,
    Guid ResourceGroupId,
    string Name,
    string Location,
    Guid AppServicePlanId,
    string RuntimeStack,
    string RuntimeVersion,
    bool HttpsOnly,
    IReadOnlyList<FunctionAppEnvironmentConfigResponse> EnvironmentSettings
);
