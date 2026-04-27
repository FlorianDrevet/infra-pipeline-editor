namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Centralized constants for the supported compute resource deployment modes
/// used across pipeline and Bicep generation engines.
/// </summary>
/// <remarks>
/// These constants mirror the values of the domain
/// <c>InfraFlowSculptor.Domain.Common.ValueObjects.DeploymentMode.DeploymentModeType</c>
/// enum but are intentionally duplicated here so that the generation layer
/// does not depend on the domain assembly.
/// </remarks>
public static class DeploymentModes
{
    /// <summary>Traditional code deployment with a runtime stack.</summary>
    public const string Code = "Code";

    /// <summary>Container-based deployment pulling an image from a registry.</summary>
    public const string Container = "Container";

    /// <summary>
    /// All supported deployment modes. Lookups are case-insensitive
    /// using <see cref="StringComparer.OrdinalIgnoreCase"/>.
    /// </summary>
    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Code,
        Container,
    };
}
