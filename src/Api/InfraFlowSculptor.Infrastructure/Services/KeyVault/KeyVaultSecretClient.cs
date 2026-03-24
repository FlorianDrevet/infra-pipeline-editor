using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Infrastructure.Services.KeyVault;

/// <summary>
/// Retrieves secrets from Azure Key Vault using <see cref="DefaultAzureCredential"/>.
/// </summary>
public sealed class KeyVaultSecretClient(DefaultAzureCredential credential) : IKeyVaultSecretClient
{
    /// <inheritdoc />
    public async Task<ErrorOr<string>> GetSecretAsync(
        string vaultUrl, string secretName, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = new SecretClient(new Uri(vaultUrl), credential);
            var response = await client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            return response.Value.Value;
        }
        catch (Exception)
        {
            return Errors.GitRepository.SecretRetrievalFailed();
        }
    }
}
