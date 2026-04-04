using System.Text;

namespace InfraFlowSculptor.BicepGeneration.Assemblers;

/// <summary>
/// Generates the Key Vault batch secrets module template (<c>kvSecrets.module.bicep</c>).
/// </summary>
internal static class KvSecretsModuleAssembler
{
    /// <summary>
    /// Produces the Key Vault batch secrets module template content.
    /// Accepts a list of secrets and outputs a dictionary keyed by secret name → secretUri.
    /// </summary>
    internal static string Generate()
    {
        var sb = new StringBuilder();
        sb.AppendLine("// ──────────────────────────────────────────────────────────────────────");
        sb.AppendLine("// Key Vault Secrets Module — stores multiple secrets in a Key Vault");
        sb.AppendLine("// ──────────────────────────────────────────────────────────────────────");
        sb.AppendLine();
        sb.AppendLine("@description('Name of the Key Vault')");
        sb.AppendLine("param keyVaultName string");
        sb.AppendLine();
        sb.AppendLine("@secure()");
        sb.AppendLine("@description('List of secrets to store: { name: string, value: string }[]')");
        sb.AppendLine("param secrets array");
        sb.AppendLine();
        sb.AppendLine("resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {");
        sb.AppendLine("  name: keyVaultName");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("resource kvSecrets 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = [for secret in secrets: {");
        sb.AppendLine("  parent: keyVault");
        sb.AppendLine("  name: secret.name");
        sb.AppendLine("  properties: {");
        sb.AppendLine("    value: secret.value");
        sb.AppendLine("  }");
        sb.AppendLine("}]");
        sb.AppendLine();
        sb.AppendLine("@description('Dictionary of secret URIs keyed by secret name')");
        sb.AppendLine("output secretUris object = toObject(range(0, length(secrets)), i => secrets[i].name, i => kvSecrets[i].properties.secretUri)");
        return sb.ToString();
    }
}
