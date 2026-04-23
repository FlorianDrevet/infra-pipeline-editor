using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request to set the layout preset of a project (informative only in V1).</summary>
public sealed class SetProjectLayoutPresetRequest
{
    /// <summary>Preset name. Valid values: <c>AllInOne</c>, <c>SplitInfraCode</c>, <c>MultiRepo</c>, <c>Custom</c>.</summary>
    [Required]
    public required string Preset { get; init; }
}
