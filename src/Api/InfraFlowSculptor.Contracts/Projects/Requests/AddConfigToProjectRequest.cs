using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request body for associating a configuration with a project.</summary>
public class AddConfigToProjectRequest
{
    /// <summary>The infrastructure configuration to associate.</summary>
    [Required, GuidValidation]
    public required string ConfigId { get; init; }
}
