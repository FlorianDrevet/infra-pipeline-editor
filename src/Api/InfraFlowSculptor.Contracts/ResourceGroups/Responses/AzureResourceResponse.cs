namespace InfraFlowSculptor.Contracts.ResourceGroups.Responses;

/// <summary>Lightweight representation of an Azure resource inside a Resource Group.</summary>
/// <param name="Id">Unique identifier of the Azure resource.</param>
/// <param name="ResourceType">Type of the Azure resource (e.g. "KeyVault", "StorageAccount", "RedisCache").</param>
/// <param name="Name">Display name of the resource.</param>
/// <param name="Location">Azure region where the resource is deployed.</param>
/// <param name="ParentResourceId">Optional identifier of the parent resource (e.g. AppServicePlan for a WebApp).</param>
/// <param name="ConfiguredEnvironments">List of environment names that have typed per-environment settings configured for this resource.</param>
public record AzureResourceResponse(
    Guid Id,
    string ResourceType,
    string Name,
    string Location,
    Guid? ParentResourceId = null,
    IReadOnlyList<string>? ConfiguredEnvironments = null);
