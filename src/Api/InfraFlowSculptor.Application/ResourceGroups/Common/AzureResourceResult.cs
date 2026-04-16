using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.ResourceGroups.Common;

/// <summary>Lightweight representation of an Azure resource inside a Resource Group.</summary>
/// <param name="Id">Unique identifier of the Azure resource.</param>
/// <param name="ResourceType">Type discriminator (e.g. "KeyVault", "WebApp").</param>
/// <param name="Name">Display name of the resource.</param>
/// <param name="Location">Azure region.</param>
/// <param name="ParentResourceId">Optional identifier of the parent resource (e.g. AppServicePlan for a WebApp).</param>
/// <param name="ConfiguredEnvironments">List of environment names that have typed per-environment settings configured.</param>
public record AzureResourceResult(
    AzureResourceId Id,
    string ResourceType,
    Name Name,
    Location Location,
    AzureResourceId? ParentResourceId = null,
    IReadOnlyCollection<string>? ConfiguredEnvironments = null);
