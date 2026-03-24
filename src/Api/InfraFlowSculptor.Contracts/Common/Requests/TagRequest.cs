using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Common.Requests;

/// <summary>A key/value Azure resource tag.</summary>
public class TagRequest
{
    /// <summary>Tag name (key).</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Tag value.</summary>
    [Required]
    public required string Value { get; init; }
}
