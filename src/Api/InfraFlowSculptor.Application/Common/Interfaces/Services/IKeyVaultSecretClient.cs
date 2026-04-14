using ErrorOr;

namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

/// <summary>
/// Abstracts secret retrieval and storage in the application's centralized Azure Key Vault.
/// </summary>
public interface IKeyVaultSecretClient
{
    /// <summary>
    /// Retrieves a secret value from the centralized Key Vault.
    /// </summary>
    /// <param name="secretName">The name of the secret.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The secret value, or an error if retrieval failed.</returns>
    Task<ErrorOr<string>> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores or updates a secret in the centralized Key Vault.
    /// </summary>
    /// <param name="secretName">The name of the secret.</param>
    /// <param name="value">The secret value to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success, or an error if storage failed.</returns>
    Task<ErrorOr<Success>> SetSecretAsync(string secretName, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a secret from the centralized Key Vault.
    /// </summary>
    /// <param name="secretName">The name of the secret to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success, or an error if deletion failed.</returns>
    Task<ErrorOr<Deleted>> DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default);
}
