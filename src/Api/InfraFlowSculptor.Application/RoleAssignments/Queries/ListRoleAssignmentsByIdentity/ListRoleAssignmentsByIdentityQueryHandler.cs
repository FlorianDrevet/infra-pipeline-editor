using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
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
        var notFoundError = Errors.UserAssignedIdentity.NotFoundError(request.IdentityId);
        var identity = await userAssignedIdentityRepository.GetByIdAsync(request.IdentityId, cancellationToken);
        if (identity is null)
            return notFoundError;

        if (!await HasReadAccessToIdentityAsync(identity.ResourceGroupId, cancellationToken))
            return notFoundError;

        var roleAssignments = await azureResourceRepository.GetRoleAssignmentsByIdentityIdAsync(
            request.IdentityId, cancellationToken);

        if (roleAssignments.Count == 0)
            return new List<IdentityRoleAssignmentResult>();

        var resourceLookup = await LoadReferencedResourcesAsync(roleAssignments, cancellationToken);
        var allRoleDefinitions = BuildRoleDefinitionLookup();

        return MapRoleAssignments(roleAssignments, resourceLookup, allRoleDefinitions);
    }

    private async Task<bool> HasReadAccessToIdentityAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(resourceGroupId, cancellationToken);
        if (resourceGroup is null)
        {
            return false;
        }

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        return !authResult.IsError;
    }

    private async Task<Dictionary<AzureResourceId, AzureResource>> LoadReferencedResourcesAsync(
        IReadOnlyCollection<Domain.Common.BaseModels.Entites.RoleAssignment> roleAssignments,
        CancellationToken cancellationToken)
    {
        var referencedIds = roleAssignments
            .SelectMany(roleAssignment => new[] { roleAssignment.SourceResourceId, roleAssignment.TargetResourceId })
            .Distinct()
            .ToList();

        var resourceLookup = new Dictionary<AzureResourceId, AzureResource>();
        foreach (var referencedId in referencedIds)
        {
            var resource = await azureResourceRepository.GetByIdAsync(referencedId, cancellationToken);
            if (resource is not null)
            {
                resourceLookup[referencedId] = resource;
            }
        }

        return resourceLookup;
    }

    private static List<IdentityRoleAssignmentResult> MapRoleAssignments(
        IReadOnlyCollection<Domain.Common.BaseModels.Entites.RoleAssignment> roleAssignments,
        IReadOnlyDictionary<AzureResourceId, AzureResource> resourceLookup,
        IReadOnlyDictionary<string, string> roleDefinitions)
    {
        return roleAssignments
            .Select(roleAssignment => MapRoleAssignment(roleAssignment, resourceLookup, roleDefinitions))
            .ToList();
    }

    private static IdentityRoleAssignmentResult MapRoleAssignment(
        Domain.Common.BaseModels.Entites.RoleAssignment roleAssignment,
        IReadOnlyDictionary<AzureResourceId, AzureResource> resourceLookup,
        IReadOnlyDictionary<string, string> roleDefinitions)
    {
        var sourceDisplay = ResolveResourceDisplay(roleAssignment.SourceResourceId, resourceLookup);
        var targetDisplay = ResolveResourceDisplay(roleAssignment.TargetResourceId, resourceLookup);
        var roleName = roleDefinitions.TryGetValue(roleAssignment.RoleDefinitionId, out var resolvedRoleName)
            ? resolvedRoleName
            : roleAssignment.RoleDefinitionId;

        return new IdentityRoleAssignmentResult(
            roleAssignment.Id,
            roleAssignment.SourceResourceId,
            sourceDisplay.Name,
            sourceDisplay.Type,
            roleAssignment.TargetResourceId,
            targetDisplay.Name,
            targetDisplay.Type,
            roleAssignment.RoleDefinitionId,
            roleName);
    }

    private static (string Name, string Type) ResolveResourceDisplay(
        AzureResourceId resourceId,
        IReadOnlyDictionary<AzureResourceId, AzureResource> resourceLookup)
    {
        return resourceLookup.TryGetValue(resourceId, out var resource)
            ? (resource.Name.Value, GetResourceTypeName(resource))
            : (resourceId.Value.ToString(), "Unknown");
    }

    /// <summary>Resolves the simple type name from the concrete <see cref="AzureResource"/> derived type.</summary>
    private static string GetResourceTypeName(AzureResource resource) =>
        resource.GetType().Name;

    /// <summary>Builds a flat dictionary of role definition ID â†’ role name from the catalog.</summary>
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
