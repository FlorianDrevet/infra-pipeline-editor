namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

/// <summary>
/// Checks whether an Azure resource name is globally available.
/// The default implementation probes DNS (no Azure authentication required).
/// </summary>
public interface IAzureNameAvailabilityChecker
{
    /// <summary>
    /// Returns <c>true</c> when the checker supports the given resource type.
    /// </summary>
    bool Supports(string resourceType);

    /// <summary>
    /// Checks whether <paramref name="name"/> is available for the given resource type.
    /// Returns <see cref="AzureNameAvailabilityStatus.Unknown"/> when the check cannot be completed.
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
