using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.RoleAssignments.Queries.AnalyzeRoleAssignmentImpact;

/// <summary>Handles the <see cref="AnalyzeRoleAssignmentImpactQuery"/> request.</summary>
public sealed class AnalyzeRoleAssignmentImpactQueryHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IRoleAssignmentImpactAnalyzer impactAnalyzer)
    : IQueryHandler<AnalyzeRoleAssignmentImpactQuery, RoleAssignmentImpactResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<RoleAssignmentImpactResult>> Handle(
        AnalyzeRoleAssignmentImpactQuery request,
        CancellationToken cancellationToken)
    {
        var sourceResource = await azureResourceRepository.GetByIdWithRoleAssignmentsAndAppSettingsAsync(
            request.SourceResourceId, cancellationToken);

        if (sourceResource is null)
            return Errors.RoleAssignment.SourceResourceNotFound(request.SourceResourceId);

        var assignment = sourceResource.RoleAssignments.FirstOrDefault(r => r.Id == request.RoleAssignmentId);

        if (assignment is null)
            return Errors.RoleAssignment.NotFound(request.RoleAssignmentId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            sourceResource.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(sourceResource.ResourceGroupId);

        var authResult = await accessService.VerifyReadAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var impacts = await impactAnalyzer.AnalyzeAsync(sourceResource, assignment, cancellationToken);

        return new RoleAssignmentImpactResult(
            HasImpact: impacts.Count > 0,
            Impacts: impacts);
    }
}
