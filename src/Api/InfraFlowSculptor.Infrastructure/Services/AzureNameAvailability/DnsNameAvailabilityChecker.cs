using System.Net;
using System.Net.Sockets;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.GenerationCore;
using Microsoft.Extensions.Logging;

namespace InfraFlowSculptor.Infrastructure.Services.AzureNameAvailability;

/// <summary>
/// <see cref="IAzureNameAvailabilityChecker"/> implementation that probes DNS to determine whether
/// an Azure resource name is already in use. Each globally-unique Azure resource type maps to a
/// well-known DNS suffix (e.g. <c>{name}.azurecr.io</c>). If the FQDN resolves, the name is taken;
/// otherwise it is considered available. This approach requires no Azure authentication.
/// </summary>
public sealed class DnsNameAvailabilityChecker(
    ILogger<DnsNameAvailabilityChecker> logger) : IAzureNameAvailabilityChecker
{
    private static readonly Dictionary<string, string> DnsSuffixes = new(StringComparer.OrdinalIgnoreCase)
    {
        [AzureResourceTypes.ContainerRegistry] = "azurecr.io",
        [AzureResourceTypes.StorageAccount] = "blob.core.windows.net",
        [AzureResourceTypes.KeyVault] = "vault.azure.net",
        [AzureResourceTypes.RedisCache] = "redis.cache.windows.net",
        [AzureResourceTypes.AppConfiguration] = "azconfig.io",
        [AzureResourceTypes.ServiceBusNamespace] = "servicebus.windows.net",
        [AzureResourceTypes.EventHubNamespace] = "servicebus.windows.net",
        [AzureResourceTypes.WebApp] = "azurewebsites.net",
        [AzureResourceTypes.FunctionApp] = "azurewebsites.net",
        [AzureResourceTypes.SqlServer] = "database.windows.net",
    };

    /// <inheritdoc />
    public bool Supports(string resourceType) => DnsSuffixes.ContainsKey(resourceType);

    /// <inheritdoc />
    public async Task<AzureNameAvailabilityResult> CheckAsync(
        string resourceType,
        string subscriptionId,
        string name,
        CancellationToken cancellationToken = default)
    {
        if (!DnsSuffixes.TryGetValue(resourceType, out var suffix))
            return AzureNameAvailabilityResult.Unknown($"Resource type '{resourceType}' is not supported.");

        var fqdn = $"{name}.{suffix}";

        try
        {
            var addresses = await Dns.GetHostAddressesAsync(fqdn, cancellationToken).ConfigureAwait(false);

            return addresses.Length > 0
                ? new AzureNameAvailabilityResult(
                    AzureNameAvailabilityStatus.Unavailable,
                    "AlreadyExists",
                    $"The name '{name}' is already in use ({fqdn} resolves).")
                : AzureNameAvailabilityResult.Available;
        }
        catch (SocketException ex) when (ex.SocketErrorCode is SocketError.HostNotFound or SocketError.NoData)
        {
            // NXDOMAIN / no data → the FQDN does not exist → the name is available.
            return AzureNameAvailabilityResult.Available;
        }
        catch (SocketException ex)
        {
            logger.LogWarning(ex, "DNS lookup failed for {Fqdn} (SocketError={SocketError}).", fqdn, ex.SocketErrorCode);
            return AzureNameAvailabilityResult.Unknown($"DNS lookup failed for {fqdn}.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unexpected error during DNS lookup for {Fqdn}.", fqdn);
            return AzureNameAvailabilityResult.Unknown($"DNS lookup error for {fqdn}.");
        }
    }
}
