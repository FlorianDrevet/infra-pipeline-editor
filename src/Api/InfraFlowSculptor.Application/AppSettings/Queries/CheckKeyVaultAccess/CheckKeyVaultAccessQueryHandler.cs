using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.AppSettings.Queries.CheckKeyVaultAccess;

/// <summary>Handles the <see cref="CheckKeyVaultAccessQuery"/> request.</summary>
public sealed class CheckKeyVaultAccessQueryHandler(
    IAzureResourceRepository azureResourceRepository)
    : IRequestHandler<CheckKeyVaultAccessQuery, ErrorOr<CheckKeyVaultAccessResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<CheckKeyVaultAccessResult>> Handle(
        CheckKeyVaultAccessQuery request,
        CancellationToken cancellationToken)
    {
        var resource = await azureResourceRepository.GetByIdWithRoleAssignmentsAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.AppSetting.SourceResourceNotFound(request.ResourceId);

        var hasAccess = resource.RoleAssignments.Any(ra =>
            ra.TargetResourceId == request.KeyVaultResourceId &&
            ra.RoleDefinitionId == AzureRoleDefinitionCatalog.KeyVaultSecretsUser);

        return new CheckKeyVaultAccessResult(
            HasAccess: hasAccess,
            MissingRoleDefinitionId: hasAccess ? null : AzureRoleDefinitionCatalog.KeyVaultSecretsUser,
            MissingRoleName: hasAccess ? null : "Key Vault Secrets User");
    }
}
