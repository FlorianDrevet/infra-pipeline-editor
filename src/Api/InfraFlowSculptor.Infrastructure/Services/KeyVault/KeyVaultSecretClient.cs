using Azure.Security.KeyVault.Secrets;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Infrastructure.Services.KeyVault;

/// <summary>
/// Centralized Azure Key Vault secret client backed by a single <see cref="SecretClient"/>.
/// </summary>
public sealed class KeyVaultSecretClient(SecretClient client) : IKeyVaultSecretClient
{
    /// <inheritdoc />
    public async Task<ErrorOr<string>> GetSecretAsync(
        string secretName, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            return response.Value.Value;
        }
        catch (Exception)
        {
            return Errors.GitRepository.SecretRetrievalFailed();
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Success>> SetSecretAsync(
        string secretName, string value, CancellationToken cancellationToken = default)
    {
        try
        {
            await client.SetSecretAsync(secretName, value, cancellationToken);
            return Result.Success;
        }
        catch (Exception)
        {
            return Errors.GitRepository.SecretRetrievalFailed();
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> DeleteSecretAsync(
        string secretName, CancellationToken cancellationToken = default)
    {
        try
        {
            await client.StartDeleteSecretAsync(secretName, cancellationToken);
            return Result.Deleted;
        }
        catch (Exception)
        {
            return Errors.GitRepository.SecretRetrievalFailed();
        }
    }
}
