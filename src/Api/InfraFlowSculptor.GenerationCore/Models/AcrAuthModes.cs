namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Centralized constants for the supported Azure Container Registry (ACR)
/// authentication modes used across pipeline and Bicep generation engines.
/// </summary>
/// <remarks>
/// These constants mirror the values of the domain
/// <c>InfraFlowSculptor.Domain.Common.ValueObjects.AcrAuthMode.AcrAuthModeType</c>
/// enum but are intentionally duplicated here so that the generation layer
/// does not depend on the domain assembly.
/// </remarks>
public static class AcrAuthModes
{
    /// <summary>Uses a managed identity to pull images from Azure Container Registry.</summary>
    public const string ManagedIdentity = "ManagedIdentity";

    /// <summary>Uses Azure Container Registry admin credentials to pull images.</summary>
    public const string AdminCredentials = "AdminCredentials";

    /// <summary>
    /// All supported ACR authentication modes. Lookups are case-insensitive
    /// using <see cref="StringComparer.OrdinalIgnoreCase"/>.
    /// </summary>
    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ManagedIdentity,
        AdminCredentials,
    };
}
