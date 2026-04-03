using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateAppPipeline;

/// <summary>Command to generate application CI/CD pipeline YAML for a specific compute resource.</summary>
public record GenerateAppPipelineCommand(
    Guid InfrastructureConfigId,
    Guid ResourceId
) : ICommand<GenerateAppPipelineResult>;

/// <summary>Result of application pipeline generation, containing URIs to the generated YAML files.</summary>
/// <param name="FileUris">Map of relative file paths to their blob storage URIs.</param>
public record GenerateAppPipelineResult(
    IReadOnlyDictionary<string, Uri> FileUris);
