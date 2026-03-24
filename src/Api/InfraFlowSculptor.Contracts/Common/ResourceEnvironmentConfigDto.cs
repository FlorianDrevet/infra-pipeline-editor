using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Common;

/// <summary>Represents a per-environment configuration entry for an Azure resource.</summary>
public class ResourceEnvironmentConfigEntry
{
    /// <summary>Name of the environment (must match an existing environment definition, e.g., "dev", "staging", "prod").</summary>
    [Required]
    public required string EnvironmentName { get; init; }

    /// <summary>Configuration key-value pairs for this environment (e.g., sku=Standard, capacity=1).</summary>
    [Required]
    public required Dictionary<string, string> Properties { get; init; }
}

/// <summary>Response DTO for a per-environment configuration entry on a resource.</summary>
public record ResourceEnvironmentConfigResponse(
    string EnvironmentName,
    IReadOnlyDictionary<string, string> Properties);
