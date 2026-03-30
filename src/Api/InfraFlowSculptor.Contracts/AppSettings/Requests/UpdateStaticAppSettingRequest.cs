using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.AppSettings.Requests;

/// <summary>Request to update a static app setting on a compute resource.</summary>
public class UpdateStaticAppSettingRequest
{
    /// <summary>The new environment variable name (e.g., KEYVAULT_URI).</summary>
    [Required]
    [MaxLength(256)]
    public required string Name { get; init; }

    /// <summary>Per-environment values for the static setting.</summary>
    [Required]
    public required Dictionary<string, string> EnvironmentValues { get; init; }
}
