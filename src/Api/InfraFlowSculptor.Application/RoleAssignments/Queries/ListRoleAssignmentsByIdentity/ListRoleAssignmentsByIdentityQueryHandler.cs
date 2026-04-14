using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.GenerationCore;
using MediatR;

namespace InfraFlowSculptor.Application.RoleAssignments.Queries.ListRoleAssignmentsByIdentity;

/// <summary>
/// Handles the <see cref="ListRoleAssignmentsByIdentityQuery"/> by loading all role assignments
/// that reference the given User-Assigned Identity, enriched with source/target names and types.
/// </summary>
public sealed class ListRoleAssignmentsByIdentityQueryHandler(
    IAzureResourceRepository azureResourceRepository,
    IUserAssignedIdentityRepository userAssignedIdentityRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IQueryHandler<ListRoleAssignmentsByIdentityQuery, List<IdentityRoleAssignmentResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<List<IdentityRoleAssignmentResult>>> Handle(
        ListRoleAssignmentsByIdentityQuery request,
        CancellationToken cancellationToken)
    {
        // Verify the identity exists
        var identity = await userAssignedIdentityRepository.GetByIdAsync(request.IdentityId, cancellationToken);
        if (identity is null)
            return Errors.UserAssignedIdentity.NotFoundError(request.IdentityId);

        // Verify read access through resource group → infra config
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(identity.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.UserAssignedIdentity.NotFoundError(request.IdentityId);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.UserAssignedIdentity.NotFoundError(request.IdentityId);

        // Query all role assignments that reference this identity
        var roleAssignments = await azureResourceRepository.GetRoleAssignmentsByIdentityIdAsync(
            request.IdentityId, cancellationToken);

        if (roleAssignments.Count == 0)
            return new List<IdentityRoleAssignmentResult>();

        // Batch-load all referenced resources (source + target) for name/type resolution
        var referencedIds = roleAssignments
            .SelectMany(r => new[] { r.SourceResourceId, r.TargetResourceId })
            .Distinct()
            .ToList();

        var resourceLookup = new Dictionary<AzureResourceId, AzureResource>();
        foreach (var id in referencedIds)
        {
            var resource = await azureResourceRepository.GetByIdAsync(id, cancellationToken);
            if (resource is not null)
                resourceLookup[id] = resource;
        }

        // Build all role definitions into a lookup for name resolution
        var allRoleDefs = BuildRoleDefinitionLookup();

        var results = roleAssignments.Select(ra =>
        {
            var sourceName = resourceLookup.TryGetValue(ra.SourceResourceId, out var src)
                ? src.Name.Value : ra.SourceResourceId.Value.ToString();
            var sourceType = src is not null ? GetResourceTypeName(src) : "Unknown";

            var targetName = resourceLookup.TryGetValue(ra.TargetResourceId, out var tgt)
                ? tgt.Name.Value : ra.TargetResourceId.Value.ToString();
            var targetType = tgt is not null ? GetResourceTypeName(tgt) : "Unknown";

            var roleName = allRoleDefs.TryGetValue(ra.RoleDefinitionId, out var rd)
                ? rd : ra.RoleDefinitionId;

            return new IdentityRoleAssignmentResult(
                ra.Id,
                ra.SourceResourceId,
                sourceName,
                sourceType,
                ra.TargetResourceId,
                targetName,
                targetType,
                ra.RoleDefinitionId,
                roleName);
        }).ToList();

        return results;
    }

    /// <summary>Resolves the simple type name from the concrete <see cref="AzureResource"/> derived type.</summary>
    private static string GetResourceTypeName(AzureResource resource) =>
        resource.GetType().Name;

    /// <summary>Builds a flat dictionary of role definition ID → role name from the catalog.</summary>
    private static Dictionary<string, string> BuildRoleDefinitionLookup()
    {
        var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var resourceTypes = AzureResourceTypes.All;

        foreach (var rt in resourceTypes)
        {
            foreach (var role in AzureRoleDefinitionCatalog.GetForResourceType(rt))
            {
                lookup.TryAdd(role.Id, role.Name);
            }
        }

        return lookup;
    }
}
