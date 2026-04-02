using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics.Rules;

/// <summary>
/// Checks that resources referencing Key Vault secrets via app settings
/// have a <c>KeyVaultSecretsUser</c> role assignment targeting the referenced Key Vault.
/// </summary>
public sealed class KeyVaultAccessDiagnosticRule : IDiagnosticRule
{
    /// <summary>Stable diagnostic code emitted when a Key Vault access assignment is missing.</summary>
    private const string RuleCode = "KEY_VAULT_ACCESS_MISSING";

    /// <inheritdoc />
    public IReadOnlyList<ResourceDiagnosticItem> Evaluate(InfrastructureConfigReadModel config)
    {
        var diagnostics = new List<ResourceDiagnosticItem>();

        var kvReferencePairs = config.AppSettings
            .Where(s => s.IsKeyVaultReference && s.KeyVaultResourceId is not null)
            .Select(s => new
            {
                s.ResourceId,
                s.ResourceName,
                s.ResourceType,
                KeyVaultResourceId = s.KeyVaultResourceId!.Value,
                s.KeyVaultResourceName,
            })
            .Distinct()
            .GroupBy(s => (s.ResourceId, s.KeyVaultResourceId));

        foreach (var group in kvReferencePairs)
        {
            var (resourceId, keyVaultResourceId) = group.Key;
            var first = group.First();

            var hasKvAccess = config.RoleAssignments.Any(ra =>
                ra.SourceResourceId == resourceId
                && ra.TargetResourceId == keyVaultResourceId
                && ra.RoleDefinitionId.Equals(AzureRoleDefinitionCatalog.KeyVaultSecretsUser, StringComparison.OrdinalIgnoreCase));

            if (hasKvAccess)
                continue;

            var targetKvName = first.KeyVaultResourceName ?? keyVaultResourceId.ToString();

            diagnostics.Add(new ResourceDiagnosticItem(
                resourceId,
                first.ResourceName,
                first.ResourceType,
                DiagnosticSeverity.Warning,
                RuleCode,
                targetKvName));
        }

        return diagnostics;
    }
}
