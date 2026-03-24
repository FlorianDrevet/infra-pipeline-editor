using ErrorOr;

namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

/// <summary>
/// Abstracts secret retrieval from Azure Key Vault.
/// </summary>
public interface IKeyVaultSecretClient
{
    /// <summary>
    /// Retrieves a secret value from the specified Key Vault.
    /// </summary>
    /// <param name="vaultUrl">The Key Vault URL (e.g. https://my-vault.vault.azure.net).</param>
    /// <param name="secretName">The name of the secret.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The secret value, or an error if retrieval failed.</returns>
    Task<ErrorOr<string>> GetSecretAsync(string vaultUrl, string secretName, CancellationToken cancellationToken = default);
}
