using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.RoleAssignments.Queries.ListAvailableRoleDefinitions;

public class ListAvailableRoleDefinitionsQueryHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<ListAvailableRoleDefinitionsQuery, ErrorOr<List<AzureRoleDefinitionResult>>>
{
    public async Task<ErrorOr<List<AzureRoleDefinitionResult>>> Handle(
        ListAvailableRoleDefinitionsQuery request,
        CancellationToken cancellationToken)
    {
        var resource = await azureResourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.RoleAssignment.SourceResourceNotFound(request.ResourceId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            resource.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(resource.ResourceGroupId);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var resourceType = resource.GetType().Name;
        var roles = AzureRoleDefinitionCatalog.GetForResourceType(resourceType);

        return roles
            .Select(r => new AzureRoleDefinitionResult(r.Id, r.Name, r.Description, r.DocumentationUrl))
            .ToList();
    }
}
