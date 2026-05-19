using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerRegistries.Queries.CheckAcrPullAccess;

/// <summary>Query to check whether a compute resource has the "AcrPull" role on a Container Registry.</summary>
/// <param name="ResourceId">Identifier of the compute resource (WebApp, FunctionApp, or ContainerApp).</param>
/// <param name="ContainerRegistryId">Identifier of the Container Registry resource.</param>
/// <param name="AcrAuthMode">Authentication mode used by the compute resource to access the container registry.</param>
/// <param name="AcrPullIdentityId">
/// Identifier of the specific User Assigned Identity used for ACR pull operations.
/// If specified, the check validates this specific identity has AcrPull role.
/// If null, falls back to legacy behavior (finds any UAI with AcrPull role).
/// </param>
public record CheckAcrPullAccessQuery(
    AzureResourceId ResourceId,
    AzureResourceId ContainerRegistryId,
    string? AcrAuthMode = null,
    AzureResourceId? AcrPullIdentityId = null
) : IQuery<CheckAcrPullAccessResult>;
