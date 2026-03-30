using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.ContainerRegistries.Queries.CheckAcrPullAccess;

/// <summary>Handles the <see cref="CheckAcrPullAccessQuery"/> request.</summary>
public sealed class CheckAcrPullAccessQueryHandler(
    IAzureResourceRepository azureResourceRepository)
    : IQueryHandler<CheckAcrPullAccessQuery, CheckAcrPullAccessResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<CheckAcrPullAccessResult>> Handle(
        CheckAcrPullAccessQuery request,
        CancellationToken cancellationToken)
    {
        var resource = await azureResourceRepository.GetByIdWithRoleAssignmentsAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.ContainerRegistry.NotFoundError(request.ResourceId);

        var hasAccess = resource.RoleAssignments.Any(ra =>
            ra.TargetResourceId == request.ContainerRegistryId &&
            ra.RoleDefinitionId == AzureRoleDefinitionCatalog.AcrPull);

        return new CheckAcrPullAccessResult(
            HasAccess: hasAccess,
            MissingRoleDefinitionId: hasAccess ? null : AzureRoleDefinitionCatalog.AcrPull,
            MissingRoleName: hasAccess ? null : "AcrPull");
    }
}
