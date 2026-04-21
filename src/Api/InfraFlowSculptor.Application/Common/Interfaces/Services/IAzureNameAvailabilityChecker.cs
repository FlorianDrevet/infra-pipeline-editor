namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

/// <summary>
/// Calls the Azure ARM REST API to check if a resource name is globally available
/// (DNS-unique resources such as ContainerRegistry, StorageAccount, KeyVault, etc.).
/// </summary>
public interface IAzureNameAvailabilityChecker
{
    /// <summary>
    /// Returns <c>true</c> when the checker supports the given resource type.
    /// </summary>
    bool Supports(string resourceType);

    /// <summary>
    /// Checks whether <paramref name="name"/> is available in the given <paramref name="subscriptionId"/>.
    /// Returns <see cref="AzureNameAvailabilityStatus.Unknown"/> when the call fails (network, auth, throttling).
    /// </summary>
    Task<AzureNameAvailabilityResult> CheckAsync(
        string resourceType,
        string subscriptionId,
        string name,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an Azure name availability check.
/// </summary>
public sealed record AzureNameAvailabilityResult(
    AzureNameAvailabilityStatus Status,
    string? Reason,
    string? Message)
{
    /// <summary>Cached "Available" result.</summary>
    public static AzureNameAvailabilityResult Available { get; } =
        new(AzureNameAvailabilityStatus.Available, null, null);

    /// <summary>Builds an "Unknown" result with the given diagnostic message.</summary>
    public static AzureNameAvailabilityResult Unknown(string message) =>
        new(AzureNameAvailabilityStatus.Unknown, null, message);
}

/// <summary>Possible outcomes of an Azure name availability check.</summary>
public enum AzureNameAvailabilityStatus
{
    /// <summary>The name is available.</summary>
    Available,

    /// <summary>The name is not available (already taken or invalid per Azure).</summary>
    Unavailable,

    /// <summary>The check could not be completed (network/auth/throttling).</summary>
    Unknown
}
