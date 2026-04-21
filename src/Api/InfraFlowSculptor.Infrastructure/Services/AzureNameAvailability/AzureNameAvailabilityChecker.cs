using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Identity;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.GenerationCore;
using Microsoft.Extensions.Logging;

namespace InfraFlowSculptor.Infrastructure.Services.AzureNameAvailability;

/// <summary>
/// Default <see cref="IAzureNameAvailabilityChecker"/> calling the Azure ARM REST API
/// for resource types that expose a <c>checkNameAvailability</c> endpoint.
/// </summary>
public sealed class AzureNameAvailabilityChecker(
    HttpClient httpClient,
    ILogger<AzureNameAvailabilityChecker> logger) : IAzureNameAvailabilityChecker
{
    private const string ManagementBaseUri = "https://management.azure.com";
    private const string ManagementScope = "https://management.azure.com/.default";

    private static readonly TokenCredential Credential = new DefaultAzureCredential();

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly Dictionary<string, ResourceTypeArmInfo> SupportedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [AzureResourceTypes.ContainerRegistry] = new(
            ApiVersion: "2023-07-01",
            ProviderPath: "Microsoft.ContainerRegistry",
            ArmType: AzureResourceTypes.ArmTypes.ContainerRegistry)
    };

    /// <inheritdoc />
    public bool Supports(string resourceType) => SupportedTypes.ContainsKey(resourceType);

    /// <inheritdoc />
    public async Task<AzureNameAvailabilityResult> CheckAsync(
        string resourceType,
        string subscriptionId,
        string name,
        CancellationToken cancellationToken = default)
    {
        if (!SupportedTypes.TryGetValue(resourceType, out var armInfo))
            return AzureNameAvailabilityResult.Unknown($"Resource type '{resourceType}' is not supported.");

        try
        {
            var token = await Credential
                .GetTokenAsync(new TokenRequestContext([ManagementScope]), cancellationToken)
                .ConfigureAwait(false);

            var url = $"{ManagementBaseUri}/subscriptions/{subscriptionId}/providers/{armInfo.ProviderPath}" +
                      $"/checkNameAvailability?api-version={armInfo.ApiVersion}";

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(
                    new CheckNameAvailabilityRequestBody(name, armInfo.ArmType),
                    options: JsonOptions)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

            using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                logger.LogWarning(
                    "Azure checkNameAvailability returned {StatusCode} for {ResourceType} '{Name}': {Body}",
                    (int)response.StatusCode, resourceType, name, body);
                return AzureNameAvailabilityResult.Unknown(
                    $"Azure ARM returned HTTP {(int)response.StatusCode}.");
            }

            var payload = await response.Content
                .ReadFromJsonAsync<CheckNameAvailabilityResponseBody>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (payload is null)
                return AzureNameAvailabilityResult.Unknown("Empty response from Azure ARM.");

            return payload.NameAvailable
                ? AzureNameAvailabilityResult.Available
                : new AzureNameAvailabilityResult(AzureNameAvailabilityStatus.Unavailable, payload.Reason, payload.Message);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex, "Azure checkNameAvailability timed out for {ResourceType} '{Name}'.", resourceType, name);
            return AzureNameAvailabilityResult.Unknown("Azure ARM request timed out.");
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Azure checkNameAvailability network failure for {ResourceType} '{Name}'.", resourceType, name);
            return AzureNameAvailabilityResult.Unknown($"Network error: {ex.Message}");
        }
        catch (AuthenticationFailedException ex)
        {
            logger.LogWarning(ex, "Azure checkNameAvailability authentication failure for {ResourceType} '{Name}'.", resourceType, name);
            return AzureNameAvailabilityResult.Unknown("Failed to acquire Azure management token.");
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Azure checkNameAvailability returned malformed JSON for {ResourceType} '{Name}'.", resourceType, name);
            return AzureNameAvailabilityResult.Unknown("Malformed response from Azure ARM.");
        }
    }

    private sealed record ResourceTypeArmInfo(string ApiVersion, string ProviderPath, string ArmType);

    private sealed record CheckNameAvailabilityRequestBody(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("type")] string Type);

    private sealed record CheckNameAvailabilityResponseBody(
        [property: JsonPropertyName("nameAvailable")] bool NameAvailable,
        [property: JsonPropertyName("reason")] string? Reason,
        [property: JsonPropertyName("message")] string? Message);
}
