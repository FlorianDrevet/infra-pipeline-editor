using System.Net;
using Azure;
using Azure.Security.KeyVault.Secrets;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using Microsoft.Extensions.Logging;

namespace InfraFlowSculptor.Infrastructure.Services.KeyVault;

/// <summary>
/// Centralized Azure Key Vault secret client backed by a single <see cref="SecretClient"/>.
/// </summary>
public sealed class KeyVaultSecretClient(SecretClient client, ILogger<KeyVaultSecretClient> logger) : IKeyVaultSecretClient
{
    /// <inheritdoc />
    public async Task<ErrorOr<string>> GetSecretAsync(
        string secretName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var response = await client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            return response.Value.Value;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (RequestFailedException exception)
        {
            logger.LogError(exception, "Failed to retrieve secret {SecretName} from Azure Key Vault.", secretName);
            return Errors.GitRepository.SecretRetrievalFailed();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to retrieve secret {SecretName} from Azure Key Vault.", secretName);
            return Errors.GitRepository.SecretRetrievalFailed();
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Success>> SetSecretAsync(
        string secretName, string value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await client.SetSecretAsync(secretName, value, cancellationToken);
            return Result.Success;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (RequestFailedException exception)
        {
            logger.LogError(exception, "Failed to store secret {SecretName} in Azure Key Vault.", secretName);
            return Errors.GitRepository.SecretStorageFailed(DescribeSecretStorageFailure(exception));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to store secret {SecretName} in Azure Key Vault.", secretName);
            return Errors.GitRepository.SecretStorageFailed();
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> DeleteSecretAsync(
        string secretName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await client.StartDeleteSecretAsync(secretName, cancellationToken);
            return Result.Deleted;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return Errors.GitRepository.SecretRetrievalFailed();
        }
    }

    private static string? DescribeSecretStorageFailure(RequestFailedException exception)
    {
        return (HttpStatusCode)exception.Status switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                => "the application is not allowed to write secrets to the configured Key Vault.",
            HttpStatusCode.NotFound
                => "the configured Key Vault could not be found.",
            _ => string.IsNullOrWhiteSpace(exception.Message) ? null : exception.Message,
        };
    }
}
