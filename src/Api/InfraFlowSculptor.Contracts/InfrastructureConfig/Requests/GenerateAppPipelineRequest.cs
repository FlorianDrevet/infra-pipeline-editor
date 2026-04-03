using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request to generate an application pipeline for a specific compute resource.</summary>
public class GenerateAppPipelineRequest
{
    /// <summary>Identifier of the infrastructure configuration containing the resource.</summary>
    [Required]
    public required Guid InfrastructureConfigId { get; init; }

    /// <summary>Identifier of the target compute resource (WebApp, FunctionApp, or ContainerApp).</summary>
    [Required]
    public required Guid ResourceId { get; init; }
}
