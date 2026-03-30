using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerRegistries.Queries.CheckAcrPullAccess;

/// <summary>Query to check whether a compute resource has the "AcrPull" role on a Container Registry.</summary>
/// <param name="ResourceId">Identifier of the compute resource (WebApp, FunctionApp, or ContainerApp).</param>
/// <param name="ContainerRegistryId">Identifier of the Container Registry resource.</param>
public record CheckAcrPullAccessQuery(
    AzureResourceId ResourceId,
    AzureResourceId ContainerRegistryId
) : IQuery<CheckAcrPullAccessResult>;

/// <summary>Result indicating whether the compute resource has AcrPull access via a User Assigned Identity.</summary>
/// <param name="HasAccess">Whether the resource has the required role assignment via UAI.</param>
/// <param name="MissingRoleDefinitionId">The role definition ID that is missing, if access is not granted.</param>
/// <param name="MissingRoleName">The name of the missing role.</param>
/// <param name="AssignedUserAssignedIdentityId">ID of the UAI that has a role assignment on this container registry, if any.</param>
/// <param name="AssignedUserAssignedIdentityName">Name of the UAI that has a role assignment on this container registry, if any.</param>
/// <param name="HasUserAssignedIdentity">Whether the resource has any UAI-based role assignment at all.</param>
public sealed record CheckAcrPullAccessResult(
    bool HasAccess,
    string? MissingRoleDefinitionId,
    string? MissingRoleName,
    string? AssignedUserAssignedIdentityId,
    string? AssignedUserAssignedIdentityName,
    bool HasUserAssignedIdentity);
