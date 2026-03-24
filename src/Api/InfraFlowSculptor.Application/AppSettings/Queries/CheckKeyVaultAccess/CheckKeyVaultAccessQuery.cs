using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.AppSettings.Queries.CheckKeyVaultAccess;

/// <summary>Query to check whether a compute resource has the "Key Vault Secrets User" role on a Key Vault.</summary>
/// <param name="ResourceId">Identifier of the compute resource.</param>
/// <param name="KeyVaultResourceId">Identifier of the Key Vault resource.</param>
public record CheckKeyVaultAccessQuery(
    AzureResourceId ResourceId,
    AzureResourceId KeyVaultResourceId
) : IRequest<ErrorOr<CheckKeyVaultAccessResult>>;

/// <summary>Result indicating whether the compute resource has Key Vault access.</summary>
/// <param name="HasAccess">Whether the resource has the required role assignment.</param>
/// <param name="MissingRoleDefinitionId">The role definition ID that is missing, if access is not granted.</param>
/// <param name="MissingRoleName">The name of the missing role.</param>
public sealed record CheckKeyVaultAccessResult(
    bool HasAccess,
    string? MissingRoleDefinitionId,
    string? MissingRoleName);
